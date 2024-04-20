using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Banking
{
    public class GradientFrame : Frame
    {
        public enum GradientOrientation
        {
            Vertical,
            Horizontal
        }

        public Color StartColor
        {
            get; set;
        }

        public Color EndColor
        {
            get; set;
        }
        public GradientOrientation GradientColorOrientation
        {
            get; set;
        }

    }
}