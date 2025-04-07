using System;
using System.IO;
using CustomViewElements;
using Utils;
using Xamarin.Forms;

namespace Cryptogram.Views
{
    public partial class PdfViewPage : BasePage
    {
        public PdfViewPage(byte[] data)
        {
            InitializeComponent();
            var pdfBase64 = Convert.ToBase64String(data);
            var htmlSource = new HtmlWebViewSource
            {
                Html = $@"
                        <html>
                        <body>
                            <embed src='data:application/pdf;base64,{pdfBase64}' width='100%' height='100%' type='application/pdf' />
                        </body>
                        </html>"
            };
            pdfViewerControl.Source = htmlSource;
        }

        private void DocumentLoaded(object sender, WebNavigatedEventArgs e)
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
