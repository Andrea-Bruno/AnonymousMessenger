using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace CustomViewElements
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomNoItem: ContentView
    {
  
        public CustomNoItem()
        {
            InitializeComponent();
            Image.Source = Utils.Icons.IconProvider?.Invoke("ic_noItem");
        
        }
     

    }
}