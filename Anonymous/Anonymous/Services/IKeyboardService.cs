using System;
namespace Telegraph.Services
{
    public interface IKeyboardService
    {
        event EventHandler KeyboardIsShown;
        event EventHandler KeyboardIsHidden;
        void HideKeyboard();

    }
}
