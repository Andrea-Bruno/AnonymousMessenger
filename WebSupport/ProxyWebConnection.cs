using EncryptedMessaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSupport
{
    public static class ProxyWebConnection
    {
#if DEBUG
        public static bool Reset = false;
#endif
        /// <summary>
        /// Initializer: When the QR code in the website is scanned, the connection between the website and the device is created by initializing this class. The public key generated in the browser is passed with the QR code, and used to create an instance of this class
        /// </summary>
        /// <param name="context"></param>
        /// <param name=" login">To be set true only for debug mode: It is used for testing in order to prepare the device to receive the public key from the proxy. In production the public key must be sent to the device only using the QR code scan, so this parameter must be false</param>
        public static void Initialize(Context context, bool login = false)
        {
            context.ViewMessage += OnViewMessage;
            Context = context;

            Context.OnContactEvent += OnMessageReceived;
            WebProxy = Context.Contacts.AddContact(WebProxyPublicKey, "WebProxy", true, Contacts.SendMyContact.None);
            if (login)
                Context.Messaging.LoginToServer(false, WebProxy);
        }

        /// <summary>
        /// Avoid sending messages to the browser that have been written to the browser
        /// </summary>
        private static readonly List<ulong> ExludeMessages = new List<ulong>();
        private static void OnViewMessage(Message message, bool isMyMessage)
        {
            if (ExludeMessages.Contains(message.PostId))
            {
                ExludeMessages.Remove(message.PostId);
            }
            else
            {
                var messageJson = new MessageJson(message, isMyMessage);
                Communication.SendMessages(new MessageJson[] { messageJson }, Communication.Command.SetNewPost);
            }
        }
#if DEBUG
        const string WebProxyPublicKey = @"AgR2WgAkdJJSCIvhzriqLxAcpSOGblFcJ15KAVx/nMf6"; // For testing
#else
        const string WebProxyPublicKey = @"AujbGstMDTBo2hAxcbXJjAdinn1X00KEdyyvDMOxt5fT"; // Production
#endif
        public static Contact WebProxy { get; private set; }
        internal static Context Context;
        private static void OnMessageReceived(Message message)
        {
            if (message.ChatId == WebProxy.ChatId)
            {
                if (message.Type == MessageFormat.MessageType.SubApplicationCommandWithData || message.Type == MessageFormat.MessageType.SubApplicationCommandWithParameters)
                {
                    message.GetSubApplicationCommand(out var appId, out var purpose, out var data, out var parameters);
                    if (appId == BitConverter.ToUInt16(Encoding.ASCII.GetBytes("web"), 0))
                    {
                        Communication.ProcessIncomingCommand((Communication.Purpose)purpose, parameters != null && parameters.Count != 0 ? parameters[0] : data);
                    }
                }
            }
        }
    }
}
