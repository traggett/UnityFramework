using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Framework
{
	namespace UI
	{
		public class MultiTouchScrollRect : ScrollRect
		{
			#region Serialized Data
			[SerializeField] private bool _deactivateContentOutOfView;
			#endregion

			#region Private Data
			private readonly DragInputHandler _dragInput = new DragInputHandler();
			#endregion

			#region Unity Messages
			protected override void LateUpdate()
			{
				base.LateUpdate();

				_dragInput.ValidateInput();

				if (_deactivateContentOutOfView)
				{
					Rect viewRect = viewport.rect;

					//Hide/enable children out of view
					foreach (RectTransform child in content.transform)
					{
						Rect childRect = child.rect;

						childRect.x += child.anchoredPosition.x;
						childRect.y += child.anchoredPosition.y;

						childRect.x += content.anchoredPosition.x;
						childRect.y += content.anchoredPosition.y;

						child.gameObject.SetActive(viewRect.Overlaps(childRect));
					}
				}
			}
			#endregion

			#region ScrollRect
			public override void OnBeginDrag(PointerEventData eventData)
			{
				_dragInput.OnBeginDrag((ExtendedPointerEventData)eventData);

				//If this is the only touch, start dragging now
				if (_dragInput.GetCurrentInputCount() == 1)
				{
					base.OnBeginDrag(eventData);
				}
				//If now have multiple touches, stop dragging
				else if (_dragInput.GetCurrentInputCount() > 1)
				{
					base.OnEndDrag(eventData);
					StopMovement();
				}
			}

			public override void OnDrag(PointerEventData eventData)
			{
				_dragInput.OnDrag((ExtendedPointerEventData)eventData);

				//Only drag when one touch is active
				if (_dragInput.GetCurrentInputCount() == 1)
				{
					base.OnDrag(eventData);
				}
			}

			public override void OnEndDrag(PointerEventData eventData)
			{
				_dragInput.OnEndDrag((ExtendedPointerEventData)eventData);

				//If no more touches, stop dragging
				if (_dragInput.GetCurrentInputCount() == 0)
				{
					base.OnEndDrag(eventData);
				}
				//If now have just one touch, start dragging with it
				else if (_dragInput.GetCurrentInputCount() == 1)
				{
					//Hacky, but override position of touch
					eventData.position = _dragInput.GetPrimaryDragPosition();
					base.OnBeginDrag(eventData);
				}
			}
			#endregion
		}
	}
}