using Xamarin.Forms;

namespace Banking
{
	public class CustomEditor : Editor
	{
#pragma warning disable CS0108 // 'CustomEditor.PlaceholderProperty' nasconde il membro ereditato 'Editor.PlaceholderProperty'. Se questo comportamento è intenzionale, usare la parola chiave new.
        public static BindableProperty PlaceholderProperty
#pragma warning restore CS0108 // 'CustomEditor.PlaceholderProperty' nasconde il membro ereditato 'Editor.PlaceholderProperty'. Se questo comportamento è intenzionale, usare la parola chiave new.
          = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(CustomEditor));

#pragma warning disable CS0108 // 'CustomEditor.PlaceholderColorProperty' nasconde il membro ereditato 'Editor.PlaceholderColorProperty'. Se questo comportamento è intenzionale, usare la parola chiave new.
        public static BindableProperty PlaceholderColorProperty
#pragma warning restore CS0108 // 'CustomEditor.PlaceholderColorProperty' nasconde il membro ereditato 'Editor.PlaceholderColorProperty'. Se questo comportamento è intenzionale, usare la parola chiave new.
           = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(CustomEditor), Color.LightGray);

        public static BindableProperty HasRoundedCornerProperty
        = BindableProperty.Create(nameof(HasRoundedCorner), typeof(bool), typeof(CustomEditor), false);

        public static BindableProperty IsExpandableProperty
        = BindableProperty.Create(nameof(IsExpandable), typeof(bool), typeof(CustomEditor), false);

        public bool IsExpandable
        {
            get { return (bool)GetValue(IsExpandableProperty); }
            set { SetValue(IsExpandableProperty, value); }
        }
        public bool HasRoundedCorner
        {
            get { return (bool)GetValue(HasRoundedCornerProperty); }
            set { SetValue(HasRoundedCornerProperty, value); }
        }

#pragma warning disable CS0108 // 'CustomEditor.Placeholder' nasconde il membro ereditato 'InputView.Placeholder'. Se questo comportamento è intenzionale, usare la parola chiave new.
        public string Placeholder
#pragma warning restore CS0108 // 'CustomEditor.Placeholder' nasconde il membro ereditato 'InputView.Placeholder'. Se questo comportamento è intenzionale, usare la parola chiave new.
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

#pragma warning disable CS0108 // 'CustomEditor.PlaceholderColor' nasconde il membro ereditato 'InputView.PlaceholderColor'. Se questo comportamento è intenzionale, usare la parola chiave new.
        public Color PlaceholderColor
#pragma warning restore CS0108 // 'CustomEditor.PlaceholderColor' nasconde il membro ereditato 'InputView.PlaceholderColor'. Se questo comportamento è intenzionale, usare la parola chiave new.
        {
            get { return (Color)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }

        public CustomEditor()
        {
            TextChanged += OnTextChanged;
        }

        ~CustomEditor()
        {
            TextChanged -= OnTextChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsExpandable) InvalidateMeasure();
        }
    }
}