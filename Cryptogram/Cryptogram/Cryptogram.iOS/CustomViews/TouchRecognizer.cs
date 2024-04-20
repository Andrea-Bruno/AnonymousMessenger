using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using TouchTracking;
using UIKit;
using Xamarin.Forms;

namespace Anonymous.iOS
{
	internal class TouchRecognizer : UIGestureRecognizer
	{
		private readonly Element _element;        // Forms element for firing events
		private readonly UIView _view;            // iOS UIView 
		private readonly TouchTracking.TouchEffect _touchEffect;
		private bool _capture;
		private static readonly Dictionary<UIView, TouchRecognizer> _viewDictionary =
				new Dictionary<UIView, TouchRecognizer>();
		private static readonly Dictionary<long, TouchRecognizer> _idToTouchDictionary =
				new Dictionary<long, TouchRecognizer>();

		public TouchRecognizer(Element element, UIView view, TouchTracking.TouchEffect touchEffect)
		{
			_element = element;
			_view = view;
			_touchEffect = touchEffect;
			_viewDictionary.Add(view, this);
		}

		public void Detach() => _viewDictionary.Remove(_view);

		// touches = touches of interest; evt = all touches of type UITouch
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				var id = touch.Handle.ToInt64();
				FireEvent(this, id, TouchActionType.Pressed, touch, true);

				if (!_idToTouchDictionary.ContainsKey(id))
				{
					_idToTouchDictionary.Add(id, this);
				}
			}
			_capture = _touchEffect.Capture;
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			base.TouchesMoved(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				var id = touch.Handle.ToInt64();

				if (_capture)
				{
					FireEvent(this, id, TouchActionType.Moved, touch, true);
				}
				else
				{
					CheckForBoundaryHop(touch);

					if (_idToTouchDictionary[id] != null)
					{
						FireEvent(_idToTouchDictionary[id], id, TouchActionType.Moved, touch, true);
					}
				}
			}
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				var id = touch.Handle.ToInt64();

				if (_capture)
				{
					FireEvent(this, id, TouchActionType.Released, touch, false);
				}
				else
				{
					CheckForBoundaryHop(touch);

					if (_idToTouchDictionary[id] != null)
					{
						FireEvent(_idToTouchDictionary[id], id, TouchActionType.Released, touch, false);
					}
				}
				_idToTouchDictionary.Remove(id);
			}
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				var id = touch.Handle.ToInt64();

				if (_capture)
				{
					FireEvent(this, id, TouchActionType.Cancelled, touch, false);
				}
				else if (_idToTouchDictionary[id] != null)
				{
					FireEvent(_idToTouchDictionary[id], id, TouchActionType.Cancelled, touch, false);
				}
				_idToTouchDictionary.Remove(id);
			}
		}

		private void CheckForBoundaryHop(UITouch touch)
		{
			var id = touch.Handle.ToInt64();
			TouchRecognizer recognizerHit = null;

			foreach (UIView view in _viewDictionary.Keys)
			{
				CGPoint location = touch.LocationInView(view);

				if (new CGRect(new CGPoint(), view.Frame.Size).Contains(location))
				{
					recognizerHit = _viewDictionary[view];
				}
			}
			if (recognizerHit != _idToTouchDictionary[id])
			{
				if (_idToTouchDictionary[id] != null)
				{
					FireEvent(_idToTouchDictionary[id], id, TouchActionType.Exited, touch, true);
				}
				if (recognizerHit != null)
				{
					FireEvent(recognizerHit, id, TouchActionType.Entered, touch, true);
				}
				_idToTouchDictionary[id] = recognizerHit;
			}
		}

		private void FireEvent(TouchRecognizer recognizer, long id, TouchActionType actionType, UITouch touch, bool isInContact)
		{
			CGPoint cgPoint = touch.LocationInView(recognizer.View);
			var xfPoint = new Point(cgPoint.X, cgPoint.Y);
			// Get the method to call for firing events
			Action<Element, TouchActionEventArgs> onTouchAction = recognizer._touchEffect.OnTouchAction;

			// Call that method
			onTouchAction(recognizer._element,
					new TouchActionEventArgs(id, actionType, xfPoint, isInContact));
		}
	}
}