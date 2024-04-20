using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace CommunityClient
{
    public class EditableProperties : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Emotion is composed by 2 char
        /// </summary>
        [XmlIgnore]
        public string Emoticon { get => emoticon; set { emoticon = value; OnPropertyChanged(nameof(Emoticon)); } }
        [XmlIgnore]
        public string Text { get => text; set { text = value; OnPropertyChanged(nameof(Text)); } }
        [XmlIgnore]
        public string Description { get => description; set { description = value; OnPropertyChanged(nameof(Description)); } }

        [XmlIgnore]
        public Xamarin.Forms.ImageSource Picture { get; set; }

        [XmlIgnore]
        public byte[] Audio { get; set; }
        [XmlIgnore]
        public PostType TypePost;
        private string emoticon;
        private string text;
        private string description;

        public byte[] Data
        {
            get
            {
                {
                    //	var dataList = new List<byte[]> { ((uint)Post.PostType.Emoticon).GetBytes(), Emoticon.GetBytes(), ((uint)Post.PostType.Text).GetBytes(), Encrypt(Name.GetBytes()), ((uint)Post.PostType.Text).GetBytes(), Encrypt(Description.GetBytes()) };
                    var dataList = new List<byte[]>();
                    if (Emoticon != null)
                    {
                        TypePost = PostType.Emoticon;
                        dataList.Add(((ushort)PostType.Emoticon).GetBytes());
                        dataList.Add(Emoticon.GetBytes());
                    }
                    if (Text != null)
                    {
                        TypePost = PostType.Text;
                        dataList.Add(((ushort)PostType.Text).GetBytes());
                        dataList.Add(Text.GetBytes());
                    }
                    if (Description != null)
                    {
                        TypePost = PostType.Description;
                        dataList.Add(((ushort)PostType.Description).GetBytes());
                        dataList.Add(Description.GetBytes());
                    }
                    if (Picture != null)
                    {
                        TypePost = PostType.Picture;
                        //dataList.Add(((uint)Post.PostType.Picture).GetBytes());
                        //dataList.Add(ReadFully(ReadFully( Picture ));
                    }
                    if (dataList.Count == 2)
                        return System.BitConverter.GetBytes((ushort)TypePost).Combine(dataList[1]);
                    else
                        TypePost = PostType.Composed;
                    return System.BitConverter.GetBytes((ushort)PostType.Composed).Combine(EncryptedMessaging.Functions.JoinData(false, dataList.ToArray()));
                }
            }
            set
            {
                TypePost = (PostType)System.BitConverter.ToUInt16(value, 0);
                var data = value.Skip(2);
                if (TypePost == PostType.Emoticon)
                    Emoticon = Encoding.Unicode.GetString(data);
                if (TypePost == PostType.Text)
                    Text = Encoding.Unicode.GetString(data);
                if (TypePost == PostType.Description)
                    Description = Encoding.Unicode.GetString(data);
                if (TypePost == PostType.Composed)
                {
                    List<byte[]> dataArray;
                    try
                    {
                        dataArray = EncryptedMessaging.Functions.SplitData(data, false);
                    }
                    catch (System.Exception)
                    {
                        return; // The data packet does not respect the standard (with an integer of 4 bytes as a separator)
                    }
                    var p = 0;
                    while (p < dataArray.Count - 1)
                    {
                        var typeBytes = dataArray[p];
                        if (typeBytes.Length != 2)
                            break;
                        var type = (PostType)System.BitConverter.ToUInt16(dataArray[p], 0);
                        p++;
                        if (type == PostType.Emoticon)
                            Emoticon = Encoding.Unicode.GetString(dataArray[p]);
                        else if (type == PostType.Text)
                            Text = Encoding.Unicode.GetString(dataArray[p]);
                        else if (type == PostType.Description)
                            Description = Encoding.Unicode.GetString(dataArray[p]);
                        p++;
                    }
                }
            }
        }
        private static byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream((int)input.Length))
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }

        }
        //public abstract void AddToCollection();
        //internal abstract void SaveToCloud();
        //internal abstract void SaveLocal();

        public byte[] GetDataEncrypted(BaseCommunity baseCommunity)
        {
            return baseCommunity.Key.PubKey.Encrypt(Data);
        }
        public bool SetDataEncrypted(BaseCommunity baseCommunity, byte[] encrypted)
        {
            try
            {
                Data = baseCommunity.Key.Decrypt(encrypted);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        public enum PostType : ushort
        {
            Composed,
            Text,
            Description,
            Picture,
            Audio,
            Emoticon,
        }
    }
}
