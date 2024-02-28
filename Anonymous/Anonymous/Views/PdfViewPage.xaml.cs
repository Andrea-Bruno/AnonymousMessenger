using System;
using System.IO;
using CustomViewElements;
using Xamarin.Forms;

namespace Telegraph.Views
{
    public partial class PdfViewPage : BasePage
    {
        public PdfViewPage(byte[] data)
        {
            InitializeComponent();
            pdfViewerControl.LoadDocument(new MemoryStream(data));
        }

        private void DocumentLoaded(object arg1, EventArgs arg2)
        {
            HideProgressDialog();
        }
        private void Back_Clicked(object sender, EventArgs e) => OnBackButtonPressed();

        void OnSwiped(object sender, SwipedEventArgs e) => OnBackButtonPressed();
    }
}
