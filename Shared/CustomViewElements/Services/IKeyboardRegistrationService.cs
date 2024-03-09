namespace CustomViewElements.Services
{
    public interface IKeyboardRegistrationService
    {
        void RegisterForKeyboardNotifications();

        void UnregisterForKeyboardNotifications();
    }
}
