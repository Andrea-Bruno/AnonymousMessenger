using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace XamarinShared.ViewCreator
{
    public class CircularProgressBar : GraphicsView
    {
        public CircularProgressBar()
        {
            Drawable = new CircleDrawable(this);
        }

        public static readonly BindableProperty ProgressProperty =
            BindableProperty.Create(nameof(Progress), typeof(double), typeof(CircularProgressBar), 0.0,
                propertyChanged: (b, o, n) => ((CircularProgressBar)b).Invalidate());

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create(nameof(Color), typeof(Color), typeof(CircularProgressBar), Colors.DodgerBlue,
                propertyChanged: (b, o, n) => ((CircularProgressBar)b).Invalidate());

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public bool Easing { get; set; }

        private sealed class CircleDrawable : IDrawable
        {
            private readonly CircularProgressBar _owner;

            public CircleDrawable(CircularProgressBar owner)
            {
                _owner = owner;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                float size = Math.Min(dirtyRect.Width, dirtyRect.Height);
                if (size <= 0) return;

                float stroke = Math.Max(3f, size * 0.12f);
                float inset = stroke / 2f;
                var rect = new RectF(
                    dirtyRect.X + (dirtyRect.Width - size) / 2f + inset,
                    dirtyRect.Y + (dirtyRect.Height - size) / 2f + inset,
                    size - stroke,
                    size - stroke);

                canvas.StrokeColor = Colors.Gray.WithAlpha(0.3f);
                canvas.StrokeSize = stroke;
                canvas.DrawEllipse(rect);

                double pct = Math.Max(0, Math.Min(100, _owner.Progress)) / 100.0;
                if (pct > 0)
                {
                    canvas.StrokeColor = _owner.Color;
                    canvas.StrokeSize = stroke;
                    canvas.StrokeLineCap = LineCap.Round;
                    float start = -90f;
                    float sweep = (float)(pct * 360.0);
                    canvas.DrawArc(rect.Left, rect.Top, rect.Right, rect.Bottom, start, sweep, false, false);
                }
            }
        }
    }
}
