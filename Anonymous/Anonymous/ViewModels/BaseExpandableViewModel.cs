using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraph.ViewModels
{
	public class BaseExpandableViewModel : BaseViewModel
    {
		public int Id { get; set; }

		private bool _isExpanded;
		public bool isExpanded
		{
			get => isExpanded;
			set
			{
				_isExpanded = value;
				OnPropertyChanged(nameof(_isExpanded));
			}
		}
	}
}
