using System;
using Xamarin.Forms;

namespace Telegraph
{
    [Serializable()]
    public class CustomStackLayout : StackLayout
    {
        public delegate void LongPress(object sender);
        public event LongPress LongPressed;

        public void InvokeLongPressedEvent()
        {
            LongPressed(this);
        }
    }
}
