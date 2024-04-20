using System.Collections.Generic;
using Xamarin.Forms;

namespace SubAppInterface
{
    public class SubAppUserInterface : Interface
    {
        public SubAppUserInterface(Page mainPage, string subAppName, List<NavigationPage> navigationPages, List<ToolbarItem> toolbarItems, List<ImageButton> buttons)
        {
            _navigationPages = navigationPages;
            _toolbarItems = toolbarItems;
            _buttons = buttons;
            _subAppName = subAppName;
            Root = mainPage;
            foreach (var page in navigationPages)
            {
                AddSubAppPage(page);
            }
        }
        private readonly Page Root;
        public void AddSubAppPage(NavigationPage navigationPage)
        {
            var tabbedPage = Root is TabbedPage ? (TabbedPage)Root : null;
            navigationPage.Focused += (s, e) => Root.Title = _subAppName;
            tabbedPage?.Children.Add(navigationPage);
            if (_toolbarItems != null)
            {
                tabbedPage.CurrentPageChanged += (s, e) =>
                {
                    if (tabbedPage != null)
                    {
                        var currentPage = tabbedPage.CurrentPage;
                        if (currentPage is NavigationPage currentNavigationPage)
                        {
                            if (_navigationPages.Contains(currentNavigationPage))
                            {
                                foreach (var toolbarItem in _toolbarItems)
                                {
                                    if (!tabbedPage.ToolbarItems.Contains(toolbarItem))
                                        tabbedPage.ToolbarItems.Add(toolbarItem);
                                }
                            }
                            else
                            {
                                foreach (var toolbarItem in _toolbarItems)
                                {
                                    _ = tabbedPage.ToolbarItems.Remove(toolbarItem);
                                }
                            }
                        }
                    }
                };
            }
        }
        public void RemoveSubAppPage(NavigationPage navigationPage)
        {
            TabbedPage tabbedPage = Root is TabbedPage ? (TabbedPage)Root : null;
            tabbedPage?.Children.Remove(navigationPage);
        }
        public void AddButton(ImageSource icon, Command onClick)
        {
            ImageButton button = new ImageButton
            {
                Source = icon,
                Command = onClick
            };
            _buttons.Add(button);
        }

        public void AddToolbarItem(string text, Command onClick, ImageSource icon)
        {
            ToolbarItem menuItem = new ToolbarItem
            {
                Text = text,
                Command = onClick,
                IconImageSource = icon
            };
            _toolbarItems.Add(menuItem);
        }

        private readonly List<ToolbarItem> _toolbarItems;
        private readonly List<ImageButton> _buttons;

        private readonly string _subAppName;
        string Interface.SubAppName => _subAppName;

        private readonly List<NavigationPage> _navigationPages;
        NavigationPage[] Interface.NavigationPages => _navigationPages.ToArray();

        ImageButton[] Interface.Buttons => _buttons.ToArray();

        ToolbarItem[] Interface.ToolbarItem => _toolbarItems.ToArray();

    }
}
