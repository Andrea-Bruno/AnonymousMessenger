namespace AnonymousWhiteLabel.UWP
{
	public sealed partial class MainPage
	{
		public MainPage()
		{
			this.InitializeComponent();

            // ZXing.Net.Mobile.Forms.WindowsUniversal.Platform.Init();

            // NOTE: https://github.com/Redth/ZXing.Net.Mobile/issues/894
            ZXing.Net.Mobile.Forms.WindowsUniversal.ZXingScannerViewRenderer.Init();
            ZXing.Net.Mobile.Forms.WindowsUniversal.ZXingBarcodeImageViewRenderer.Init();

            LoadApplication(new AnonymousWhiteLabel.App());
		}
	}
}
