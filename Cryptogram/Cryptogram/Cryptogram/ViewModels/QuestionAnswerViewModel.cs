using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Utils;

namespace Anonymous.ViewModels
{
	public class QuestionAnswerViewModel : BaseExpandableViewModel
	{
		
		public string Title { get; set; }

		public ObservableCollection<QuestionAnswersInnerViewModel> QuestionList { get; set; }
		

	

		private ImageSource _icon;
		public ImageSource Icon {
			get => _icon;
			set
			{
				_icon = value;
				OnPropertyChanged(nameof(Icon));
			}
		}
	

	}
}