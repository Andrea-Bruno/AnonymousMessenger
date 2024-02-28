
namespace Telegraph.ViewModels
{
    public class CurrencyModel : BaseViewModel
    {
        public int id { get; set; }

        public string Currency { get; set; }

        public bool isVisible { get; set; }

        public string icon { get; set; }

        public CurrencyModel(int id, string Currency, bool isVisible, string icon)
        {
            this.id = id;
            this.Currency = Currency;
            this.isVisible = isVisible;
            this.icon = icon;
        }
    }
}
