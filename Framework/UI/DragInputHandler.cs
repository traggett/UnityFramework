using UnityEngine;
using UnityEngine.InputSystem.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Framework
{
	namespace UI
	{
		public class DragInputHandler
		{
			private const int kMaxInputs = 16;

			private struct InputData
			{
				public int _touchId;
				public int _pointerId;
				public Vector2 _position;
				public Camera _camera;
			}

			private int _currentInputCount;
			private InputData[] _activeDragInputs = new InputData[kMaxInputs];

			public void ValidateInput()
			{
				for (int i = 0; i < _currentInputCount; i++)
				{
					bool valid = true;

					//If touch, check event is still active?
					if (_activeDragInputs[i]._touchId != 0)
					{
						//Check touch is still active
						Touch touch = GetTouch(_activeDragInputs[i]._touchId);
						valid = touch.valid && touch.phase != TouchPhase.None && touch.phase != TouchPhase.Canceled;
					}

					//Check touch is still active
					if (!valid)
					{
						//Remove touch
						for (int j = i; j < _currentInputCount - 1; j++)
						{
							_activeDragInputs[j] = _activeDragInputs[j + 1];
						}

						_currentInputCount--;
					}
				}
			}

			public void OnBeginDrag(ExtendedPointerEventData eventData)
			{
				//Touch event
				if (eventData.touchId != 0)
				{
					for (int i = 0; i < _currentInputCount; i++)
					{
						if (_activeDragInputs[i]._touchId == eventData.touchId)
						{
							_activeDragInputs[i]._position = eventData.position;
							return;
						}
					}
				}

				//Pointer event
				{
					for (int i = 0; i < _currentInputCount; i++)
					{
						if (_activeDragInputs[i]._pointerId == eventData.pointerId)
						{
							_activeDragInputs[i]._position = eventData.position;
							return;
						}
					}
				}

				//New input
				if (_currentInputCount < _activeDragInputs.Length)
				{
					_activeDragInputs[_currentInputCount]._touchId = eventData.touchId;
					_activeDragInputs[_currentInputCount]._pointerId = eventData.pointerId;
					_activeDragInputs[_currentInputCount]._position = eventData.position;
					_activeDragInputs[_currentInputCount]._camera = eventData.pressEventCamera;
					_currentInputCount++;
				}
			}

			public void OnDrag(ExtendedPointerEventData eventData)
			{
				//Touch event
				if (eventData.touchId != 0)
				{
					for (int i = 0; i < _currentInputCount; i++)
					{
						if (_activeDragInputs[i]._touchId == eventData.touchId)
						{
							_activeDragInputs[i]._position = eventData.position;
							return;
						}
					}
				}

				//Pointer event
				{
					for (int i = 0; i < _currentInputCount; i++)
					{
						if (_activeDragInputs[i]._pointerId == eventData.pointerId)
						{
							_activeDragInputs[i]._position = eventData.position;
							return;
						}
					}
				}
			}

			public void OnEndDrag(ExtendedPointerEventData eventData)
			{
				bool foundInput = false;

				for (int i = 0; i < _currentInputCount; i++)
				{
					if ((eventData.touchId != 0 && _activeDragInputs[i]._touchId == eventData.touchId) || (eventData.touchId == 0 && _activeDragInputs[i]._pointerId == eventData.pointerId))
						foundInput = true;

					if (foundInput && i < _currentInputCount - 1)
						_activeDragInputs[i] = _activeDragInputs[i + 1];
				}

				if (foundInput)
					_currentInputCount--;
			}

			public int GetCurrentInputCount()
			{
				return _currentInputCount;
			}

			public Vector2 GetPrimaryDragPosition()
			{
				if (_currentInputCount > 0)
				{
					return _activeDragInputs[0]._position;
				}

				return Vector2.zero;
			}

			public Vector2 GetSecondaryDragPosition()
			{
				if (_currentInputCount > 1)
				{
					return _activeDragInputs[1]._position;
				}

				return Vector2.zero;
			}

			public Camera GetPrimaryDragCamera()
			{
				if (_currentInputCount > 0)
				{
					return _activeDragInputs[0]._camera;
				}

				return null;
			}

			private Touch GetTouch(int touchId)
			{
				for (int i = 0; i < Touch.activeTouches.Count; i++)
				{
					if (Touch.activeTouches[i].touchId == touchId)
					{
						return Touch.activeTouches[i];
					}
				}

				return default;
			}
		}
	}
}