using System;
using EncryptedMessaging;
using EncryptedMessaging.Resources;
using Xamarin.Forms;
using AnonymousWhiteLabel.Views;
using System.Collections.Generic;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using NavigationPage = Xamarin.Forms.NavigationPage;
using Page = Xamarin.Forms.Page;

namespace AnonymousWhiteLabel.Pages
{
    internal class ContactsPage : ContentPage
    {
        //========= Setup =======	
        public static TapGestureRecognizer tapChat = new TapGestureRecognizer();
        //======= End Setup =====	

        private ToolbarItem _done;
        private Label groupNameLabel;
        private Editor groupNameEntry;
        private StackLayout groupNameLayout;

        public ContactsPage()
        {
            On<iOS>().SetUseSafeArea(true);
            // var addedtoNavigation = false;

            groupNameLabel = new Label
            {
                Text = "Please enter group name and select users:",
                FontAttributes = FontAttributes.Bold,
                //Margin = new Thickness(0, 0, 0, 5)
            };
            groupNameEntry = new Editor
            {
                Placeholder = "Autogenerates if not specified",
            };
            groupNameLayout = new StackLayout { Children = { groupNameLabel, groupNameEntry }, Margin = new Thickness(5, 10, 5, 10) };

            InitToolbar();


            ChatPage = new ChatPage();

            Appearing += (sender, e) =>
            {
                _list.SelectedItem = null;
                groupNameLayout.IsVisible = false;
            };

            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Children = { groupNameLayout, _list }
            };

            tapChat.Tapped += (sender, e) =>
            {
                if (!((sender as Grid)?.BindingContext is Contact contact)) return;

                if (_list.SelectionMode == SelectionMode.None)
                {
                    _list.SelectedItem = contact;
                    ChatPage.SetCurrentContact(contact);
                    Navigation.PushAsync(ChatPage);
                }
                else if (_list.SelectionMode == SelectionMode.Multiple && (Device.RuntimePlatform == Device.iOS ||
                                                                           Device.RuntimePlatform == Device.Android))
                {
                    if (!contact.IsGroup)
                    {
                        if (_list.SelectedItems.Contains(contact))
                            _list.SelectedItems.Remove(contact);
                        else
                            _list.SelectedItems.Add(contact);
                    }


                    _done.IsEnabled = _list.SelectedItems.Count >= 2;
                }
            };
            if (Device.RuntimePlatform == Device.UWP)
            {
                _list.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                {
                    _done.IsEnabled = _list.SelectedItems.Count >= 2;
                };
            }
        }

        private void InitToolbar()
        {
            var add = new ToolbarItem
            {
                Text = Dictionary.Add
            };
            add.Command = new Command(() => { Navigation.PushAsync(new ContactPage()); });
            _done = new ToolbarItem
            {
                Text = "Done",
            };

            var abort = new ToolbarItem
            {
                Text = "Abort",
            };

            var createGroup = new ToolbarItem
            {
                Text = "Create Group",
            };
            createGroup.Command = new Command(() =>
            {
                ToolbarItems.Clear();
                ToolbarItems.Add(_done);
                ToolbarItems.Add(abort);
                _list.SelectionMode = SelectionMode.Multiple;
                _done.IsEnabled = false;
                groupNameLayout.IsVisible = true;

            });

            ToolbarItems.Add(add);
            ToolbarItems.Add(createGroup);


            _done.Command = new Command(() =>
            {
                var selectedContacts = new List<Contact>();
                foreach (var item in _list.SelectedItems)
                {
                    selectedContacts.Add((Contact) item);
                }

                App.Context.Contacts.AddContact(selectedContacts, name: groupNameEntry.Text != null || groupNameEntry.Text != "" ? groupNameEntry.Text : null);
                _list.SelectedItems.Clear();
                _list.SelectionMode = SelectionMode.None;
                ToolbarItems.Clear();
                ToolbarItems.Add(add);
                ToolbarItems.Add(createGroup);
                groupNameLayout.IsVisible = false;
                groupNameEntry.Text = "";

            });

            abort.Command = new Command(() =>
            {
                _list.SelectedItems.Clear();
                _list.SelectionMode = SelectionMode.None;
                ToolbarItems.Clear();
                ToolbarItems.Add(add);
                ToolbarItems.Add(createGroup);
                groupNameLayout.IsVisible = false;

            });
        }

        private readonly CollectionView _list = new CollectionView
        {
            ItemsSource = App.Context.Contacts.GetContacts(),
            ItemTemplate = _contactDataTemplate,
            Margin = new Thickness(10, 0, 10, 0),
        };

        public class IntToFontAttributes : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture) => (FontAttributes)value;

            public object ConvertBack(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture) => (int)value;
        }

        public class IsNotZero : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture) => (int)value != 0;

            public object ConvertBack(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture) => (bool)value ? 1 : 0;
        }

        private static readonly DataTemplate _contactDataTemplate = new DataTemplate(() =>
        {
            Grid grid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 0,
                RowDefinitions =
                {
                    new RowDefinition {Height = new GridLength(25)},
                    new RowDefinition { },
                },

                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(50)},
                    new ColumnDefinition { }
                },
            };
            var avatar = new Image()
            {
                Aspect = Aspect.AspectFill,
                WidthRequest = 50,
                HeightRequest = 50,
                MinimumHeightRequest = 50,
                MinimumWidthRequest = 50,
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.End,
            };
            avatar.SetBinding(Image.SourceProperty, "Avatar", converter: new BytesToImageSourceConverter());
            var nameLabel = new Label {FontAttributes = FontAttributes.Bold};
            nameLabel.SetBinding(Label.TextProperty, nameof(Contact.Name));
#if DEBUG
            var checkbox = new CheckBox() {IsChecked = true,};
            checkbox.SetBinding(CheckBox.IsCheckedProperty, "IsGroup");
            if (checkbox.IsChecked)
                nameLabel.SetBinding(Label.TextProperty, "Group" + nameof(Contact.Name));
#endif
            var lastMessage = new Label();
            lastMessage.SetBinding(Label.TextProperty, nameof(Contact.LastMessagePreview));
            lastMessage.SetBinding(Label.FontAttributesProperty, nameof(Contact.LastMessageFontAttributes),
                BindingMode.Default, new IntToFontAttributes());

            var lastMessageTimeDistance = new Label {HorizontalTextAlignment = TextAlignment.End};
            lastMessageTimeDistance.SetBinding(Label.TextProperty, nameof(Contact.LastMessageTimeDistance));

            var ballon = new Frame
            {
                Margin = 0,
                Padding = 0,
                WidthRequest = 18,
                HeightRequest = 18,
                CornerRadius = 9,
                BackgroundColor = Color.Red,
                HorizontalOptions = LayoutOptions.Start
            };
            ballon.SetBinding(IsVisibleProperty, nameof(Contact.UnreadMessages), BindingMode.Default, new IsNotZero());
            var unreadMessages = new Label
            {
                FontSize = 9,
                BackgroundColor = Color.Transparent,
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            unreadMessages.SetBinding(Label.TextProperty, nameof(Contact.UnreadMessages));
            ballon.Content = unreadMessages;

            Grid.SetRowSpan(avatar, 2);

            grid.Children.Add(avatar);
            grid.Children.Add(nameLabel, 1, 0);
            grid.Children.Add(ballon, 1, 0);
            grid.Children.Add(lastMessageTimeDistance, 1, 0);
            grid.Children.Add(lastMessage, 1, 1);
            grid.GestureRecognizers.Add(tapChat);

            return grid;
        });

        public class BytesToImageSourceConverter : IValueConverter
        {
            public object Convert(object pobjValue, Type pobjTargetType, object pobjParameter,
                System.Globalization.CultureInfo pobjCulture)
            {
                ImageSource objImageSource;
                if (pobjValue != null)
                {
                    byte[] bytImageData = (byte[]) pobjValue;
                    objImageSource = ImageSource.FromStream(() => new System.IO.MemoryStream(bytImageData));
                }
                else
                {
                    objImageSource = null;
                }

                return objImageSource;
            }

            public object ConvertBack(object pobjValue, Type pobjTargetType, object pobjParameter,
                System.Globalization.CultureInfo pobjCulture)
            {
                throw new NotImplementedException();
            }
        }

        public static ChatPage ChatPage;
    }
}