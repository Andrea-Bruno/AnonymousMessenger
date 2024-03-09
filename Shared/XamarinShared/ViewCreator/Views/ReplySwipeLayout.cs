using System;
using Xamarin.Forms;
namespace XamarinShared.ViewCreator.Views
{
    public class ReplySwipeLayout : SwipeView
    {
        public Action OnReplySwiped;
        private double _lastOffset;
        private bool _IsSwipeEnabled =true;

        public bool IsSwipeEnabled
        {
            get => _IsSwipeEnabled;
            set
            {
                _IsSwipeEnabled = value;
                if (!value)
                {
                    IsEnabled = false;
                }
               
            }
        }

        protected ReplySwipeLayout()
        {
            BackgroundColor = Color.Transparent;

            LeftItems.Add(new SwipeItem()
            {
                BackgroundColor = Color.Transparent
            });

            SwipeEnded += (s, e) =>
            {
                if (_lastOffset > 85 && IsSwipeEnabled)
                    OnReplySwiped?.Invoke();
                Close();

            };
            SwipeChanging += (s, e) => {
                _lastOffset = e.Offset;
            };

        }
    }
}
