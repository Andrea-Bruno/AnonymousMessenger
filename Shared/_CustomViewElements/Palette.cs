using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CustomViewElements
{
    public class Palette
    {
        private static Color _CommonBackgroundColor = Color.White;
        public static Color CommonBackgroundColor {
            get => _CommonBackgroundColor;
            set => _CommonBackgroundColor = value;
        }
    }
}
