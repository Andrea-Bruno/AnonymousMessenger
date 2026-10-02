using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace CustomViewElements
{
    public class Palette
    {
        private static Color _CommonBackgroundColor = Colors.White;
        public static Color CommonBackgroundColor {
            get => _CommonBackgroundColor;
            set => _CommonBackgroundColor = value;
        }
    }
}
