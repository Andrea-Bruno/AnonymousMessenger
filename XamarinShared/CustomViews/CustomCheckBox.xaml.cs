using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinShared.CustomViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomCheckBox : ContentView
    {
        public event EventHandler<CheckedChangedEventArgs> CheckedChanged;
        public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(CustomCheckBox), default(bool), BindingMode.TwoWay);
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set
            {
                SetValue(IsCheckedProperty, value);
                CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(value));
            }
        }

        public event EventHandler<EventArgs> ColorChanged;
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(CustomCheckBox), default(Color), BindingMode.TwoWay);
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set
            {
                SetValue(ColorProperty, value);
                ColorChanged?.Invoke(this, new EventArgs());
            }
        }


        public CustomCheckBox()
        {
            InitializeComponent();
        }

        private void OnCheckBoxTapped(object sender, EventArgs e)
        {
            IsChecked ^= true;
        }

        private void UpdateChecked()
        {
            if (IsChecked)
            {
                BgRectangle.Fill = BgRectangle.Stroke = new SolidColorBrush(Color);
            }
            else
            {
                BgRectangle.Fill = new SolidColorBrush(Color.Transparent);
                BgRectangle.Stroke = new SolidColorBrush(Color);
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);


            switch (propertyName)
            {
                case nameof(IsChecked):
                case nameof(Color):
                    UpdateChecked();
                    break;
            }
        }
    }
}