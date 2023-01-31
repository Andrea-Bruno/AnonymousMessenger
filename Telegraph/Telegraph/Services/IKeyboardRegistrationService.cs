namespace Telegraph.Services
{
    public interface IKeyboardRegistrationService
    {
        void RegisterForKeyboardNotifications();

        void UnregisterForKeyboardNotifications();
    }
}
