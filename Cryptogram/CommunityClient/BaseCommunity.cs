using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using NBitcoin;
using System.Text;
using System.IO.IsolatedStorage;
using System.Linq;

namespace CommunityClient
{
    public partial class BaseCommunity
    {
        public BaseCommunity()
        {
        }

        /// <summary>
        /// is the post where community information and settings are saved. This post is the basis of the community, inside there is a tree structure with the posts that form the topics and to follow the posts in response to the topics and other levels of posts if required. This post has ID 0 as it is also the first in the data stream representing the community saved on the cloud
        /// </summary>
        [XmlIgnore]
        public Post Root;
        public const byte CurrentVersion = 0;
        internal readonly IsolatedStorageFile IsoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, null, null);
        internal string Directory()
        {
            return CommunityId.ToHex();
        }
        private void LoadPosts()
        {
            var posts = Post.LoadAllPostsLocal(this);
            AddPosts(posts, false);
        }

        [XmlIgnore]
        public DateTime LastUpdateFromServer = DateTime.UtcNow;

        public void RefreshFromServer()
        {
            if ((DateTime.UtcNow - LastUpdateFromServer).TotalMinutes > 1) // Avoid and continuous refresh in a short amount of time (does not overload the server with unnecessary requests)
            {
                LastUpdateFromServer = DateTime.UtcNow;
                Communication.GetPosts(this);
            }
        }

        public void AddPosts(Dictionary<long, Post> posts, bool isOrphans = false)
        {
            var list = new List<Post>(posts.Values);
            list.Sort((x, y) => x.Id.CompareTo(y.Id));
            foreach (var post in list)
            {
                if (AddPost(post))
                    _ = posts.Remove(post.Id);

            }
            if (!isOrphans)
                AddPosts(PostOrphans, true);
        }

        /// <summary>
        /// Add a post to the community
        /// </summary>
        /// <param name="post"></param>
        /// <returns>Returns false if the post is orphaned, true otherwise</returns>
        public bool AddPost(Post post)
        {
#if DEBUG
            if (post.Id < 0)
            {
                System.Diagnostics.Debugger.Break(); // It is not possible to add posts without valid Id
                post.Delete(this);
                return false;
            }
#endif
            LastPost = LastPost == null ? post : post.Id > LastPost.Id ? post : LastPost;
            if (post.Id == 0) // Post 0 is the root, it is at the base of the community and holds the data relating to the community, so the post at the base of the community must be replaced with this;
            {
                post.ReplyToId = -1;
                if (Root == null)
                    Root = post;
                else
                    Root.Update(this, post.Data);
            }
            if (AllPosts.ContainsKey(post.Id))
            {
                if (post != Root) // Because in this case the update has already been done above
                {
                    AllPosts[post.Id].Update(this, post.Data);
                }
            }
            else
                AllPosts.Add(post.Id, post);
            if (post.ReplyToId == -1) // -1 = post Root
            {
                return true;
            }
            else
            {
                if (AllPosts.TryGetValue(post.ReplyToId, out var parent))
                {
                    if (!parent.SubPosts.Contains(post))
                        Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() => parent.SubPosts.Insert(0, post));
                    return true;
                }
                else
                   if (!PostOrphans.ContainsKey(post.Id))
                    PostOrphans.Add(post.Id, post);
            }
            return false;
        }


        public static void ImportPosts(byte[] id, byte[] export)
        {
            var community = Social.FindSubscriptions(id);
            if (community != null)
                community.ImportPosts(export);
        }

        public void ImportPosts(byte[] export)
        {
            var type = (Post.ExportType)export[0];
            //List<Tuple<long, byte[]>> list = new List<Tuple<long, byte[]>>();
            if (type == Post.ExportType.ExportFlags)
            {
                var length = 8 + 49; // postId + flags
                for (var p = 1; p < export.Length; p += length)
                {
                    var postId = BitConverter.ToInt64(export, p);
                    var flags = export.Skip(p + 8).Take(49);
                    ImportPost(type, postId, flags);
                }
            }
            else if (type == Post.ExportType.ExportData || type == Post.ExportType.ExportRecord)
            {
                var p = 1;
                while (p < export.Length)
                {
                    var postId = BitConverter.ToInt64(export, p);
                    p += 8;
                    var length = BitConverter.ToInt32(export, p);
                    p += 4;
                    var data = export.Skip(p).Take(length);
                    p += length;
                    ImportPost(type, postId, data);
                }
#if DEBUG
                if (p != export.Length)
                    System.Diagnostics.Debugger.Break(); // export is corrupt
#endif
            }
        }

        private void ImportPost(Post.ExportType type, long postId, byte[] export)
        {
            if (!AllPosts.TryGetValue(postId, out var post))
            {
                bool successful;
                if (postId == 0)
                {
                    post = Root;
                    successful = true;
                }
                else
                    post = new Post(this, postId, type, export, out successful);
                if (successful)
                {
                    post.SavePostLocal(this);
                    AddPost(post);
                }
            }
            post.Import(this, type, export);
        }

        /// <summary>
        /// Create a new topic
        /// </summary>
        /// <param name="editableProperties"></param>
        /// <param name="replyTo"></param>
        public void CreateNewPost(EditableProperties editableProperties, Post replyTo = null)
        {
            var post = new Post(this, Social.Context.My.GetId(), editableProperties, replyTo);
        }

        /// <summary>
        /// Create a new topic (a topic is simply a post with no reference to other posts), then it will appear in the root
        /// </summary>
        /// <param name="text"></param>
        /// <param name="description"></param>
        public void CreateNewTopic(string text, string description)
        {
            var editable = new EditableProperties() { Text = text, Description = description };
            CreateNewPost(editable);
        }
        internal Post LastPost;
        internal long PostIdQueueLastTrain
        {
            get
            {
                var orderedPosts = AllPosts.OrderBy(x => x.Value.Id).Select(x => x.Value);
                Post lastPost = null;
                foreach (var post in orderedPosts.Reverse())
                {
                    if (lastPost != null)
                    {
                        if (post.NextPostId != lastPost.Id)
                            return lastPost.Id;
                    }
                    lastPost = post;
                }
                return -1;
            }
        }

        private readonly Dictionary<long, Post> PostOrphans = new Dictionary<long, Post>();
        private readonly Dictionary<long, Post> AllPosts = new Dictionary<long, Post>();

        //===================================================
        public void Initialize(string emoticon, string text, string description, byte[] privateKey, Uri entryPoint, byte[] serverPublicKey = null, ModalitySubscription modality = ModalitySubscription.Default)
        {
            EntryPoint = entryPoint;
            ServerPublicKey = serverPublicKey ?? Convert.FromBase64String(DefaultServerPublicKey);
            Key = new Key(privateKey);
            var properties = new EditableProperties
            {
                Emoticon = emoticon,
                Text = text,
                Description = description
            };
            Root = new Post(this, Social.Context.My.GetId(), properties) { Id = 0 };
            Root.Data = properties.Data;
            Modality = modality;
        }
        [XmlIgnore]
        public const string DefaultServerPublicKey = "AlLbA7nCRN3R49zcIadKVU9wdvFV4m6yKwL0EokLKBag";
        [XmlIgnore]
        virtual public CommunityType Type { get; }
        [XmlIgnore] // EntryPoint is contained in the Address
        public static Uri EntryPoint;
        private static byte[] EntryPointBytes
        {
            get
            {
                if (EntryPoint == null)
                    return Array.Empty<byte>();
                else
                {
                    //if the entry point is an IP address then we only use 4 bytes to represent it
                    if (System.Net.IPAddress.TryParse(EntryPoint.AbsoluteUri, out var address))
                    {
                        return address.GetAddressBytes();
                    }
                    return Encoding.ASCII.GetBytes(EntryPoint.ToString());
                }
            }
        }
        [XmlIgnore]
        public byte[] ServerPublicKey;
        [XmlIgnore]  // PrivateKey is contained in the Address
        public string PrivateKey { get => Key == null ? null : Convert.ToBase64String(Key.ToBytes()); set { Key = new Key(Convert.FromBase64String(value)); LoadPosts(); } }
        [XmlIgnore]
        public Key Key;
        [XmlIgnore]
        public byte[] PrivateKeyBytes => Key.ToBytes();
        [XmlIgnore]
        public byte[] CommunityId => NBitcoin.Crypto.Hashes.DoubleSHA256(PrivateKeyBytes).ToBytes();
        [XmlIgnore]
        public ModalitySubscription Modality;
        public enum ModalitySubscription : byte
        {
            Default = 0,
            NoAdmin = 1,
            NoSubscriptionRequired = 2,
            WitInvitationOnly = 4,
        }
        public byte[] Encrypt(byte[] data)
        {
            return Key.PubKey.Encrypt(data);
        }
        public byte[] Decrypt(byte[] data)
        {
            return Key.Decrypt(data);
        }
        public string Address
        {
            get { return Convert.ToBase64String(GetAddress()); }
            set
            {
                var data = Convert.FromBase64String(value).Split();
                if (data[0].Length == 0)
                    EntryPoint = null;
                else if (data[0].Length == 4) //If it is 4 bytes then it represents an ip address
                    EntryPoint = new Uri(new System.Net.IPAddress(data[0]).ToString());
                else
                    EntryPoint = new Uri(Encoding.ASCII.GetString(data[0]));
                ServerPublicKey = data[1];
                PrivateKey = Convert.ToBase64String(data[2]);
            }
        }

        private byte[] GetAddress()
        {
            return Bytes.Join(EntryPointBytes, ServerPublicKey, PrivateKeyBytes);
        }
        public List<byte[]> Export()
        {
            return new List<byte[]> { new byte[] { (byte)Type }, new byte[] { (byte)Modality } };
        }
        public static string GeneratePrivateKey()
        {
            var key = new Key();
            return Convert.ToBase64String(key.ToBytes());
        }
        public uint TotalUsers { get; set; }
        [XmlIgnore]
        static public ulong MyId => Social.Context.My.GetId();
        public Role MyRole;
        public enum Role
        {
            Admin,
            Moderator,
            User,
            Pending,
            Banned,
        }

        //============ START commands to communicate to the cloud ============
        [XmlIgnore]
        internal static readonly List<Tuple<byte[], byte[]>> idToPrivateKeyTable = new List<Tuple<byte[], byte[]>>();
        public static bool GetCommunityFromCloud(string privateKeyBase64, Post lastPost) => GetCommunityFromCloud(privateKeyBase64.Base64ToBytes(), lastPost);
        public static bool GetCommunityFromCloud(byte[] privateKey, Post lastPost)
        {
            try
            {
                var key = new Key(privateKey); // validate
                var id = NBitcoin.Crypto.Hashes.DoubleSHA256(privateKey).ToBytes();
                idToPrivateKeyTable.Add(new Tuple<byte[], byte[]>(id, privateKey));
                if (lastPost == null)
                    Communication.JoinAndSend(id, Communication.Command.GetCommunity);
                else
                    Communication.JoinAndSend(id, Communication.Command.GetCommunity, lastPost.NextPostId.GetBytes());
                return true;
            }
            catch (Exception)
            {
                return false; // private key is not valid
            }
        }

        public void GetCommunityFromCloud()
        {
            // Avoid unnecessary traffic by avoiding frequent updates in a short amount of time
            if ((DateTime.UtcNow - LastGetCommunityFromCloud).TotalMinutes > 1)
            {
                LastGetCommunityFromCloud = DateTime.UtcNow;
                GetCommunityFromCloud(PrivateKeyBytes, LastPost);
            }
        }

        internal DateTime LastGetCommunityFromCloud; // Necessary to avoid generating unnecessary traffic with frequent updates in a short period of time
        //============ END  commands to communicate to the cloud ============

        internal void SaveToCloud()
        {
            Root.SaveToCloud(this);
        }

        internal void SaveLocal()
        {
            //GenerateFirstPost();
            //FirstPost.SavePostLocal(this);
            Root.SavePostLocal(this);
            Social.Context.SecureStorage.ObjectStorage.SaveObject(this, CommunityId.ToHex());
        }

        /// <summary>
        /// Add a community, it can be saved on the cloud server if it is new
        /// </summary>
        /// <param name="saveRemote">Save the new community to the cloud server</param>
        public void AddToCollection(bool saveRemote = true)
        {
            Social.AddCommunity(this, saveRemote);
        }

        public void Dispose()
        {
            IsoStore.Dispose();
        }
    }

    public enum CommunityType : byte
    {
        Wall,
        Group,
        Page,
    }
}
