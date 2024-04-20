using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Banking
{
	public class CustomLinkLabel : CustomLabel
	{
		public static BindableProperty LinksTextProperty = BindableProperty.Create(nameof(LinksText), typeof(FormattedString), typeof(CustomLinkLabel), propertyChanged: OnLinksTextPropertyChanged);

		private readonly ICommand _linkTapGesture = new Command<string>((url) => Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred));

        public FormattedString LinksText
		{
			get => GetValue(LinksTextProperty) as FormattedString;
			set => SetValue(LinksTextProperty, value);
		}

		private void SetFormattedText()
		{
			var formattedString = new FormattedString();
			IList<Span> spans = LinksText.Spans;
			foreach (Span s in spans)
			{
				if (s.TextColor != Color.FromHex("#5F9EFB"))
				{
					var splitText = s.Text.Split(' ');

					foreach (var textPart in splitText)
					{
						var span = new Span { Text = $"{textPart} " };

						if (IsUrl(textPart)) // a link
						{
							span.TextColor = Color.DeepSkyBlue;
							span.GestureRecognizers.Add(new TapGestureRecognizer
							{
								Command = _linkTapGesture,
								CommandParameter = textPart
							});
						}
						formattedString.Spans.Add(span);
					}
				}
				else formattedString.Spans.Add(s);
			}
			FormattedText = formattedString;
		}

		private bool IsUrl(string input)
		{
			return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) &&
				(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
		}
		private static void OnLinksTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var linksLabel = bindable as CustomLinkLabel;
			linksLabel.SetFormattedText();
		}
	}
}
