using Xamarin.Forms;

namespace XamarinShared
{

    public class PaletteSetting
    {
        public Color ForegroundColor;
        public Color MainBackgroundColor;
        public Color SecondaryBackgroundColor;
        public Color BackgroundColor;
        public Color CommonBackgroundColor;
        public Color SecondaryTextColor;
        public Color ThemeColor;

    }

    public class Palette
    {
        public static PaletteSetting Colors;

        // ************* Andrea : Set at start defaults value or pass a different palette customized at initialization of library

        public static Color MainTextColor(bool isOutgoing)
        {
            if (Colors.ThemeColor == null)
                return Color.Default;
            return isOutgoing ? Colors.MainBackgroundColor : Colors.ForegroundColor;
        }

      
    
        public static Color BoxView
        {
            get => Color.Orange;
        }


        public static Color SliderThumbColor(bool isOutgoing)
        {
            return isOutgoing ? Colors.MainBackgroundColor : Colors.SecondaryBackgroundColor;
        }

        public static Color SliderMinimumColor(bool isOutgoing)
        {
            if (Colors.ThemeColor == null)
                return Color.Default;
            return isOutgoing ? Colors.SecondaryBackgroundColor : Colors.ForegroundColor;
        }

        public static Color SliderMaximumColor(bool isOutgoing)
        {
            return isOutgoing ? Colors.MainBackgroundColor : Colors.SecondaryBackgroundColor; 
        }

        public static Color FrameBackgroundColor(bool isOutgoing)
        {
            return isOutgoing ? Colors.ForegroundColor : Colors.BackgroundColor;
        }

        public static Color UnreadedMessagesLabelTextColor
        {
            get => Colors.SecondaryTextColor;
        }
        public static Color UnreadedMessagesLabelBackgroundColor
        {
            get => Colors.CommonBackgroundColor;
        }

        public static Color DateLabelTextColor
        {
            get {
                if (Colors.ThemeColor == null)
                {
                    return Color.Default;
                }
                return Colors.ForegroundColor;
            }
        }

        public static Color FrameShadowColor
        {
            get => Colors.MainBackgroundColor;
        }

        public static Color CheckBoxColor
        {
            get => Colors.ThemeColor;
        }

        public static Color CommonTextColor
        {
            get => Colors.MainBackgroundColor;
        }

        public static Color CommonFrameBackground
        {
            get => Colors.CommonBackgroundColor;
        }

        public static Color AudioTimerColor
        {
            get => Colors.SecondaryBackgroundColor;
        }

        public static Color PhoneContactBottomTextColor
        {
            get => Colors.MainBackgroundColor;
        }

        public static Color PhoneContactBottomBackgroundColor
        {
            get => Colors.CommonBackgroundColor;
        }
        public static Color PhoneContactBottomDividerColor
        {
            get => Colors.SecondaryBackgroundColor;
        }
    }
}
