using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CustomViewElements
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyCustomToolbar : ContentView
    {
        private static ImageSource BackButtonImageSource = Utils.Icons.IconProvider?.Invoke("ic_new_back_icon");//  prevent additional loading in every page

        public string Title
        {
            get => Title;
            set => TitleLabel.Text = value;
        }
        public int RightBtnCount
        {
            get => RightBtnCount;
            set
            {
                for (var i = 0; i < value; i++)
                {
                    RightButtonsLayout.ColumnDefinitions.Add(new ColumnDefinition());
                }
            }
        }

        public event EventHandler OnBackBtnClicked
        {
            add
            {
                foreach (ImageButton button in LeftButtonsLayout.Children) // prevent button duplication
                    if (button.Source == BackButtonImageSource)
                        return;
                AddLeftButton(BackButtonImageSource, value);
            }
            remove
            {
                
            }
        }

        public MyCustomToolbar()
        {
            InitializeComponent();
            SearchIcon.Source = Utils.Icons.IconProvider?.Invoke("ic_toolbar_search");
            toolbarSearchClear.Source = Utils.Icons.IconProvider?.Invoke("ic_toolbar_search_clear");
            InitToolbar();
        }

        private void InitToolbar()
        {
            toolbar.IsVisible = true;
            ToolbarSearchLayout.IsVisible = false;
            SearchEntry.Text = "";
        }

        public void AddRightButton(int pos, ImageSource imageSource, EventHandler click)
        {
            var btn = CreateImageButton(imageSource);
            btn.Clicked += click;
            RightButtonsLayout.Children.Add(btn, pos, 0);
        }

        public void AddLeftButton(ImageSource imageSource, EventHandler click)
        {
            var btn = CreateImageButton(imageSource);
            btn.Clicked += click;
            
            LeftButtonsLayout.Children.Add(btn);
        }
       

        public void AddNewChatButton(EventHandler click)
        {
            AddRightButton(1, Utils.Icons.IconProvider?.Invoke("ic_add_new_chat"), (sender, args) =>
            {
                pancake.IsVisible = !pancake.IsVisible;
                RightButtonsLayout.Children[1].IsVisible = !pancake.IsVisible;
                RightButtonsLayout.Children[1].IsVisible = !pancake.IsVisible;
                click?.Invoke(sender, args);
            });
        }

        public void AddSearchButton(EventHandler click)
        {
            AddRightButton(0, Utils.Icons.IconProvider?.Invoke("ic_toolbar_search"), (sender, args) =>
            {
                ToolbarSearchLayout.IsVisible = true;
                pancake.IsVisible = false;
                SearchEntry.Text = "";
                toolbar.IsVisible = false;
                SearchEntry.Focus();
                click?.Invoke(sender, args);
            });
        }

        private ImageButton CreateImageButton(ImageSource imageSource)
        {
            return new ImageButton { Source = imageSource,
                Padding = new Thickness(12),
                HeightRequest = 48,
                WidthRequest = 48,
                BackgroundColor = Color.Transparent };
        }

        private void SearchClearButton_Clicked(object sender, EventArgs e)
        {
            InitToolbar();
        }

        public void ClearViewState()
        {
            pancake.IsVisible = false;
            RightButtonsLayout.Children[1].IsVisible = true;
            InitToolbar();
        }
    }
}