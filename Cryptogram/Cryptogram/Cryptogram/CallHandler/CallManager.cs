using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Anonymous.CallHandler;
using Anonymous.Models;
using Anonymous.Services;
using Anonymous.Views;
using EncryptedMessaging;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Extensions;

[assembly: Dependency(typeof(CallManager))]
namespace Anonymous.CallHandler
{
    public class CallManager : ICallNotificationService
    {
        public void FinishCall(string chatId, bool isVideoCall, bool isCallingByMe, int durationInSecond)
        {
            try
            {
                if (!isCallingByMe && durationInSecond > 0)
                    return;

                if (NavigationTappedPage.Context == null)
                    NavigationTappedPage.InitContext(false);

                Contact contact = NavigationTappedPage.Context.Contacts.GetContact(Convert.ToUInt64(chatId));
                if (contact == null) return;
                if (contact.IsGroup)
                {
                    NavigationTappedPage.Context.Messaging.SendEndCall(Encoding.Unicode.GetBytes(durationInSecond + ""), contact);
                }
                else
                {
                  
                    if (isVideoCall && isCallingByMe)
                        NavigationTappedPage.Context.Messaging.SendVideoCall(Encoding.Unicode.GetBytes(durationInSecond + ""), contact);
                    else if (isCallingByMe)
                        NavigationTappedPage.Context.Messaging.SendAudioCall(Encoding.Unicode.GetBytes(durationInSecond + ""), contact);

                    else if (!isCallingByMe && durationInSecond == 0)
                    {       /// cancel call without receiver accept
                        SendEndCallMessage(chatId);
                        return;
                    }
                }
            }
            catch(Exception e)
            {
                Debugger.Break();
            }
        }

        public void DeclineCall(string chatId, bool isCallDeclinedByReceiver = true)
        {
            try
            {
                if (NavigationTappedPage.Context == null && NavigationTappedPage.InitContextStarted)
                {
                    new Thread(WaitContextInit).Start();
                }
                else if (NavigationTappedPage.Context == null)
                {
                    NavigationTappedPage.InitContext(false);
                }
                if (isCallDeclinedByReceiver)
                    SendDeclinedCallMessage(chatId);
                else
                    SendEndCallMessage(chatId);
            }catch(Exception e)
            {
                Debugger.Break();
            }
        }

        public async void StartCall(Contact _contact, bool isVideoCall, bool joinCall = false)
        {
            if (_contact.IsBlocked) return;
            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                await Application.Current.MainPage.DisplayToastAsync(Localization.Resources.Dictionary.CheckYourInternetConnection);
                return;
            }
            if (await PermissionManager.CheckCameraPermission().ConfigureAwait(true)
                    && await PermissionManager.CheckMicrophonePermission().ConfigureAwait(true)
                    && await PermissionManager.CheckStoragePermission().ConfigureAwait(true))
            {
                try
                {
                    DependencyService.Get<IAudioCallConnector>().Start(_contact.ChatId.ToString(), _contact.Name, isVideoCall, true, _contact.IsGroup, _contact.Avatar); // maybe it is needed to register
                    if (joinCall) return;
                    if (_contact.IsGroup)
                    {
                        if(isVideoCall)
                            NavigationTappedPage.Context.Messaging.SendStartVideoGroupCall(Encoding.Unicode.GetBytes("Call started"), _contact);
                        else
                            NavigationTappedPage.Context.Messaging.SendStartAudioGroupCall(Encoding.Unicode.GetBytes("Call started"), _contact);
                    }
                    else if(!_contact.ImBlocked)
                    {
                        if (_contact.Os == Contact.RuntimePlatform.Android)
                            App.SendNotification(_contact, isVideoCall? NotificationService.NotificationType.P2P_START_VIDEO_CALL : NotificationService.NotificationType.P2P_START_AUDIO_CALL);
                        else
                            NavigationTappedPage.Context.Messaging.SendPushNotification(_contact.DeviceToken, _contact.ChatId, isVideoCall, _contact.MyRemoteName == null ? NavigationTappedPage.Context.My.Name : _contact.MyRemoteName);
                    }
                }
                catch (Exception)
                {
                    await Application.Current.MainPage.DisplayToastAsync(Localization.Resources.Dictionary.TheFunctionalityHasNotBeenImplemented);
                }
            }
        }

        public void SetRingingStatus(string chatId)
        {
            try
            {
                if (NavigationTappedPage.Context == null && NavigationTappedPage.InitContextStarted)
                {
                    new Thread(WaitContextInit).Start();
                }
                else if (NavigationTappedPage.Context == null)
                {
                    NavigationTappedPage.InitContext(false);
                }
                App.SendNotification(NavigationTappedPage.Context.Contacts.GetContact(Convert.ToUInt64(chatId)), NotificationService.NotificationType.RINGING);
            }
            catch(Exception e)
            {
                Debugger.Break();
            }
        }

        private void SendEndCallMessage(string chatId)
        {
            Contact contact = NavigationTappedPage.Context.Contacts.GetContact(Convert.ToUInt64(chatId));
            if (contact != null)
                NavigationTappedPage.Context.Messaging.SendEndCall(Encoding.Unicode.GetBytes(0 + ""), contact);
        }

        private void SendDeclinedCallMessage(string chatId)
        {
            Contact contact = NavigationTappedPage.Context.Contacts.GetContact(Convert.ToUInt64(chatId));
            if(contact!=null)
                NavigationTappedPage.Context.Messaging.SendDeclinedCall(Encoding.Unicode.GetBytes("Missed call"), contact);
        }

        private void WaitContextInit()
        {
            while (NavigationTappedPage.Context == null)
            {
                Thread.Sleep(50);
            }
        }
    }
}
