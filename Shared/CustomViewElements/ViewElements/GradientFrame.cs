using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace CustomViewElements
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