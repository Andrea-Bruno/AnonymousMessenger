using Xamarin.Forms;

namespace Telegraph.ViewModels
{
	public class LanguageChooseModel : BaseViewModel
	{

		public string LanguageName { get; set; }

		public ImageSource CountryFlag { get; set; }

		private bool _isSelected;
		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				_isSelected = value;
				OnPropertyChanged(nameof(IsSelected));
			}
		}

		public string Locale { get; set; }

		public LanguageChooseModel(string language, ImageSource countryFlag, bool isSelected, string locale)
		{
			LanguageName = language;
			CountryFlag = countryFlag;
			IsSelected = isSelected;
			Locale = locale;
		}


	}


}