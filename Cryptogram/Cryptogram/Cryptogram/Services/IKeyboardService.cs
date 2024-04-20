using System;
namespace Anonymous.Services
{
    public interface IKeyboardService
    {
        event EventHandler KeyboardIsShown;
        event EventHandler KeyboardIsHidden;
        void HideKeyboard();

    }
}
