using System;
using Microsoft.Maui.Controls;

namespace CustomViewElements
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