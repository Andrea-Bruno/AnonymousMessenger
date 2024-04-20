using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SecureKeyboard
{
    public static class VirtualKeyboard
    {
        public static ViewSecureKeyboard ViewKeyboard;
        public delegate void ViewSecureKeyboard(ContentView contentView);
        private static List<List<string>> _keys = new List<List<string>>();
        private static readonly List<KeyValuePair<char, MR.Gestures.Button>> _controls = new List<KeyValuePair<char, MR.Gestures.Button>>();
        private static Editor _edit;
        private static readonly double _keyboardHeight = 220;
        private static ContentView _view;
        private static StackLayout _keyboardView;
        private static StackLayout _emojiView;
        private static List<string> _emojiList;
        private static int _lettersIndex = 0;
        private static bool _isAllCapital = false;
        private static double _width;
        public static void Initialize()
        {
            _controls.Clear();
            _keys = KeyboardHelper.GetKeys();
            _emojiList = KeyboardHelper.GetEmojiList();
            DisplayInfo mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            _width = mainDisplayInfo.Width / (mainDisplayInfo.Density * 12);

            _keyboardView = GetTextKeyboard();
            _emojiView = GetEmojiKeyboard();
        }
        public static void Initialize(ViewSecureKeyboard keyboard, Editor editor, CultureInfo language = null)
        {
            ViewKeyboard = keyboard;
            _edit = editor;

            //this disallow the system keyboard when editor is selected
#pragma warning disable CS1998 // In questo metodo asincrono non sono presenti operatori 'await', pertanto verrà eseguito in modo sincrono. Provare a usare l'operatore 'await' per attendere chiamate ad API non di blocco oppure 'await Task.Run(...)' per effettuare elaborazioni basate sulla CPU in un thread in background.
            editor.Focused += async (n, m) =>
#pragma warning restore CS1998 // In questo metodo asincrono non sono presenti operatori 'await', pertanto verrà eseguito in modo sincrono. Provare a usare l'operatore 'await' per attendere chiamate ad API non di blocco oppure 'await Task.Run(...)' per effettuare elaborazioni basate sulla CPU in un thread in background.
                editor.IsEnabled = false;
            editor.Unfocused += (n, m) =>
                editor.IsEnabled = true;

            //this show the keyboard when editor is selected
#pragma warning disable CS1998 // In questo metodo asincrono non sono presenti operatori 'await', pertanto verrà eseguito in modo sincrono. Provare a usare l'operatore 'await' per attendere chiamate ad API non di blocco oppure 'await Task.Run(...)' per effettuare elaborazioni basate sulla CPU in un thread in background.
            _edit.Focused += async (n, m) => _view.IsVisible = true;
#pragma warning restore CS1998 // In questo metodo asincrono non sono presenti operatori 'await', pertanto verrà eseguito in modo sincrono. Provare a usare l'operatore 'await' per attendere chiamate ad API non di blocco oppure 'await Task.Run(...)' per effettuare elaborazioni basate sulla CPU in un thread in background.

            //hidden the keyboard when is unfocused
#pragma warning disable CS1998 // In questo metodo asincrono non sono presenti operatori 'await', pertanto verrà eseguito in modo sincrono. Provare a usare l'operatore 'await' per attendere chiamate ad API non di blocco oppure 'await Task.Run(...)' per effettuare elaborazioni basate sulla CPU in un thread in background.
            _view.Unfocused += async (n, m) => _view.IsVisible = false; UpdateLetters(KeyboardType.FIRST_CAPITAL); _isAllCapital = false;
#pragma warning restore CS1998 // In questo metodo asincrono non sono presenti operatori 'await', pertanto verrà eseguito in modo sincrono. Provare a usare l'operatore 'await' per attendere chiamate ad API non di blocco oppure 'await Task.Run(...)' per effettuare elaborazioni basate sulla CPU in un thread in background.
            ViewKeyboard?.Invoke(_view);
        }

        public static StackLayout GetTextKeyboard()
        {
            _view = new ContentView { IsVisible = false, Padding = new Thickness(0, 10, 0, 10), BackgroundColor = Color.Black, HeightRequest = _keyboardHeight };
            _view.ForceLayout();
            var stack = new StackLayout { Orientation = StackOrientation.Vertical, HeightRequest = _keyboardHeight };
            stack.Spacing = 2;
            _view.Content = stack;
            for (var r = 0; r < _keys.Count; r++)
            {
                var row = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.CenterAndExpand };
                row.Padding = new Thickness(0);
                if (r <= 2)
                    row.HeightRequest = 54;
                else
                    row.HeightRequest = 44;
                row.Spacing = 4;
                stack.Children.Add(row);
                var n = 0;

                foreach (var c2 in _keys[r][0])
                {
                    row.Children.Add(GetButton(c2, _width, false));
                    n++;
                }
            }
            return stack;
        }

        public static void UpdateKeyboardButtons()
        {
            UpdateLetters(KeyboardType.FIRST_CAPITAL);
        }

        public static StackLayout GetEmojiKeyboard()
        {
            var rootScroolLayout = new StackLayout { Orientation = StackOrientation.Vertical, MinimumHeightRequest = 170, VerticalOptions = LayoutOptions.FillAndExpand};
            var scrollView = new ScrollView { Orientation = ScrollOrientation.Vertical, MinimumHeightRequest = 170 };
            var root = new StackLayout { Orientation = StackOrientation.Vertical, HeightRequest = _keyboardHeight };
            scrollView.ForceLayout();
            root.ForceLayout();
            var footer = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.FillAndExpand, HeightRequest = 50, BackgroundColor = Color.FromHex("#4F4F52") };
            StackLayout abcButton = GetButton('❹', _width + 10, true);
            StackLayout deleteButton = GetButton('⌫', _width + 10, true);
            abcButton.HorizontalOptions = LayoutOptions.StartAndExpand;
            abcButton.Margin = new Thickness(15, 0, 0, 0);
            deleteButton.HorizontalOptions = LayoutOptions.EndAndExpand;
            deleteButton.Margin = new Thickness(0, 0, 15, 0);
            footer.Children.Add(abcButton);
            footer.Children.Add(deleteButton);
            var body = new StackLayout { Orientation = StackOrientation.Vertical, MinimumHeightRequest = 170 };
            body.Spacing = 2;

            scrollView.Content = body;

            rootScroolLayout.Children.Add(scrollView);
            root.Children.Add(rootScroolLayout);
            root.Children.Add(footer);
            var r = 0;
            var row = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.FillAndExpand };
            row.Padding = new Thickness(0);
            row.Spacing = 4;
            body.Children.Add(row);
            foreach (var emoji in _emojiList)
            {
                if (r % 10 == 0 && r != 0)
                {
                    row = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.FillAndExpand };
                    row.Padding = new Thickness(0);
                    row.Spacing = 4;
                    body.Children.Add(row);
                }
                row.Children.Add(GetEmojiButton(emoji, _width));
                r++;

            }
            return root;
        }

        public static StackLayout GetEmojiButton(string emoji, double width)
        {
            var container = new StackLayout
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0)
            };

            var bt = new MR.Gestures.Button { Text = emoji };
            container.WidthRequest = width + 1;
            bt.WidthRequest = width + 1;
            bt.Padding = new Thickness(0);
            bt.Margin = new Thickness(0);
            bt.BackgroundColor = Color.Black;
            bt.Tapped += (n, m) =>
            {
                _edit.Text += emoji;
            };
            container.Children.Add(bt);
            return container;

        }
        public static StackLayout GetButton(char c2, double widthRequest, bool isEmojiButtons)
        {

            var container = new StackLayout();

            var bt = new MR.Gestures.Button
            {
                Text = GetText(c2),
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                CornerRadius = 4,

            };
            _controls.Add(new KeyValuePair<char, MR.Gestures.Button>(c2, bt));
            if (!isEmojiButtons)
            {
                if (c2 != '⇯' && c2 != '⇮' && c2 != '⌫' && c2 != '⏎' && c2 != '✓' && c2 != '.' && c2 != ',' && c2 != '?' && c2 != '☺' && c2 != '❶' && c2 != '❹')
                    bt.BackgroundColor = Color.FromHex("#262628");
                else
                    bt.BackgroundColor = Color.Black;
            }
            else
                bt.BackgroundColor = Color.FromHex("#4F4F52");

            if (c2 == ' ')
                bt.TextColor = Color.FromHex("#66666D");
            else
                bt.TextColor = Color.FromHex("#C4C2C2");
            if (c2 != ' ' && c2 != '⌫'  && c2 != '✓')
            {
                container.WidthRequest = widthRequest + 1;
                bt.WidthRequest = widthRequest + 1;
            }
            else if (c2 == '⌫')
            {
                container.WidthRequest = widthRequest + 8;
                bt.WidthRequest = widthRequest + 8;
            }
            container.Padding = new Thickness(0);
            container.Margin = new Thickness(0);
            bt.LongPressing += (s, e) => { if (c2 == '⌫') { _edit.Text = ""; _isAllCapital = false; UpdateLetters(KeyboardType.FIRST_CAPITAL); } };
            bt.Down += (n, m) =>
            {
                if (bt.Text == GetText('⇮'))
                {
                    UpdateLetters(KeyboardType.SMALL);
                    _isAllCapital = false;
                }
                else if (bt.Text == GetText('⇯'))
                {
                    UpdateLetters(KeyboardType.FIRST_CAPITAL);
                    _isAllCapital = false;
                    bt.Text = GetText('⇫');
                }
                else if (bt.Text == GetText('⇫'))
                {
                    UpdateLetters(KeyboardType.FIRST_CAPITAL);
                    _isAllCapital = true;
                }
                else if (bt.Text == GetText('❶'))
                {
                    UpdateLetters(KeyboardType.NUMBERS);
                }
                else if (bt.Text == GetText('❷'))
                {
                    if (_lettersIndex == 0)
                        UpdateLetters(KeyboardType.FIRST_CAPITAL);
                    else
                        UpdateLetters(KeyboardType.SMALL);
                }
                else if (bt.Text == GetText('❸'))
                {
                    UpdateLetters(KeyboardType.SYMBOLS);
                }
                else if (bt.Text == GetText('❹'))
                {
                    _view.Content = _keyboardView;

                }
                else if (c2 == '☺')
                {
                    _view.Content = _emojiView;
                }
                else if (bt.Text == GetText('⏎'))
                    _edit.Text += '\n';

                else if (bt.Text == GetText('✓'))
                    _view.IsVisible = false;

                else if (c2 == ' ')
                    _edit.Text += " ";
                else if (c2 == '⌫' && _edit.Text != null && _edit.Text.Length > 0)
                {
                    if (_edit.Text.Length > 1)
                    {
                        if (_emojiList.Contains(_edit.Text.Substring(_edit.Text.Length - 2)))
                            _edit.Text = _edit.Text.Substring(0, _edit.Text.Length - 2);
                        else
                            _edit.Text = _edit.Text.Substring(0, _edit.Text.Length - 1);
                    }
                    else
                        _edit.Text = _edit.Text.Substring(0, _edit.Text.Length - 1);

                    if (_edit.Text.Length == 0 && _lettersIndex == 1)
                        UpdateLetters(KeyboardType.FIRST_CAPITAL);
                }

                else if (c2 != '⌫')
                {
                    _edit.Text += bt.Text;
                    if (c2 == '.' || c2 == '?' || c2 == '!')
                        UpdateLetters(KeyboardType.FIRST_CAPITAL);
                    else if (_lettersIndex == 0 && !_isAllCapital)
                        UpdateLetters(KeyboardType.SMALL);
                }

            };

            container.Children.Add(bt);
            return container;
        }

        private static void UpdateLetters(KeyboardType type)
        {
            var index = (int)type;
            if (index < 2) _lettersIndex = index;
            var count = 0;
            for (var i = 0; i < _keys.Count; i++)
            {
                for (var j = 0; j < _keys[i][index].Length; j++)
                {
                    _controls[count].Value.Text = GetText(_keys[i][index][j]);
                    count++;
                }
            }
        }

        private static string GetText(char c)
        {
            if (c == ' ')
                return "   Space   ";
            if (c == '❶')
                return "123";
            if (c == '❷')
                return "abc";
            if (c == '❹')
                return " abc ";
            if (c == '❸')
                return "【]§  ";
            if (c == '⏎')
                return " ⏎";
            if (c == '✓')
                return "Done  ";
            if (c == '⇯')
                return "⇯  ";
            if (c == '⇮')
                return "⇮  ";
            if (c == '⇫')
                return "⇫  ";
            if (c == '⌫')
                return "  ⌫ ";
            return c.ToString();
        }
    }

    enum KeyboardType : int
    {
        FIRST_CAPITAL = 0,
        SMALL = 1,
        NUMBERS = 2,
        SYMBOLS = 3,
    }

}
