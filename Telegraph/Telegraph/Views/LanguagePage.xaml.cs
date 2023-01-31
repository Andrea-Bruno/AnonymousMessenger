using System;
using System.Collections.Generic;
using System.Globalization;
using CustomViewElements;
using Plugin.Toast;
using Telegraph.DesignHandler;
using Telegraph.Services;
using Telegraph.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Telegraph.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LanguagePage : BasePage
    {
        private readonly List<LanguageChooseModel> _languageChoose = new List<LanguageChooseModel>();
        private LanguageChooseModel _clickedItem;
        private readonly string previousLanguage;
        private string lang;
        public LanguagePage()
        {
            InitializeComponent();
            previousLanguage = lang = Xamarin.Essentials.Preferences.Get("language", null);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _languageChoose.Clear();
            _languageChoose.Add(new LanguageChooseModel(Localization.Resources.Dictionary.English, DesignResourceManager.GetImageSource("ic_english_flag.png"),  false, "en-US"));
            _languageChoose.Add(new LanguageChooseModel(Localization.Resources.Dictionary.Italian, DesignResourceManager.GetImageSource("ic_italian_flag.png"), false, "it-IT"));
            _languageChoose.Add(new LanguageChooseModel(Localization.Resources.Dictionary.German,  DesignResourceManager.GetImageSource("ic_german_flag.png"), false, "de-DE"));
            _languageChoose.Add(new LanguageChooseModel(Localization.Resources.Dictionary.Spanish, DesignResourceManager.GetImageSource("ic_spanish_flag.png"), false, "es-ES"));
            _languageChoose.Add(new LanguageChooseModel(Localization.Resources.Dictionary.French,  DesignResourceManager.GetImageSource("ic_french_flag.png"), false, "fr-FR"));
            LoadTicks();
            Language.ItemsSource = _languageChoose;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();          
        }
       
        private void LoadTicks()
        {
            if (string.IsNullOrWhiteSpace(lang))
                lang = CultureInfo.CurrentCulture.ToString().Substring(0, 2);
            foreach(LanguageChooseModel model in _languageChoose)
            {
                model.IsSelected = model.Locale == lang;
            }
        }
        private void OnItemSelected(object sender, ItemTappedEventArgs args)
        {
            _clickedItem = args.Item as LanguageChooseModel;
            lang = _clickedItem.Locale;
            LoadTicks();
            if (sender is ListView lv) lv.SelectedItem = null;
        }

        private void Save()
        {
            if (previousLanguage != lang)
            {
                Xamarin.Essentials.Preferences.Set("language",lang);
            }
        }

        private void Save_Clicked(object sender, EventArgs e)
        {
            Save();
            OnBackButtonPressed();
            if (previousLanguage != lang)
            {
                CrossToastPopUp.Current.ShowToastMessage(Localization.Resources.Dictionary.RestartApplication);
            }
        }

        private void Back_Clicked(object sender, EventArgs e) => OnBackButtonPressed();
    }
}