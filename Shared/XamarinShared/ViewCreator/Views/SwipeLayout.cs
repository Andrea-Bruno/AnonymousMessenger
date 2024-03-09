using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XamarinShared.ViewCreator.Views
{
    public class SwipeLayout :Frame
    {

        private PanGestureRecognizer _panGesture = new PanGestureRecognizer();
        public SwipeLayout()
        {
            _panGesture.PanUpdated += OnPanGestureUpdated;
            GestureRecognizers.Add(_panGesture);

        }

        public event EventHandler SlideCompleted;

        private double? _minimumX;
        private const uint _animLength = 100;
        async void OnPanGestureUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (_minimumX == null)
                _minimumX = -Width * 0.25;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    break;
                case GestureStatus.Running:
                    //var x = Math.Max(0, e.TotalX);  //min limit


                    var x = e.TotalX;
                    if (x < _minimumX)  //max limit
                        x = _minimumX.Value;

                    if (x > 0) x = 0;

                    //if (e.TotalX < Content.TranslationX) //disable backdraw
                    //    return;


                    Content.TranslationX = x;
                    break;

                case GestureStatus.Completed:
                    var posX = Content.TranslationX;

                    // Reset translation applied during the pan (snap effect)
                    await Content.TranslateTo(0, 0, _animLength, Easing.CubicIn);

                    if (posX >= (_minimumX - 5/* keep some margin for error*/))
                        SlideCompleted?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }
    }
}
