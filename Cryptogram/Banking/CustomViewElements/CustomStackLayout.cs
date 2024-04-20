using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Banking
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
