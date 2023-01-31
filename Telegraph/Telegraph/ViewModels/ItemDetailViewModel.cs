namespace Telegraph.ViewModels
{
	public class ItemDetailViewModel : BaseViewModel
	{
		public EncryptedMessaging.Contact Item { get; set; }
		public ItemDetailViewModel(EncryptedMessaging.Contact item = null)
		{
			Title = item?.Name;
			Item = item;
		}
	}
}
