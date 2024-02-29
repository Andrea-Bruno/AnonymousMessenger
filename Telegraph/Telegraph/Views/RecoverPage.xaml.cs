using System;
using Plugin.Toast;
using Xamarin.Forms;
using CustomViewElements;
using Telegraph.DesignHandler;

namespace Telegraph.Views
{
    public partial class RecoverPage : BasePage
    {
        private TapGestureRecognizer addTapGestureRecognizer;
        private TapGestureRecognizer removeTapGestureRecognizer;
        private StackLayout[] filledGaps = new StackLayout[12];

        public RecoverPage()
        {
            InitializeComponent();
            InitRecognizer();
        }

        private void InitRecognizer()
        {
            addTapGestureRecognizer = new TapGestureRecognizer();
            removeTapGestureRecognizer = new TapGestureRecognizer();
            addTapGestureRecognizer.NumberOfTapsRequired = 1;
            removeTapGestureRecognizer.NumberOfTapsRequired = 1;
            removeTapGestureRecognizer.Tapped += FrameRemoveTapped;
            SetContinueButtonStatus(false);
        }

        protected override void OnAppearing() => base.OnAppearing();

        private void FrameRemoveTapped(object sender, EventArgs e)
        {
            StackLayout stackLayout = (StackLayout)sender;
            Frame frame = (Frame)stackLayout.Children[1];
            Label label = (Label)frame.Content;
            if (!string.IsNullOrEmpty(label.Text.Trim()))
            {
                for (int i = 0; i < filledGaps.Length; i++)
                    if (filledGaps[i] == stackLayout)
                    {
                        filledGaps[i] = null;

                        break;
                    }
                SetContinueButtonStatus(false);
                label.Text = GenerateEmptyString(label.Text);
            }
            PrepareEntryPlaceHolder();
        }

        private StackLayout FillGaps(string word, int index)
        {
            if (Gaps.Children.Count - 1 >= index && Gaps.Children.Count > 0)
            {
                StackLayout layout = (StackLayout)Gaps.Children[index];
                Frame fr = (Frame)layout.Children[1];
                Label label = (Label)fr.Content;
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
                FontFamily = "PoppinsRegular",
                Text = (index + 1) + ".",
                HeightRequest = 38,
                Margin = 0,
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
            Frame frame = new Frame()
            {
                CornerRadius = 24,
                MinimumWidthRequest = 40,
                Margin = 5,
                Padding = new Thickness(8, 0, 9, 0),
                HeightRequest = 28,
                BorderColor = DesignResourceManager.GetColorFromStyle("Gray"),
                BackgroundColor = Color.Transparent,

            };
            Label label = new Label()
            {
                FontSize = 13,
                FontFamily = "PoppinsLight",
                Text = word,
                Margin = new Thickness(6, 3, 6, 3),
                MinimumWidthRequest = 50,
                TextColor = Color.White,
            };
            frame.Content = label;
            return frame;
        }

        private string GenerateEmptyString(string originalWord)
        {
            string result = "";
            foreach (char c in originalWord)
                result += " ";
            return result;
        }

        private void Restore_Clicked(object sender, EventArgs e)
        {
            if (CheckPassPhrase())
            {
                ShowProgressDialog();
                var timer = new System.Threading.Timer((object obj) => { Device.BeginInvokeOnMainThread(() => Save()); }, null, 100, System.Threading.Timeout.Infinite);
            }
        }

        private void Save()
        {
            Xamarin.Essentials.Preferences.Set("isPassphrase", true);
            Application.Current.MainPage.Navigation.PushAsync(new NavigationTappedPage(), false);
            Navigation.RemovePage(this);
        }

        private bool CheckPassPhrase()
        {
            string passPhrase = "";
            foreach(StackLayout stackLayout in filledGaps)
            {
                if (stackLayout == null)
                    return false;
                Frame fr = (Frame)stackLayout.Children[1];
                Label label = (Label)fr.Content;
                passPhrase += label.Text + " ";
            }
            var validate = EncryptedMessaging.Functions.PassphraseValidation(passPhrase);
            if (!validate)
            {
                CrossToastPopUp.Current.ShowToastMessage(EncryptedMessaging.Resources.Dictionary.InvalidPassphrase);
                return false;
            }
            App.Passphrase = passPhrase;
            return true;
        }

        private void PhraseEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
              
            AddWord.IsVisible = !string.IsNullOrEmpty(PhraseEntry.Text); 

        }

        private void AddWord_Clicked(object sender, EventArgs e)
        {
                    
            if (!string.IsNullOrWhiteSpace(PhraseEntry.Text))
            {
                for (int i = 0; i < filledGaps.Length; i++)
                    if (filledGaps[i] == null)
                    {
                        filledGaps[i] = FillGaps(PhraseEntry.Text, i);
                        PhraseEntry.Text = "";
                        PrepareEntryPlaceHolder();
                        if(i == filledGaps.Length - 1)
                        {
                            SetContinueButtonStatus(true);
                        }
                        return;
                    }
                PhraseEntry.Text = "";
                PrepareEntryPlaceHolder();
            }
          
        }

        private void PrepareEntryPlaceHolder()
        {
            var s ="Insert the {0} word here";
            for (int i = 0; i < filledGaps.Length; i++)
                if (filledGaps[i] == null)
                {
                    PhraseEntry.Placeholder= string.Format(s, i+1);
                    break;
                }
        }

        private void SetContinueButtonStatus(bool isEnable)
        {
            if (isEnable && !ContinueButton.IsEnabled)
            {
                ContinueButton.BackgroundColor = DesignResourceManager.GetColorFromStyle("Theme");
                ContinueButton.TextColor = DesignResourceManager.GetColorFromStyle("Color1");
                ContinueButton.IsEnabled = true;
                AddWordFrame.IsVisible = false;
            }
            else if (!isEnable && ContinueButton.IsEnabled)
            {
                ContinueButton.BackgroundColor = DesignResourceManager.GetColorFromStyle("BackgroundSecondary");
                ContinueButton.TextColor = DesignResourceManager.GetColorFromStyle("WhiteColor");
                ContinueButton.IsEnabled = false;
                AddWordFrame.IsVisible = true;
            }
        }

        private void Back_Clicked(object sender, EventArgs args) => OnBackButtonPressed();

    }
}