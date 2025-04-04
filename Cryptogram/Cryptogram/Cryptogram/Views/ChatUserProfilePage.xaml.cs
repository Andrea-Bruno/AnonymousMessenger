using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using EncryptedMessaging;
using Cryptogram.Services;
using CustomViewElements;
using Cryptogram.DesignHandler;
using Xamarin.CommunityToolkit.Extensions;

namespace Cryptogram.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatUserProfilePage : BasePage
    {
        private Contact _contact;
        private List<Contact> _contacts = new List<Contact>();
        private Contact _lastItemSelected;

        public ChatUserProfilePage(Contact contact)
        {
            _contact = contact;
            InitView();
            BindingContext = _contact;

            if (contact.IsGroup)
            {
                ItemsListView.IsVisible = true;
                Edit.IsVisible = false;
                Toolbar.Title = Localization.Resources.Dictionary.InfoGroup;
                foreach (var key in _contact.Participants)
                {
                    if (key != null && Convert.ToBase64String(key) != NavigationTappedPage.Context.My.GetPublicKey())
                    {
                        Contact participant = NavigationTappedPage.Context.Contacts.GetParticipant(key);
                        if (participant != null && !string.IsNullOrWhiteSpace(participant.Name))
                            _contacts.Add(participant);
                    }
                }
                ItemsListView.ItemsSource = _contacts;
            }
        }

        private void InitView()
        {
            InitializeComponent();
            Name.Text = _contact.Name;
            SetUITextForBlock();
            SetUITextForMute();
            if (_contact.IsGroup)
            {
                Edit.Opacity = 0.5;
                Edit.IsEnabled = false;
            }
            MessageAutoTranslation.IsOn = _contact.TranslationOfMessages;
            MessageConfirmationButton.IsOn = !_contact.SendConfirmationOfReading;
        }

        protected override void OnAppearing() => base.OnAppearing();

        public async void Photo_Clicked(object sender, EventArgs _)
        {
            var imagebox = (Image)sender;
            if (imagebox != null)
                await PopupNavigation.Instance.PushAsync(new ImageViewPopupPage(imagebox.Source), false).ConfigureAwait(true);
        }

        private void Edit_Clicked(object _, EventArgs e)
        {
            NameEntry.Focus();
            NameEntry.Text = Name.Text;
            ChangeViewState(false);
        }

        private void Save_Clicked(object _, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                Name.Text = NameEntry.Text;
                _contact.Name = NameEntry.Text.Trim();
                NameEntry.Unfocus();
            }
            else
            {
                this.DisplayToastAsync(Localization.Resources.Dictionary.PleaseAddValidName);
                return;
            }

            ChangeViewState(true);

            if (Device.RuntimePlatform == Device.Android)
                DependencyService.Get<ISharedPreference>().AddContact(_contact.ChatId + "", _contact.Name, _contact.Os == Contact.RuntimePlatform.Android);
        }

        private async void OnItemSelected(object _, SelectedItemChangedEventArgs args)
        {
            _lastItemSelected = args.SelectedItem as Contact;
            if (_lastItemSelected != null)
                await PopupNavigation.Instance.PushAsync(new EditGroupUserPopup(_lastItemSelected), true).ConfigureAwait(true);
        }

        private void Notification_Clicked(object _, EventArgs e)
        {
            _contact.IsMuted = !_contact.IsMuted;
            SetUITextForMute();
            _contact.Save();
        }

        private void Block_Clicked(object _, EventArgs e)
        {
            _contact.IsBlocked = !_contact.IsBlocked;
            SetUITextForBlock();
            _contact.Save();
        }

        private void SetUITextForMute()
        {
            if (_contact.IsMuted)
            {
                Mute.Source = DesignResourceManager.GetImageSource("ic_chatuser_mute.png");
            }
            else
            {
                Mute.Source = DesignResourceManager.GetImageSource("ic_chatuser_unmute.png");
            }
        }

        private void SetUITextForBlock()
        {
            if (_contact.IsBlocked)
            {
                BlockUser.Text = Localization.Resources.Dictionary.UnblockTheUser;
                BlockUserInfo.Text = Localization.Resources.Dictionary.UnblockTheUserInfo;
                UserBlockblockIcon.Source = DesignResourceManager.GetImageSource("ic_chatuser_unblocked.png");
            }

            else
            {
                BlockUser.Text = Localization.Resources.Dictionary.BlockTheUser;
                BlockUserInfo.Text = Localization.Resources.Dictionary.BlockTheUserInfo;
                UserBlockblockIcon.Source = DesignResourceManager.GetImageSource("ic_chatuser_blocked.png");
            }
        }

        private void BackEdit_Clicked(object _, EventArgs e)
        {
            NameEntry.Unfocus();
            ChangeViewState(true);
        }

        protected override bool OnBackButtonPressed()
        {
            if (NameEntryLyt.IsVisible)
            {
                ChangeViewState(true);
            }

            else Application.Current.MainPage.Navigation.PopAsync(false);

            return true;
        }

        private void ChangeViewState(bool isEditEnabled)
        {
            Edit.IsVisible = isEditEnabled;
            Name.IsVisible = isEditEnabled;
            SaveLyt.IsVisible = !isEditEnabled;
            BackButton.IsVisible = !isEditEnabled;
            NameEntryLyt.IsVisible = !isEditEnabled;
        }

        private void MessageAutoTranslation_StateChanged(object sender, Syncfusion.XForms.Buttons.SwitchStateChangedEventArgs e)
        {
            {
                if (_contact.IsGroup)
                {
                    if (MessageAutoTranslation.IsOn == true)
                    {
                        foreach (var c in _contacts)
                        {
                            c.TranslationOfMessages = true;
                            c.Save();
                        }
                    }
                    else
                    {
                        foreach (var c in _contacts)
                        {
                            c.TranslationOfMessages = false;
                            c.Save();
                        }
                    }
                }
                if (MessageAutoTranslation.IsOn == true)
                {
                    _contact.TranslationOfMessages = true;
                    _contact.Save();
                }
                else
                {
                    _contact.TranslationOfMessages = false;
                    _contact.Save();
                }
            }
        }

        private void MessageConfirmationButton_StateChanged(object sender, Syncfusion.XForms.Buttons.SwitchStateChangedEventArgs e)
        {
            _contact.SendConfirmationOfReading = !(bool)MessageConfirmationButton.IsOn;
            _contact.Save();
        }

        private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();

        private void Message_Clicked(object _, EventArgs e)
        {
            Application.Current.MainPage.Navigation.PushAsync(new ChatRoom(_contact), false);
        }

        private void AudioCall_Clicked(object _, EventArgs e_) => ((App)Application.Current).CallManager.StartCall(_contact, false);

        private void VideoCall_Clicked(object _, EventArgs e_) => ((App)Application.Current).CallManager.StartCall(_contact, true);
    }
}