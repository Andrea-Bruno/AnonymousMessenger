using System;
namespace CustomViewElements.Services
{
    public interface IKeyboardService
    {
        event EventHandler KeyboardIsShown;
        event EventHandler KeyboardIsHidden;
        void HideKeyboard();

    }
}
