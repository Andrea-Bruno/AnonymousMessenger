namespace Cryptogram.ViewModels
{
    using Xamarin.Forms;


    public class QuestionAnswersInnerViewModel : BaseExpandableViewModel
	{
		public string Title { get; set; }
		public string Description { get; set; }


		private ImageSource _icon;
		public ImageSource Icon
		{
			get => _icon;
			set
			{
				_icon = value;
				OnPropertyChanged(nameof(Icon));
			}
		}

    }
}
