using System;
using System.Collections.Generic;
using CustomViewElements;
using Rg.Plugins.Popup.Services;
using Anonymous.DesignHandler;
using Anonymous.PopupViews;
using Utils;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace Anonymous.Views
{
    public partial class VerifyPassphrase : BasePage
    {
        private TapGestureRecognizer addTapGestureRecognizer;
        private TapGestureRecognizer removeTapGestureRecognizer;
        private StackLayout[] filledGaps = new StackLayout[12];
        public Action CloseEventHandler;
        private string[] parts;
        private readonly Dictionary<string, Frame> words = new Dictionary<string, Frame>();

        public VerifyPassphrase()
        {
            InitializeComponent();
            InitRecognizer();
            var parts = NavigationTappedPage.Context.My.GetPassphrase().Split(' ');
            Randomize(parts);
            for (int i = 0; i < parts.Length; i++)
            {
                AddWords(Convert.ToString(i), parts[i]);
            }
        }

        private void InitRecognizer()
        {
            addTapGestureRecognizer = new TapGestureRecognizer();
            removeTapGestureRecognizer = new TapGestureRecognizer();
            addTapGestureRecognizer.NumberOfTapsRequired = 1;
            removeTapGestureRecognizer.NumberOfTapsRequired = 1;
            addTapGestureRecognizer.Tapped += FrameAddTapped;
            removeTapGestureRecognizer.Tapped += FrameRemoveTapped;
            SetContinueButtonStatus();
        }

        void ContinueButton_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            if (CheckPassPhrase())
            {
                Save();
            }
        }

        private void FrameAddTapped(object sender, EventArgs e)
        {
            Frame frame = (Frame) sender;
            Label label = (Label) frame.Content;
            if (!string.IsNullOrEmpty(label.Text.Trim()))
            {
                for (int i = 0; i < filledGaps.Length; i++)
                    if (filledGaps[i] == null)
                    {
                        filledGaps[i] = FillGaps(label.Text, i);
                        filledGaps[i].SyncTag(frame);
                        label.Text = GenerateEmptyString(label.Text);
                        SetContinueButtonStatus();

                        return;
                    }
            }
        }

        private bool isPassphraseFilled()
        {
            foreach (var stackLayout in filledGaps)
            {
                if (stackLayout == null)
                    return false;
                var fr = (Frame) stackLayout.Children[1];
                var label = (Label) fr.Content;
                var b = string.IsNullOrWhiteSpace(label.Text);
                if (b)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetContinueButtonStatus()
        {
            var passphrase = isPassphraseFilled();
            if (passphrase&& !ContinueButton.IsEnabled)
            {
                ContinueButton.BackgroundColor = DesignResourceManager.GetColorFromStyle("Theme");
                ContinueButton.TextColor = DesignResourceManager.GetColorFromStyle("Color1");
                ContinueButton.IsEnabled = true;
            }
            else if (!passphrase && ContinueButton.IsEnabled)
            {
                ContinueButton.BackgroundColor = DesignResourceManager.GetColorFromStyle("BackgroundSecondary");
                ContinueButton.TextColor = DesignResourceManager.GetColorFromStyle("WhiteColor");
                ContinueButton.IsEnabled = false;
            }
        }

        private void FrameRemoveTapped(object sender, EventArgs e)
        {
            StackLayout stackLayout = (StackLayout) sender;
            Frame frame = (Frame) stackLayout.Children[1];
            Label label = (Label) frame.Content;
            if (!string.IsNullOrEmpty(label.Text.Trim()))
            {
                for (int i = 0; i < filledGaps.Length; i++)
                    if (filledGaps[i] == stackLayout)
                    {
                        filledGaps[i] = null;
                        break;
                    }

                SetContinueButtonStatus();
                string key = stackLayout.GetTag();
                AddWords(key, label.Text);
                label.Text = GenerateEmptyString(label.Text);
            }
        }

        private void AddWords(string key, string word)
        {
            words.TryGetValue(key, out Frame frame);

            if (frame != null)
            {
                ((Label) frame.Content).Text = word;
                return;
            }

            Frame newFrame = GenerateFrame(word);
            newFrame.SetTag(key);
            newFrame.GestureRecognizers.Add(addTapGestureRecognizer);
            Words.Children.Add(newFrame);
            words.Add(key, newFrame);
        }

        private StackLayout FillGaps(string word, int index)
        {
            if (TapWordsBelowFrame.IsVisible == true)
            {
                TapWordsBelowFrame.IsVisible = false;
                Gaps.IsVisible = true;
            }

            if (Gaps.Children.Count - 1 >= index && Gaps.Children.Count > 0)
            {
                StackLayout layout = (StackLayout) Gaps.Children[index];
                Frame fr = (Frame) layout.Children[1];
                Label label = (Label) fr.Content;
                label.Text = word;
                return layout;
            }

            StackLayout stackLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                MinimumWidthRequest = 50,
                Padding = 0,
            };
            Label indexLabel = new Label()
            {
                FontSize = 18,
                FontFamily = "PoppinsMedium",
                Text = (index + 1) + ".",
                HeightRequest = 38,
                Margin = new Thickness(0, 0, 3, 0),
                Padding = 0,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = DesignResourceManager.GetColorFromStyle("Theme"),
                VerticalOptions = LayoutOptions.Center,
            };
            Frame frame = GenerateFrame(word);
            stackLayout.GestureRecognizers.Add(removeTapGestureRecognizer);
            stackLayout.Children.Add(indexLabel);
            stackLayout.Children.Add(frame);
            Gaps.Children.Insert(index, stackLayout);
            return stackLayout;
        }

        private Frame GenerateFrame(string word)
        {
            var label = new Label()
            {
                VerticalOptions = LayoutOptions.Center,
                FontSize = 13,
                FontFamily = "PoppinsLight",
                HorizontalOptions = LayoutOptions.Fill,
                Text = word,
                Margin = new Thickness(6, 3, 6, 3),
                MinimumWidthRequest = 50,
                LineBreakMode = LineBreakMode.NoWrap,
                TextColor = Color.White,
            };

            return new Frame()
            {
                CornerRadius = Device.RuntimePlatform == Device.Android ? 24 : 18,
                MinimumWidthRequest = 40,
                Margin = new Thickness(0, 5),
                Padding = new Thickness(4, 0, 4, 0),
                BackgroundColor = Color.Transparent,
                HeightRequest = Device.RuntimePlatform == Device.Android ? 28 : 38,
                BorderColor = DesignResourceManager.GetColorFromStyle("Gray"),
                Content = label,
            };
        }

        private string GenerateEmptyString(string originalWord)
        {
            string result = "";
            foreach (char c in originalWord)
                result += " ";
            return result;
        }

        private void Randomize<T>(T[] items)
        {
            Random rand = new Random();
            for (int i = 0; i < items.Length - 1; i++)
            {
                int j = rand.Next(i, items.Length);
                T temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }

        private void Save()
        {
            Xamarin.Essentials.Preferences.Set("isPassphrase", true);
            var successPopup = new SuccessPopup();
            successPopup.BackButtonAction += CloseEventHandler;
            PopupNavigation.Instance.PushAsync(successPopup, false);
            OnBackButtonPressed();
        }

        private bool CheckPassPhrase()
        {
            string passPhrase = "";
            foreach (StackLayout stackLayout in filledGaps)
            {
                if (stackLayout == null)
                    return false;
                Frame fr = (Frame) stackLayout.Children[1];
                Label label = (Label) fr.Content;
                passPhrase += label.Text + " ";
            }

            var validate = EncryptedMessaging.Functions.PassphraseValidation(passPhrase);
            if (!validate)
            {
                this.DisplayToastAsync(EncryptedMessaging.Resources.Dictionary.InvalidPassphrase);
                return false;
            }

            App.Passphrase = passPhrase;
            return true;
        }

        private void Back_Clicked(object sender, EventArgs args)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }
    }
}