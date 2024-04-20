using System;
using System.IO;
using CustomViewElements;
using Utils;
using Xamarin.Forms;

namespace Anonymous.Views
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
        private void Back_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }

        void OnSwiped(object sender, SwipedEventArgs e) => OnBackButtonPressed();
    }
}
