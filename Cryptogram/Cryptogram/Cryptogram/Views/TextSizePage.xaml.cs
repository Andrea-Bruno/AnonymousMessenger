using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Utils;
using CustomViewElements;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.ApplicationModel.Communication;
using Cryptogram.DesignHandler;
using FontSizeConverter = XamarinShared.ViewCreator.FontSizeConverter;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Cryptogram.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TextSizePage : BasePage
    {
        private FontSizeConverter _fontSizeConverter;
        private int _textSize=1;
                                                
        public float TextSize
        {
            get { 
                return FontSizeConverter.FontRatios[_textSize];
            }
            set {
                _textSize = (int)value;
                OnPropertyChanged(nameof(TextSize));
            } 
        } 

        public TextSizePage()
        {
            InitializeComponent();
            _textSize = FindFontRationIndex(); 
            _fontSizeConverter = new FontSizeConverter();
            slider.Value = _textSize;
            DemoLayout.Children.Add(OnViewMessage(false, Localization.Resources.Dictionary.HelloWellcomeToCryptogram)); 
            DemoLayout.Children.Add(OnViewMessage(true, Localization.Resources.Dictionary.HeyHowAreYouDoing)); 
            DemoLayout.Children.Add(OnViewMessage(true, Localization.Resources.Dictionary.IAmDoingGoodHowAboutYou)); 
            DemoLayout.Children.Add(OnViewMessage(false, Localization.Resources.Dictionary.YeahEverythingIsFine)); 
            DemoLayout.Children.Add(OnViewMessage(true, Localization.Resources.Dictionary.IDoNotHaveAnyPlanIfYouHaveAny)); 
            DemoLayout.Children.Add(OnViewMessage(true, Localization.Resources.Dictionary.TheWeatherIsFine));
            DemoLayout.Children.Add(OnViewMessage(false, Localization.Resources.Dictionary.YesMarkGoodIdea)); 
            var ballSize = 16;
            Steps.Margin = new Thickness(ballSize, 0, ballSize, 0);
            BindingContext = this;
        }

        private View OnViewMessage(bool isMyMessage, string _text)
        {
            var paddingRight = 5;
            Color background;
            int paddingLeft;
            if (isMyMessage)
            {
                paddingLeft = 10;
                background = DesignResourceManager.GetColorFromStyle("WhiteColor");
            }
            else
            {
                paddingLeft = 10;
                paddingRight = 5;
                background = DesignResourceManager.GetColorFromStyle("Color2");
            }
            var frame = new Border()
            {
                HorizontalOptions = isMyMessage ? LayoutOptions.EndAndExpand : LayoutOptions.StartAndExpand,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = isMyMessage ? new CornerRadius(15, 15, 15, 0) : new CornerRadius(15, 15, 0, 15) },
                Background = new SolidColorBrush(background),
                Padding = 3,
                MinimumHeightRequest = 200,

            };
            var layout = new StackLayout() 

            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                Padding = isMyMessage ? new Thickness(35, 0, 0, 0): new Thickness(0, 0, 35, 0)
            };

            layout.Children.Add(frame);
            var box = new StackLayout() { Padding = new Thickness(paddingLeft, 0, paddingRight, 0) };                    
            frame.Content = box;

            var label = new CustomLabel
            {
                FontFamily = "PoppinsSemiBold",
                TextType = TextType.Text,
                TextColor = isMyMessage ? Colors.Black : Colors.White,
                Padding = new Thickness(0, 5, isMyMessage ? 0 : 10, 0),

            };
            label.SetBinding(Label.FontSizeProperty, new Binding("TextSize", converter: _fontSizeConverter, converterParameter: 14));
            var timeLabel = new Label() { FontFamily = "PoppinsRegular", TextColor = isMyMessage ? Colors.Black : Colors.White };
            timeLabel.Text = "10:10";
            timeLabel.SetBinding(Label.FontSizeProperty, new Binding("TextSize", converter: _fontSizeConverter, converterParameter: 12));
            var flagAndTime = new StackLayout() { Orientation = StackOrientation.Horizontal, HorizontalOptions =LayoutOptions.EndAndExpand};
            
            flagAndTime.Children.Add(timeLabel);

            if (isMyMessage)
            {
                Label StatusLabel = new Label() { Padding = 0 };
                StatusLabel.Text = "✓✓"; StatusLabel.TextColor = Colors.Green;
                StatusLabel.SetBinding(Label.FontSizeProperty, new Binding("TextSize", converter: _fontSizeConverter, converterParameter: 12));
                flagAndTime.Children.Add(StatusLabel);
            }
          
            label.Text = _text;
            label.HorizontalTextAlignment = TextAlignment.Start;
            box.Children.Add(label);
            box.Children.Add(flagAndTime);
            return layout;
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            double data= Math.Round(e.NewValue);
            slider.Value = data;

            if (data != _textSize)
            {
                TextSize = (float)data;
            }
        }

        void SaveButton_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            FontSizeConverter.DefaultSelectedFontRatio = FontSizeConverter.FontRatios[_textSize];
            Preferences.Set("FontRatio", FontSizeConverter.DefaultSelectedFontRatio);
            XamarinShared.ViewCreator.MessageViewCreator.Instance.TextSize = FontSizeConverter.DefaultSelectedFontRatio;
            OnBackButtonPressed();
        }
     
        private int FindFontRationIndex()
        {
            for (int i = 0; i < FontSizeConverter.FontRatios.Length; i++)
                if (FontSizeConverter.DefaultSelectedFontRatio == FontSizeConverter.FontRatios[i])
                    return i;
            return 1;
        }

        private void Back_Clicked(object sender, EventArgs args)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }
    }

}