using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CustomViewElements
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomNoResult : ContentView
    {
  
        public CustomNoResult()
        {
            InitializeComponent();
            Image.Source = Utils.Icons.IconProvider?.Invoke("ic_noResult");

        }
     

    }
}