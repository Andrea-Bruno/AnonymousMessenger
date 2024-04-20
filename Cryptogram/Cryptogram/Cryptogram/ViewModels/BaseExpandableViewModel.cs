using System;
using System.Collections.Generic;
using System.Text;

namespace Anonymous.ViewModels
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
