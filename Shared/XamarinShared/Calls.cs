﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using EncryptedMessaging;
using XamarinShared.ViewModels;
using static EncryptedMessaging.Contacts;
using static EncryptedMessaging.MessageFormat;

namespace XamarinShared
{
    public class Calls
    {
        private static ulong MyId;
        public Observable<CallViewModel> AllCalls;
        private static Calls Instance;

        public static Calls GetInstance()
        {
            if (Instance == null)
                Instance = new Calls();
            return Instance;
        }

        private Calls()
        {
            AllCalls = new Observable<CallViewModel>();
        }

        public void SetMyId(ulong myId)
        {
            MyId = myId;
        }

        public void Insert(Message message, int index)
        {
            
            AllCalls.Insert(index, new CallViewModel(message.Contact, 
                   message.Type, GetCallStatus(message, out CallType callType), GetMessageTime(message.Creation),
                   message.PostId, message.Creation, callType));
        }

        public void Add(Message message)
        {
            AllCalls.Add(new CallViewModel(message.Contact, 
                   message.Type, GetCallStatus(message, out CallType callType), GetMessageTime(message.Creation),
                   message.PostId, message.Creation, callType));
        }

        public void Remove(ulong messageId)
        {
            foreach (CallViewModel call in AllCalls)
            {
                if (call.MessageId == messageId)
                {
                    AllCalls.Remove(call);
                    return;
                }
            }
        }
        public void AddAndSortCallMessages(Message newMessage)
        {
            try
            {
                if (newMessage.Contact == null) return;
                DateTime dateLimit = DateTime.Now - TimeSpan.FromDays(7);

                if (newMessage.Creation.CompareTo(dateLimit) == -1) return;
                lock (AllCalls)
                {
                    for (int i = 0; i < AllCalls.Count; i++)
                    {
                        if (AllCalls[i].Creation.CompareTo(newMessage.Creation) < 0)
                        {
                            Insert(newMessage, i);
                            return;
                        }
                    }
                    Add(newMessage);
                }
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }

        public void ClearCleanedContactCalls(string publicKeys)
        {
            lock (AllCalls)
            {
                List<CallViewModel> messages = AllCalls.ToList();
                foreach (CallViewModel message in messages)
                    if (message.Contact.PublicKeys == publicKeys)
                        AllCalls.Remove(message);
            }
        }

       private string GetCallStatus(Message message, out CallType callType)
        {

            CallType row = ResolveType(message, out int duration);
            callType = row;
            switch (row)
            {
                case CallType.INCOMING:
                    return Localization.Resources.Dictionary.Incoming + "  " + FormatTime(duration);
                case CallType.OUTGOING:
                    return Localization.Resources.Dictionary.Outgoing + "  " + FormatTime(duration);
                case CallType.UNANSWERED:
                    return (message.Type == MessageType.VideoCall || message.Type == MessageType.StartVideoGroupCall) ? Localization.Resources.Dictionary.UnasweredVideoCall : Localization.Resources.Dictionary.UnasweredAudioCall;
                case CallType.LOST:
                    return Localization.Resources.Dictionary.Lost;
                case CallType.NONE:
                default:
                    return Localization.Resources.Dictionary.AudioCall;
            };
        }

        private string GetMessageTime(DateTime dateTime)
        {
            DateTime now = DateTime.Now.ToLocalTime();
            CultureInfo culture = Thread.CurrentThread.CurrentUICulture;

            if ((now - dateTime).Days < 1)
                return Localization.Resources.Dictionary.Today;
            else if ((now - dateTime).Days < 2)
                return Localization.Resources.Dictionary.Yesterday;
            else if ((now - dateTime).Days < 7)
                return culture.DateTimeFormat.GetDayName(dateTime.DayOfWeek);
            else
                return dateTime.ToString("dd.MM hh:mm", culture);
        }

        private CallType ResolveType(Message m, out int duration)
        {
            duration = 0;
            byte[] data = m.GetData();
            if (data == null)
                return CallType.NONE;

            string decodedData = Encoding.Unicode.GetString(data);
            try
            {
                duration = Convert.ToInt32(decodedData);
                bool isImAuthor = m.AuthorId == MyId;

                if (duration == 0)
                    return isImAuthor ? CallType.UNANSWERED : CallType.LOST;
                return isImAuthor ? CallType.OUTGOING : CallType.INCOMING;
            }
            catch (FormatException)
            {
                return CallType.LOST;
            }
        }

        private string FormatTime(int time)
        {
            string sec, min;
            min = Convert.ToString(time / 60);
            sec = Convert.ToString(time % 60);
            if (min.Length == 1) min = "0" + min;
            if (sec.Length == 1) sec = "0" + sec;
            return min + ":" + sec;
        }

    }

    public enum CallType
    {
        UNANSWERED,
        LOST,
        INCOMING,
        OUTGOING,
        NONE
    }
}
