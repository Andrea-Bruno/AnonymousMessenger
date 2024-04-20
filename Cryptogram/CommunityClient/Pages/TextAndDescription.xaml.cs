using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CommunityClient.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TextAndDescription : ContentPage
    {
        //internal enum ElementType
        //{
        //    Page,
        //    Group,
        //    Social,
        //}
        private bool IsTopic;
        private BaseCommunity Community;
        internal void Initialize(BaseCommunity community, EditableProperties element = null, string title = null)
        {
            InitializeComponent();
            Community = community;
            IsTopic = element == null;
            if (IsTopic)
            {
                CreateNew.Text = Localization.Resources.Dictionary.CreateNewTopic;
                EmoticonLabel.IsVisible = false;
                Emoticon.IsVisible = false;
                TextLabel.Text = Localization.Resources.Dictionary.Title;
            }
            Title.Text = title;
            Title.IsVisible = title != null;

            if (Emoticon.IsVisible)
                Emoticon.TextChanged += (obj, sender) =>
                {
                    if ((Emoticon.Text.Length == 1 && char.IsSymbol(Emoticon.Text[0])) || (Emoticon.Text.Length == 2 && char.IsSurrogatePair(Emoticon.Text[0], Emoticon.Text[1])))
                        return;
                    Emoticon.Text = "";
                };
            void refresh(object obj, object sender)
            {
                CreateNew.IsEnabled = (!Text.IsVisible || !string.IsNullOrEmpty(Text.Text)) && (!Description.IsVisible || !string.IsNullOrEmpty(Description.Text));
            }
            if (Text.IsVisible)
                Text.TextChanged += refresh;
            if (Description.IsVisible)
                Description.TextChanged += refresh;
            if (element != null)
            {
                Emoticon.Text = element.Emoticon;
                Text.Text = element.Text;
                Description.Text = element.Description;
            }
            if (IsTopic)
            {
                //Create a new topic
                CreateNew.Clicked += (obj, sender) =>
                {
                    community.CreateNewTopic(Text.Text, Description.Text);
                    //if (community.Type == CommunityType.Group)
                    //    element = new Group() { Name = Name.Text, Description = Description.Text, Emoticon = Emoticon.Text, PrivateKey = BaseCommunity.GeneratePrivateKey() };
                    //if (community.Type == CommunityType.Group)
                    //    element = new Page() { Name = Name.Text, Description = Description.Text, Emoticon = Emoticon.Text, PrivateKey = BaseCommunity.GeneratePrivateKey() };
                    //element.AddToCollection();
                    //element.SaveToCloud();
                    Navigation.PopAsync();
                };
                return;
            }
            CreateNew.IsVisible = false;
        }
    }
}