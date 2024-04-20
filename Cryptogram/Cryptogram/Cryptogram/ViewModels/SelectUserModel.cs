using System.ComponentModel;

namespace Anonymous.ViewModels
{
	public class SelectUserModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		private EncryptedMessaging.Contact _contact;
		public EncryptedMessaging.Contact contact
        {
            get { return _contact; }
            set {
				_contact = value;
				PropertyChanged(this, new PropertyChangedEventArgs("contact"));
			}
        }

		private bool _isVisible;
		public bool isVisible
        {
            get { return _isVisible; }
			set
            {
				_isVisible = value;
				PropertyChanged(this, new PropertyChangedEventArgs("isVisible"));
            }
        }

		private bool _isShownInList = true;
		public bool isShownInList
		{
			get { return _isShownInList; }
			set
			{
				_isShownInList = value;
				PropertyChanged(this, new PropertyChangedEventArgs("isShownInList"));
			}
		}

		public SelectUserModel(EncryptedMessaging.Contact contact, bool isVisible)
		{

			this.contact = contact;
			this.isVisible = isVisible;
		}
	}


}