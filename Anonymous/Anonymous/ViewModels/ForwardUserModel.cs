namespace Telegraph.ViewModels
{
	public class ForwardUserModel : BaseViewModel
	{

		public EncryptedMessaging.Contact contact { get; set; }

		public bool isVisible { get; set; }


		public ForwardUserModel(EncryptedMessaging.Contact contact, bool isVisible)
		{

			this.contact = contact;
			this.isVisible = isVisible;
		}


	}


}