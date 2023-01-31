using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XamarinShared.ViewCreator.Views
{
    public class SwipeLayout :Frame
    {
        public Action OnReplySwiped;
        private bool _IsSwipeEnabled = true;

        public bool IsSwipeEnabled
        {
            get => _IsSwipeEnabled;
            set
            {
                _IsSwipeEnabled = value;
                if (!value)
                {
                    //IsEnabled = false;
                }
            }
        }

        private PanGestureRecognizer _panGesture = new PanGestureRecognizer();
        public SwipeLayout()
        {
            _panGesture.PanUpdated += OnPanGestureUpdated;
            GestureRecognizers.Add(_panGesture);

        }


        private double? _minimumX;
        private const uint _animLength = 100;
        async void OnPanGestureUpdated(object sender, PanUpdatedEventArgs e)
        {
            //if (!_IsSwipeEnabled) return;
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
                        OnReplySwiped?.Invoke();
                    break;
            }
        }
    }
}
