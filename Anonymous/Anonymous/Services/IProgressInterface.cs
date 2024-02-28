namespace Telegraph.Services
{
    public interface IProgressInterface
    {
        void Show(string title = "Loading");

        void Hide();
    }
}
