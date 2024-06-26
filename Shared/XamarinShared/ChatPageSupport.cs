using Xamarin.Forms;
using EncryptedMessaging;
using static EncryptedMessaging.MessageFormat;
using System.Collections.Generic;
using System;
using System.Text;
using XamarinShared.ViewCreator;
using VideoFileCryptographyLibrary;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Linq;
using XamarinShared.ViewCreator.ViewHolder;

namespace XamarinShared
{
    public static class ChatPageSupport
    {
        internal static void Initialize(bool multipleChatModes, bool newMessageOnTop, OnNewMessageAddedToView onNewMessageAddedToView)
        {
            //if (_onViewMessage == onViewMessage)
            //    return; // Already initialized
            Config.ChatUI.MultipleChatModes = multipleChatModes;
            Config.ChatUI.NewMessageOnTop = newMessageOnTop;
            _onNewMessageAddedToView = onNewMessageAddedToView;
        }

        public static void ScrollToMessage(string messageId)
        {
            //find position
            // var container = _scrollMessageContainer.Content as StackLayout;
            // View repliedMessageView = null;
            // lock (container)
            // {
            //     foreach (View view in container.Children)
            //     {
            //         if (view != null && view.GetTag() == messageId)
            //         {
            //             repliedMessageView = view;
            //             break;
            //         }
            //
            //     }
            // }
            //
            // if (repliedMessageView == null) return;
            //
            //
            // Device.BeginInvokeOnMainThread(() =>
            // {
            //     _scrollMessageContainer.ScrollToAsync(repliedMessageView, ScrollToPosition.MakeVisible, false);
            //     BlinkEffect(repliedMessageView);
            // });
        }

        private static void BlinkEffect(View repliedMessageView)
        {
            repliedMessageView.Opacity = 0.2;
            repliedMessageView.FadeTo(1, 1400);

        }

        private static void LoadReplyViews(bool isSingle = false)
        {
            // Dictionary<string, BaseView> keyValuePairs = new Dictionary<string, BaseView>();
            //
            // var container = _scrollMessageContainer.Content as StackLayout;
            // lock (container)
            // {
            //     //todo reverse forloop on single and get image source from baseview
            //     foreach (View view in container.Children)
            //     {
            //
            //         if (view == null) continue;
            //         var baseView = view as BaseView;
            //         string postId = view.GetTag();
            //         if (baseView == null || string.IsNullOrEmpty(postId) || keyValuePairs.ContainsKey(view.GetTag())) continue;
            //
            //         keyValuePairs.Add(view.GetTag(), baseView);
            //         string repliedPostId = Utils.GetTag(baseView.MessageFrameContent);
            //         if (string.IsNullOrEmpty(repliedPostId)) continue;
            //         keyValuePairs.TryGetValue(repliedPostId, out BaseView repliedMessageView);
            //
            //         if (repliedMessageView == null) continue;
            //
            //         Frame replyLayout = MessageViewCreator.GenerateReplyLayout(repliedMessageView.Message);
            //         baseView.MessageFrameContent.Children.Insert(0, replyLayout);
            //         replyLayout.Click((sender, args) => ScrollToMessage(repliedPostId));
            //         Utils.SetTag(baseView.MessageFrameContent, null);
            //
            //         if (isSingle) break;
            //     }
            // }
            // keyValuePairs.Clear();

        }

        private struct Config
        {
            public struct ChatUI
            {
                public static bool MultipleChatModes;
                public static bool NewMessageOnTop;
            }
        }

        private static Contacts.Observable<object> CurrentMessageContainer;

        public static void SetCurrentContact(Contact contact)
        {
            ClearMessageSelection(Setup.Context.Messaging.CurrentChatRoom ?? contact);
            if(contact!=null)
                CurrentMessageContainer = GetMessageContainerOfContact(contact) ?? InstantiateNewMessageContainer();

            //CurrentContact = contact; //Don't moving from this position
            //_scrollMessageContainer.Content = CurrentMessageContainer;
            Setup.Context.Messaging.CurrentChatRoom = contact;
            if (contact != null)
                LoadReplyViews();
        }

        private static void ClearMessageSelection(Contact contact)
        {
            if (contact == null) return;
            GetContactViewItems(contact, out ContactViewItems contactViewItems);
            if (contactViewItems.SelectedMessagesList.Count != 0) // prevent loop for collection change SelectedMessages_CollectionChanged
                contactViewItems.SelectedMessagesList.Clear();
            contactViewItems.IsMessageSelection = false;
        }

        public static void SetMultipleChatModes(bool value)
        {
            Config.ChatUI.MultipleChatModes = value;
        }
        
        public class DateRow
        {
            public string date;
        }

        public static Contact CurrentContact() => Setup.Context.Messaging.CurrentChatRoom;

        internal static void ViewMessage(Message message, bool isMyMessage)
        {
            //Device.BeginInvokeOnMainThread(() =>
            //{
            Contact contact = message.Contact;
            if (contact == null)
                return; //We received a massage from a stranger (a contact not in the address book)
            var messageContainer = GetMessageContainerOfContact(contact);
            if (messageContainer == null || (messageContainer.Count > 0 && message.Type == MessageType.Contact))
                return;
            View frame;
            
            try
            {
                MessageViewCreator.Instance.OnViewMessage(message, isMyMessage, out frame);
            }
            catch(Exception e)
            {
                frame = null;
            }
            GetContactViewItems(contact, out ContactViewItems contactViewItems);

            if (frame != null && !contactViewItems.MessageIdList.Contains(message.PostId))
            {

                lock (messageContainer)
                {

                    var addInTop = false; //Old messages are added at the top and new ones at the bottom of the view
                    lock (_contactsViewItemsDictionary)
                    {

                        DateTime lastMessageTime = contactViewItems.LastMessageTime;

                        if (lastMessageTime != DateTime.MinValue)
                        {
                            addInTop = message.ReceptionTime < lastMessageTime;
                            contactViewItems.LastMessageTime = DateTime.MinValue;
                        }

                        contactViewItems.LastMessageTime = message.ReceptionTime;
                        if (addInTop)
                            contactViewItems.IsAllMessagesLoaded = false;
                    }

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (message == null || message.GetData() == null) return;

                        if (message.Type != MessageType.Contact || (message.Type == MessageType.Contact && messageContainer.Count == 0))
                        {
                            DateTime lastUIAddedMessageDate = contactViewItems.LastUIAddedMessageDate;
                            DateTime firstUIAddedMessageDate = contactViewItems.FirstUIAddedMessageDate;

                            if (lastUIAddedMessageDate.CompareTo(DateTime.MinValue) == 0 ||
                                (((message.Creation.ToLocalTime().GetTotalDays() -
                                   lastUIAddedMessageDate.GetTotalDays()) > 0 ||
                                  (message.Creation.ToLocalTime().GetTotalDays() -
                                   firstUIAddedMessageDate.GetTotalDays()) < 0)))
                            {
                                int index = (message.Creation.ToLocalTime().GetTotalDays() -
                                             lastUIAddedMessageDate.GetTotalDays()) < 0
                                    ? 0
                                    : messageContainer.Count;
                                // messageContainer.Insert(index,
                                //     new DateRow {date = message.Creation.ToLocalTime().ToShortDateString()});

                            }

                            if (contactViewItems.FirstUIAddedMessageDate > message.Creation.ToLocalTime())
                                contactViewItems.FirstUIAddedMessageDate = message.Creation.ToLocalTime();
                            if (contactViewItems.LastUIAddedMessageDate < message.Creation.ToLocalTime())
                                contactViewItems.LastUIAddedMessageDate = message.Creation.ToLocalTime();

                            // bool isNewMessageLabelAdded = CheckContainterContainsUnreadedMessageView(messageContainer.Children) != null;
                            //
                            // if (message.Type != MessageType.Contact && contact.UnreadMessages > 0 && !isNewMessageLabelAdded)
                            // {
                            //     if (messageContainer.Children.Count > contact.UnreadMessages)
                            //     {
                            //         messageContainer.Children.Add(GetUnreadedMessagesFrame());
                            //         isNewMessageLabelAdded = true;
                            //     }
                            // }
                            var tr = new TextRow
                            {
                                message = message
                            };
                            messageContainer.Insert(addInTop ? 1 : messageContainer.Count, tr);
                            
                            contactViewItems.MessageIdList.Add(message.PostId);

                            // if (message.Type != MessageType.Contact && contact.UnreadMessages > 0 && !isNewMessageLabelAdded)
                            // {
                            //     if (messageContainer.Children.Count - 1 == contact.UnreadMessages)
                            //     {
                            //         isNewMessageLabelAdded = true;
                            //         messageContainer.Children.Insert(1, GetUnreadedMessagesFrame());
                            //     }
                            // }

                            if (Setup.Context.Messaging.CurrentChatRoom != null && message.Contact != null && Setup.Context.Messaging.CurrentChatRoom == message.Contact)
                            {
                                NewMessageReceived();
                                LoadReplyViews(true);
                            }
                        }
                    });
                }
                if (message.Type == MessageType.AudioCall || message.Type == MessageType.VideoCall || message.Type == MessageType.EndCall || message.Type == MessageType.DeclinedCall)
                {
                    Calls.GetInstance().AddAndSortCallMessages(message);
                }


            }
        }

        private static void NewMessageReceived()
        {
            _onNewMessageAddedToView.Invoke();

        }

        public static void TransalateMessageDisplay(Message message, Label textLabel)
        {
            if (message == null || textLabel == null || message.GetData()==null) return;
       
            if (message.Translation != null)
            {
                if (message.Translation.Length > 700)
                    textLabel.Text = Localization.Resources.Dictionary.Translated + ": " + message.Translation.Substring(0, 700) + ". . .";
                else
                    textLabel.Text = Localization.Resources.Dictionary.Translated + ": " + message.Translation;
            }
                
        }


        public static void ForceTransalateMessageDisplay(string text, Label textLabel)
        {
            if (string.IsNullOrWhiteSpace(text) || textLabel == null) return;
            textLabel.Text = Localization.Resources.Dictionary.Translated + ": " + text;
        }

        public static void RemoveMessage(Message message, bool alsoDeleteRemote = true)
        {
            // if (Setup.Context.Messaging.CurrentChatRoom != null)
            // {
            //     //var counter = 0;
            //     //var id = message.PostId.ToString();
            //     var container = Setup.Context.Messaging.CurrentChatRoom.MessageContainerUI as StackLayout;
            //     lock (container)
            //     {
            //         // Delete local video file to free up the space
            //         if (message.Type == MessageType.ShareEncryptedContent)
            //         {
            //             string contentType = string.Empty;
            //             byte[] privateKey = null;
            //             string description = string.Empty;
            //             string serverUrl = string.Empty;
            //             message.GetShareEncryptedContentData(out contentType, out privateKey, out description, out serverUrl);
            //             var fileName = string.Format("{0}.{1}", ComputePsuedoHash.ToHex(SHA256.Create().ComputeHash(privateKey)), contentType);
            //             void deleteVideo(object folder)
            //             {
            //                 var filePath = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, folder.ToString(), fileName);
            //                 if (File.Exists(filePath))
            //                     File.Delete(filePath);
            //             }
            //             new Thread(new ParameterizedThreadStart(deleteVideo)).Start("uploads");
            //             new Thread(new ParameterizedThreadStart(deleteVideo)).Start("downloads");
            //         }
            //
            //         GetContactViewItems(message.Contact, out var contactViewItems);
            //
            //
            //         (Setup.Context.Messaging.CurrentChatRoom.MessageContainerUI as StackLayout).Children.Remove(container.Children.Where(v => v.GetTag() == message.PostId.ToString()).FirstOrDefault());
            //         message.Delete(alsoDeleteRemote);
            //         if (contactViewItems.LastUIAddedMessageDate == DateTime.MinValue)
            //             contactViewItems.LastUIAddedMessageDate = message.Contact.LastMessageTime;
            //         Calls.GetInstance().Remove(message.PostId);
            //
            //     }
            // }
        }
        private static void DeletePreviousTwoMessages(int index, StackLayout container, ContactViewItems contactViewItems)
        {
            if (index == container.Children.Count - 1 &&
                ((index > 0 && container.Children[index - 1].GetTag().Contains("MessageDateFrameContainer@"))
                || (index > 1 && container.Children[index - 2].GetTag().Contains("MessageDateFrameContainer@") &&
                        container.Children[index - 1].GetTag().Contains("UnreadedMessageFrameContainer"))))
                contactViewItems.LastUIAddedMessageDate = DateTime.MinValue; // this is needed for addding datetime for new messages.

            if (index != container.Children.Count - 1 && !container.Children[index+1].GetTag().Contains("MessageDateFrameContainer@")) // delete message only if with there is a single message on that date
            {
                DeleteMessageFrames(new int[] { index }, container);
                return;
            }
            // if message list is like this  [...  DateFrame, UnreadedMessageFrame, Message] or contains any one of this, then delete all the three message frames.
            var previousIndex = index - 1;
            bool deletePrevious=false;
            if (previousIndex >= 0)
                if (container.Children[previousIndex].GetTag().Contains("MessageDateFrameContainer@") ||
                        container.Children[previousIndex].GetTag().Contains("UnreadedMessageFrameContainer")) //  remove message date frame or unreaded message frame 
                    deletePrevious = true;
            if (deletePrevious)
            {
                if (previousIndex - 1 >= 0 && (container.Children[previousIndex - 1].GetTag().Contains("MessageDateFrameContainer@") ||
                        container.Children[previousIndex - 1].GetTag().Contains("UnreadedMessageFrameContainer")))
                { //check two previos message
                    DeleteMessageFrames(new int[] { index, previousIndex, previousIndex - 1 }, container);
                }
            }
            else
                DeleteMessageFrames(new int[] { index }, container);

        }

        private static void DeleteMessageFrames(int[] arr, StackLayout container)
        {
            foreach (int i in arr)
                container.Children.RemoveAt(i);
        }

        public static void RemoveMessages(List<Message> messages, bool alsoDeleteRemote = true)
        {
            foreach (Message message in messages)
                RemoveMessage(message, alsoDeleteRemote);
        }

        internal static void OnContactEvent(Message message)
        {
            if (message.Type == MessageType.Delete)
            {
                RemoveMessage(message.Contact, BitConverter.ToUInt64(message.GetData(), 0));
                Calls.GetInstance().Remove(message.PostId);
            }

            if (message.Type == MessageType.SmallData || message.Type == MessageType.Data)
            {
                var keyValue = Functions.SplitIncomingData(message.GetData(), message.Type == MessageType.SmallData);
            }
        }

        public static void RemoveMessage(Contact contact, ulong postId)
        {
            // if (contact.MessageContainerUI != null)
            // {
            //     var id = postId.ToString();
            //     var container = contact.MessageContainerUI as StackLayout;
            //     lock (container)
            //     {
            //         View toRemove = null;
            //         //toRemove	= (CurrentContact.MessageContainerUI as StackLayout).Children.First(x => x.AutomationId == id);
            //         foreach (View item in container.Children)
            //         {
            //             if (item.GetTag() == id)
            //             {
            //                 toRemove = item;
            //                 break;
            //             }
            //         }
            //         if (toRemove != null)
            //             container.Children.Remove(toRemove);
            //     }
            // }
        }

        public static void RemoveUnreadedMessageView()
        {
            //todo  
            // if (Setup.Context.Messaging.CurrentChatRoom != null)
            // {
            //     var view = Setup.Context.Messaging.CurrentChatRoom.MessageContainerUI as Stac;
            //     View unreadedMessagesContainer = CheckContainterContainsUnreadedMessageView(view.Children);
            //     if (unreadedMessagesContainer != null)
            //     {
            //         GetContactViewItems(Setup.Context.Messaging.CurrentChatRoom, out var contactViewItems);
            //         view.Children.Remove(unreadedMessagesContainer);
            //     }
            //     
            // }
        }

        public static View CheckContainterContainsUnreadedMessageView(IList<View> list)
        {
            lock (list)
            {
                foreach (View view in list)
                {
                    if (view.ClassId == "UnreadedMessageContainer")
                        return view;
                }
            }
            return null;
        }

        private static View GetUnreadedMessagesFrame()
        {
            StackLayout stack = new StackLayout
            {
                ClassId = "UnreadedMessageContainer",
                HeightRequest = 36,
                MinimumHeightRequest = 36,
                BackgroundColor = Palette.UnreadedMessagesLabelBackgroundColor
            };
            Label unreadedMessageLabel = new Label
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                Text = Localization.Resources.Dictionary.UnreadedMessages,
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 18,
                TextColor = Palette.UnreadedMessagesLabelTextColor,
                FontFamily = "PoppinsSemiBold"
            };
            stack.Children.Add(unreadedMessageLabel);
            return stack;
        }

        public static View GetDateLayout(string date)
        {
            var stack = new StackLayout { ClassId = "MessageDateFrameContainer", BackgroundColor = Color.Transparent };
            stack.SetTag("MessageDateFrameContainer@"+date);
            stack.Children.Add(GetDateFrame(date));
            return stack;
        }

        public static Frame GetDateFrame(string date)
        {
            var unreadMessagesFrame = new Frame { HasShadow = false, BackgroundColor = Color.Transparent, HorizontalOptions = LayoutOptions.CenterAndExpand};
            var dateLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                Padding = new Thickness(10, 4, 10, 4),
                VerticalTextAlignment = TextAlignment.Center,
                Text = date,
                FontSize = 16,
                TextColor = Palette.DateLabelTextColor,
                FontFamily = "PoppinsRegular"
            };
            unreadMessagesFrame.Content = dateLabel;
            return unreadMessagesFrame;
        }
        
        private static Contacts.Observable<object>  InstantiateNewMessageContainer() => new Contacts.Observable<object>() { };
        public static Contacts.Observable<object> GetMessageContainerOfContact(Contact contact)
        {
            if (contact != null)
                if (Config.ChatUI.MultipleChatModes)
                {
                    if (contact.MessageContainerUI == null)
                        contact.MessageContainerUI = new Contacts.Observable<object>();
                    return (Contacts.Observable<object>)contact.MessageContainerUI;
                }
                else if (contact == Setup.Context.Messaging.CurrentChatRoom)
                    return CurrentMessageContainer;
            return null;
        }

        public delegate void OnViewMessage<T1, T2, T3>(Message message, bool isMy, out View content);

        public delegate void OnNewMessageAddedToView();
        private static OnNewMessageAddedToView _onNewMessageAddedToView;

        private static readonly Dictionary<Contact, ContactViewItems> _contactsViewItemsDictionary = new Dictionary<Contact, ContactViewItems>();
        public static void GetContactViewItems(Contact contact, out ContactViewItems contactViewItems)
        {
            _contactsViewItemsDictionary.TryGetValue(contact, out contactViewItems);

            if (contactViewItems == null)
            {
                contactViewItems = new ContactViewItems();
                _contactsViewItemsDictionary.Add(contact, contactViewItems);
            }
        }

        public static ContactViewItems GetContactViewItems(Contact contact)
        {
            GetContactViewItems(contact, out ContactViewItems contactViewItems);
            return contactViewItems;
        }
    }
}