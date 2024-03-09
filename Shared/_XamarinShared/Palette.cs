using Xamarin.Forms;

namespace XamarinShared
{
    /// <summary>
    /// This class sets the default color scheme for the application. 
    /// </summary>
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
        /// <summary>
        /// Object to get all the colors of the application 
        /// </summary>
        public static PaletteSetting Colors;

        // ************* Andrea : Set at start defaults value or pass a different palette customized at initialization of library


        /// <summary>
        /// Returns the text message background color depending on the if the message is delievered or sent.
        /// </summary>
        /// <param name="isOutgoing">identify if message is been sent </param>
        /// <returns>Color</returns>
        public static Color MainTextColor(bool isOutgoing)
        {
            if (Colors.ThemeColor == null)
                return Color.Default;
            return isOutgoing ? Colors.MainBackgroundColor : Colors.ForegroundColor;
        }

      
        /// <summary>
        /// Get default color of a box view
        /// </summary>
        public static Color BoxView
        {
            get => Color.Orange;
        }

        /// <summary>
        /// Returns the audio message slider background color depending on the if the message is delievered or sent.
        /// </summary>
        /// <param name="isOutgoing">identify if message is been sent</param>
        /// <returns> Color </returns>
        public static Color SliderThumbColor(bool isOutgoing)
        {
            return isOutgoing ? Colors.MainBackgroundColor : Colors.SecondaryBackgroundColor;
        }

        /// <summary>
        /// Returns the audio message slider minimun background color depending on the if the message is delievered or sent.
        /// </summary>
        /// <param name="isOutgoing">identify if message is been sent</param>
        /// <returns> Color </returns>
        public static Color SliderMinimumColor(bool isOutgoing)
        {
            if (Colors.ThemeColor == null)
                return Color.Default;
            return isOutgoing ? Colors.SecondaryBackgroundColor : Colors.ForegroundColor;
        }

        /// <summary>
        /// Returns the audio message slider maximum background color depending on the if the message is delievered or sent.
        /// </summary>
        /// <param name="isOutgoing">identify if message is been sent</param>
        /// <returns> Color </returns>
        public static Color SliderMaximumColor(bool isOutgoing)
        {
            return isOutgoing ? Colors.MainBackgroundColor : Colors.SecondaryBackgroundColor; 
        }

        /// <summary>
        /// Returns the Frame background color depending on the if the message is delievered or sent.
        /// </summary>
        /// <param name="isOutgoing">identify if message is been sent</param>
        /// <returns> Color </returns>
        public static Color FrameBackgroundColor(bool isOutgoing)
        {
            return isOutgoing ? Colors.ForegroundColor : Colors.BackgroundColor;
        }

        /// <summary>
        /// Get unread message text color.
        /// </summary>
        public static Color UnreadedMessagesLabelTextColor
        {
            get => Colors.SecondaryTextColor;
        }

        /// <summary>
        /// Unread message label background color
        /// </summary>
        public static Color UnreadedMessagesLabelBackgroundColor
        {
            get => Colors.CommonBackgroundColor;
        }

        /// <summary>
        /// gets date label text color.
        /// </summary>
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

        /// <summary>
        /// Get frame shadow color.
        /// </summary>
        public static Color FrameShadowColor
        {
            get => Colors.MainBackgroundColor;
        }

        /// <summary>
        /// Get checkbox color.
        /// </summary>
        public static Color CheckBoxColor
        {
            get => Colors.ThemeColor;
        }

        /// <summary>
        /// Get common text color.
        /// </summary>
        public static Color CommonTextColor
        {
            get => Colors.MainBackgroundColor;
        }

        /// <summary>
        /// Get common frame background color.
        /// </summary>
        public static Color CommonFrameBackground
        {
            get => Colors.CommonBackgroundColor;
        }

        /// <summary>
        /// Get audio timer text color.
        /// </summary>
        public static Color AudioTimerColor
        {
            get => Colors.SecondaryBackgroundColor;
        }

        /// <summary>
        /// Get contact button text color.
        /// </summary>
        public static Color PhoneContactBottomTextColor
        {
            get => Colors.MainBackgroundColor;
        }

        /// <summary>
        ///  Get contact button background color.
        /// </summary>
        public static Color PhoneContactBottomBackgroundColor
        {
            get => Colors.CommonBackgroundColor;
        }

        /// <summary>
        ///  Get contact buttons divide color.
        /// </summary>
        public static Color PhoneContactBottomDividerColor
        {
            get => Colors.SecondaryBackgroundColor;
        }
    }
}
