using EncryptedMessaging;
using System;
using System.Text;


namespace CloudClient
{
    public partial class Communication : ICloudManager
    {
        /// <summary>
        /// Disable encryption to lighten the CPU load on the server side if the data encryption is done by the client before passing these to the send methods. If the data is not encrypted, it is recommended to set this parameter to true to add encryption with data communication
        /// </summary>        
        public Communication(Context context, string publicKey)
        {
            Cloud = context.Contacts.AddContact(publicKey, "CommunityCloud", Modality.Server, Contacts.SendMyContact.None);
            Context = context;
        }
        private Context Context;

        public Contact Cloud { get; set; }

        public enum Command : ushort  // 2 byte - the names must start with Get or Set
        {
            // NOTE: If you change this enumerator, you must also change the enumerator in Cloud Server Communication.vs
            SaveData,
            LoadData,
            LoadAllData,
            DeleteData,
            PushNotification
        }

                
        public void LoadDataFromCloud(string type, string name, int? ifSizeIsDifferent, bool shared)
        {
            var parameters = new byte[][] { Encoding.ASCII.GetBytes(type), Encoding.ASCII.GetBytes(name), ifSizeIsDifferent == null ? new byte[] { } : BitConverter.GetBytes((int)ifSizeIsDifferent), BitConverter.GetBytes(shared) };
            SendCmmand(Command.LoadData, parameters);
        }

        public void LoadAllDataFromCloud(string type, bool shared)
        {
            var parameters = new byte[][] { Encoding.ASCII.GetBytes(type), BitConverter.GetBytes(shared) };
            SendCmmand(Command.LoadAllData, parameters);
        }

        public void SaveDataOnCloud(string type, string name, byte[] data, bool shared)
        {
            var parameters = new byte[][] { Encoding.ASCII.GetBytes(type), Encoding.ASCII.GetBytes(name), data, BitConverter.GetBytes(shared) };
            SendCmmand(Command.SaveData, parameters);
        }

        public void DeleteDataOnCloud(string type, string name, bool shared)
        {
            var parameters = new byte[][] { Encoding.ASCII.GetBytes(type), Encoding.ASCII.GetBytes(name), BitConverter.GetBytes(shared) };
            SendCmmand(Command.DeleteData, parameters);
        }

        private void SendCmmand(Command command, params byte[][] parameters)
        {
            SendCommand?.Invoke((ushort)command, parameters);
        }

        void ICloudManager.OnCommand(ushort command, byte[][] parameters)
        {
            var answerToCommand = (Command)command;
            if (answerToCommand == Command.LoadData || answerToCommand == Command.LoadAllData)
            {
                var type = Encoding.ASCII.GetString(parameters[0]);
                var name = Encoding.ASCII.GetString(parameters[1]);
                var data = parameters[2];
                var shared = BitConverter.ToBoolean(parameters[3], 0);
                if (data.Length > 0)
                {
                    if (type.Equals("String", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (name.Equals("MyName", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Context.My.SetName(Encoding.Unicode.GetString(data), false);
                        }
                    }
                    else if (type.Equals("Contact", StringComparison.InvariantCultureIgnoreCase))
                    {
                        data = SecureStorage.Cryptography.Decrypt(data, Context.My.Csp.ExportCspBlob(true));
                        var contactMessage = new ContactMessage(data);
                        Context.Contacts.AddContact(contactMessage, Contacts.SendMyContact.SendNamelessForUpdate);
                    }
                    else if (type.Equals("Avatar", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var contact = Context.Contacts.GetContactByUserID(ulong.Parse(name));
                        if (contact != null)
                        {
                            // The first eight bytes of a PNG file always contain the following (decimal) values: 137 80 78 71 13 10 26 10
                            //if (data[0] == 137 && data[1] == 80 && data[2] == 78 && data[3] == 71 && data[4] == 13 && data[5] == 10 && data[6] == 26 && data[7] == 10)
                            //	contact.Avatar = data; // Compatibility with the old version which sends unencrypted avatars to the cloud
                            //else
                            //{
                            try
                            {
                                var myKey = Context.My.GetPublicKeyBinary();
                                var contactKey = contact.Participants.Find(x => !x.SequenceEqual(myKey));
                                contact.Avatar = Functions.Decrypt(data, contactKey); // The avatar is public but is encrypted using the contact's public key as a password, in this way it can only be decrypted by users who have this contact in the address book
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine(e.Message);
                                contact.Avatar = data;
                            }
                            //}
                        }
                    }
                }
            }
        }

        public void SendPushNotification(string deviceToken, ulong chatId, bool isVideo, string contactNameOrigin)
        {
            var parameters = new byte[][] { Encoding.ASCII.GetBytes(deviceToken), BitConverter.GetBytes(chatId), BitConverter.GetBytes(isVideo), Encoding.ASCII.GetBytes(contactNameOrigin) };
            SendCmmand(Command.PushNotification, parameters);
        }
        public Action<ushort, byte[][]> SendCommand { set; get; }

    }
}
