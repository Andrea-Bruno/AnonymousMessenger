using EncryptedMessaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace CommunityClient
{
    public class Post : EditableProperties
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        //internal void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Post()
        {
        }

        /// <summary>
        /// Used for loading post saved locally
        /// </summary>
        /// <param name="baseCommunity">The community the post belongs to</param>
        /// <param name="id">Id</param>
        /// <param name="type">The format of the export data parameter</param>
        /// <param name="exportData">The binary data that is used to generate the post</param>
        /// <param name="successful">Indicates whether the post creation was successful</param>
        public Post(BaseCommunity baseCommunity, long id, ExportType type, byte[] exportData, out bool successful)
        /// <param name="id"></param>
        {
            Id = id;
            successful = Import(baseCommunity, type, exportData);
        }

        /// <summary>
        /// Used to create a new post and save local and to cloud
        /// </summary>
        /// <param name="baseCommunity">The community the post belongs to</param>
        /// <param name="author">The author of the post </param>
        /// <param name="editableProperties">The editable components of the post</param>
        /// <param name="replyTo">If set it indicates that it is a post in reply to another</param>
        public Post(BaseCommunity baseCommunity, ulong author, EditableProperties editableProperties, Post replyTo = null)
        {
            Time = DateTime.UtcNow;
            Author = author;
            Data = editableProperties.Data;
            if (replyTo != null)
            {
                ReplyToId = replyTo.Id;
                //Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() => replyTo.SubPosts.Insert(0, this));
            }
            SaveToCloud(baseCommunity);
        }

        public readonly ObservableCollection<Post> SubPosts = new ObservableCollection<Post>();
        private DateTime LastUpdateFlagsOfSubPosts;
        public void UpdateFlagsOfSubPosts(BaseCommunity baseCommunity)
        {
            if (SubPosts.Count > 0)
            {
                if ((DateTime.UtcNow - LastUpdateFlagsOfSubPosts).TotalMinutes > 60)
                {
                    LastUpdateFlagsOfSubPosts = DateTime.UtcNow;
                    var postsLimit = 100;
                    var n = SubPosts.Count < postsLimit ? SubPosts.Count : postsLimit;
                    var idsConcatBytes = new byte[n * 8];
                    for (var i = 0; i < n; i++)
                    {
                        SubPosts[i].Id.GetBytes().CopyTo(idsConcatBytes, i * 8);
                    }
                    Communication.GetFlags(baseCommunity, idsConcatBytes);
                }
            }
        }

        public byte Version;
        public DateTime Time { get; set; }
        public string TimeLiteral { get { return EncryptedMessaging.Functions.DateToRelative(Time); } }
        private long HashCode;
        internal void UpdateTimeLiteral()
        {
            var timeLiteral = TimeLiteral;
            if (HashCode.GetHashCode() != timeLiteral.GetHashCode() )
                OnPropertyChanged(nameof(TimeLiteral));
        }

        public DateTime LastReply
        {
            get
            {
                var result = Time;
                foreach (var post in SubPosts)
                {
                    if (post.LastReply > result)
                        result = post.LastReply;
                }
                return result;
            }
        }
        private byte[] Timestamp() { return BitConverter.GetBytes(CommunicationChannel.Converter.ToUnixTimestamp(Time)); }
        public ulong Author;
        public string UserName { get { return Social.Context.Contacts.Pseudonym(Author); } }
        //public PostType Type;
        public long ReplyToId;

        /// <summary>
        /// It is the post ID, -1 = not assigned, 0 = is the first post, the one that sets the community,> 0 represents the position in the stream on the post server, this value is assigned by the server, so initially all the posts created have Id -1, then the server assigns it an Id as soon as it receives it. If you are looking for the community ID, you need to see the CommunityId property
        /// </summary>
        public long Id = -1;
        public long NextPostId => Id == -1 ? -1 : Id + RecordLength;
        /// <summary>
        /// ================ end   - user for rendering ================
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public byte[] Export(BaseCommunity baseCommunity, ExportType type)
        {
            void Copy(byte[] source, ref byte[] target, ref int position)
            {
                Array.Resize(ref target, position + source.Length);
                source.CopyTo(target, position); position += source.Length;
            }
            var postData = Array.Empty<byte>();
            var p = 0;
            if (type != ExportType.ExportData)
            {
                if (type != ExportType.ExportFlags)
                {
                    Copy(RecordLength.GetBytes(), ref postData, ref p);
                    Copy(new byte[] { Version }, ref postData, ref p);
                    Copy(Timestamp(), ref postData, ref p);
                    Copy(Author.GetBytes(), ref postData, ref p);
                    Copy(FlagsNotUsed.GetBytes(), ref postData, ref p);
                }
                Copy(Flags.GetBytes(), ref postData, ref p);
                Copy(ReplyToId.GetBytes(), ref postData, ref p);
                Copy(Like.GetBytes(), ref postData, ref p);
                Copy(Dislike.GetBytes(), ref postData, ref p);
                Copy(Love.GetBytes(), ref postData, ref p);
                Copy(Haha.GetBytes(), ref postData, ref p);
                Copy(Yay.GetBytes(), ref postData, ref p);
                Copy(Wow.GetBytes(), ref postData, ref p);
                Copy(Sad.GetBytes(), ref postData, ref p);
                Copy(Angry.GetBytes(), ref postData, ref p);
            }
            Copy(new byte[] { ProgressiveRevision }, ref postData, ref p);
            if (type != ExportType.ExportFlags)
            {
                var data = GetDataEncrypted(baseCommunity);
                if (type != ExportType.ExportData)
                {
                    Copy(BitConverter.GetBytes(data.Length), ref postData, ref p);
                }
                Copy(data, ref postData, ref p);
            }
            return postData;
        }

        public bool Import(BaseCommunity baseCommunity, ExportType type, byte[] export)
        {
            var progressiveRevision = ProgressiveRevision;
            var p = 0;
            if (type != ExportType.ExportData)
            {
                if (type != ExportType.ExportFlags)
                {
                    RecordLength = BitConverter.ToInt32(export, p); p += 4; // [0]
                    Version = export[p]; p++; // [4]
                    Time = CommunicationChannel.Converter.FromUnixTimestamp(export.Skip(p).Take(4)); p += 4; // [5]
                    Author = BitConverter.ToUInt64(export, p); p += 8; // [9]
                    FlagsNotUsed = BitConverter.ToUInt32(export, p); p += 4; // [17]
                }
                Flags = BitConverter.ToUInt64(export, p); p += 8; // [21]
                ReplyToId = BitConverter.ToInt64(export, p); p += 8; // [29]
                Like = BitConverter.ToUInt32(export, p); p += 4; // [37]
                Dislike = BitConverter.ToUInt32(export, p); p += 4;// [41]
                Love = BitConverter.ToUInt32(export, p); p += 4;// [45]
                Haha = BitConverter.ToUInt32(export, p); p += 4;// [49]
                Yay = BitConverter.ToUInt32(export, p); p += 4;// [53]
                Wow = BitConverter.ToUInt32(export, p); p += 4;// [57]
                Sad = BitConverter.ToUInt32(export, p); p += 4;// [61]
                Angry = BitConverter.ToUInt32(export, p); p += 4;// [65]
            }
            ProgressiveRevision = export[p]; p++;
            if (type != ExportType.ExportFlags)
            {
                uint? datalen = null;
                datalen = BitConverter.ToUInt32(export, p); p += 4;
                var dataPostEncrypted = new byte[export.Length - p];
                if (datalen != null)
                {
                    if (datalen != dataPostEncrypted.Length)
                    {
#if DEBUG
                        System.Diagnostics.Debugger.Break(); // It must be equal
                        return false;
#endif
                    }
                }
                export.Skip(p).CopyTo(dataPostEncrypted, 0);
                return SetDataEncrypted(baseCommunity, dataPostEncrypted);
            }
            if (type == ExportType.ExportFlags && progressiveRevision != ProgressiveRevision) // The post has been edited 
                Communication.GetPostData(baseCommunity, Id);
            return true;
        }

        public enum ExportType : byte
        {
            ExportFlags,
            ExportData,
            ExportRecord
        }
        /// <summary>
        /// Record length on the server (new records cannot be larger)
        /// </summary>
        private int RecordLength;
        private uint FlagsNotUsed;
        private ulong Flags;
        public byte ProgressiveRevision;
        private uint like;
        private uint dislike;
        private uint love;
        private uint haha;
        private uint yay;
        private uint wow;
        private uint sad;
        private uint angry;

        /// <summary>
        /// Counters emoticons
        /// </summary>
        public uint Like { get => like; set { like = value; OnPropertyChanged(nameof(Like)); } }
        public uint Dislike { get => dislike; set { dislike = value; OnPropertyChanged(nameof(Dislike)); } }
        public uint Love { get => love; set { love = value; OnPropertyChanged(nameof(Love)); } }
        public uint Haha { get => haha; set { haha = value; OnPropertyChanged(nameof(Haha)); } }
        public uint Yay { get => yay; set { yay = value; OnPropertyChanged(nameof(Yay)); } }
        public uint Wow { get => wow; set { wow = value; OnPropertyChanged(nameof(Wow)); } }
        public uint Sad { get => sad; set { sad = value; OnPropertyChanged(nameof(Sad)); } }
        public uint Angry { get => angry; set { angry = value; OnPropertyChanged(nameof(Angry)); } }

        /// <summary>
        /// My action on emoticons
        /// </summary>
        public bool ILike { get => (MyActionInfo & 0x10000000) != 0; set { MyActionInfo ^= 0x10000000; OnPropertyChanged(nameof(ILike)); } }
        public bool IDislike { get => (MyActionInfo & 0x01000000) != 0; set { MyActionInfo ^= 0x01000000; OnPropertyChanged(nameof(IDislike)); } }
        public bool ILove { get => (MyActionInfo & 0x00100000) != 0; set { MyActionInfo ^= 0x00100000; OnPropertyChanged(nameof(ILove)); } }
        public bool IHaha { get => (MyActionInfo & 0x00010000) != 0; set { MyActionInfo ^= 0x00010000; OnPropertyChanged(nameof(IHaha)); } }
        public bool IYay { get => (MyActionInfo & 0x00001000) != 0; set { MyActionInfo ^= 0x00001000; OnPropertyChanged(nameof(IYay)); } }
        public bool IWow { get => (MyActionInfo & 0x00000100) != 0; set { MyActionInfo ^= 0x00000100; OnPropertyChanged(nameof(ILike)); } }
        public bool ISad { get => (MyActionInfo & 0x00000010) != 0; set { MyActionInfo ^= 0x00000010; OnPropertyChanged(nameof(ISad)); } }
        public bool IAngry { get => (MyActionInfo & 0x00000001) != 0; set { MyActionInfo ^= 0x00000001; OnPropertyChanged(nameof(IAngry)); } }

        private int MyActionInfo;

        public static Dictionary<long, Post> LoadAllPostsLocal(BaseCommunity baseCommubity)
        {
            var result = new Dictionary<long, Post>();
            var directory = baseCommubity.Directory();
            if (baseCommubity.IsoStore.DirectoryExists(directory))
            {
                foreach (var file in baseCommubity.IsoStore.GetFileNames(Path.Combine(directory, "*.post")))
                {
#if DEBUG
                    if (Social.Reset)
                    {
                        baseCommubity.IsoStore.DeleteFile(Path.Combine(directory, file));
                        continue;
                    }
#endif
                    var id = long.Parse(Path.GetFileNameWithoutExtension(file));
                    var fileError = false;
                    using (var stream = baseCommubity.IsoStore.OpenFile(Path.Combine(directory, file), FileMode.Open, FileAccess.Read))
                    {
                        if (stream.Length != 0)
                        {
                            using (var memoryStream = new MemoryStream(new byte[stream.Length]))
                            {
                                stream.CopyTo(memoryStream);
                                var data = memoryStream.ToArray();
                                var MyActionInfo = BitConverter.ToInt32(data, 0);
                                var post = new Post(baseCommubity, id, ExportType.ExportRecord, data.Skip(4), out var successful)
                                {
                                    MyActionInfo = MyActionInfo
                                };
                                if (successful)
                                    result.Add(post.Id, post);
                                else
                                    fileError = true;
                            }
                        }
                    }
                    if (fileError)
                        baseCommubity.IsoStore.DeleteFile(Path.Combine(directory, file));
                }
            }
            return result;
        }
        /// <summary>
        /// Save the post locally, in order to be reloaded when the application restarts, without having to request the cloud
        /// </summary>
        /// <param name="baseCommunity">The community the post belongs to</param>
        public void SavePostLocal(BaseCommunity baseCommunity)
        {
#if DEBUG
            if (Id < 0)
                System.Diagnostics.Debugger.Break(); // It is not allowed to save posts without being assigned an id
#endif

            var postData = Export(baseCommunity, ExportType.ExportRecord);
            var directory = baseCommunity.Directory();
            if (!baseCommunity.IsoStore.DirectoryExists(directory))
                baseCommunity.IsoStore.CreateDirectory(directory);
            var fileName = Path.Combine(directory, Id.ToString() + ".post");
            var isUpdate = baseCommunity.IsoStore.FileExists(fileName);
            using (var file = baseCommunity.IsoStore.OpenFile(fileName, FileMode.Create))
            {
                var data = BitConverter.GetBytes(MyActionInfo).Combine(postData); // the first 4 bytes contain the MyActionInfo which is information for local use. The remainder is the post in the same format that is on the cloud
                file.Write(data, 0, data.Length);
            }
            if (!isUpdate)
                baseCommunity.AddPost(this);
        }

        public void Update(BaseCommunity baseComunity, byte[] data)
        {
            Data = data;
        }

        public void SaveToCloud(BaseCommunity baseCommunity)
        {
            // Action to be taken when the server responds by assigning the post id
            Action<long, int> onServerAnswer = null;
            if (Id >= 0)
                SavePostLocal(baseCommunity);
            else
                onServerAnswer = (id, recordLength) =>
                    {
                        Id = id;
                        RecordLength = recordLength;
                        SavePostLocal(baseCommunity);
                    };
            Communication.SetPost(baseCommunity, onServerAnswer, Id, ExportType.ExportData, Export(baseCommunity, ExportType.ExportData));
        }

        public void Delete(BaseCommunity baseCommunity)
        {
            var fileName = Path.Combine(baseCommunity.Directory(), Id.ToString() + ".post");
            if (baseCommunity.IsoStore.FileExists(fileName))
                baseCommunity.IsoStore.DeleteFile(fileName);
            if (baseCommunity.Root != null)
                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() => baseCommunity.Root.SubPosts.Remove(this)); // Use the main thread to not generate errors when updating the GUI
            if (Id >= 0)
                Communication.DeletePost(baseCommunity, this);
        }
    }
}
