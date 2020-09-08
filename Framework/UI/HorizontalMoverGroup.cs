using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	using Maths;

	namespace UI
	{
		[ExecuteInEditMode]
		public class HorizontalMoverGroup : MonoBehaviour
		{
			public TextAnchor _alignment;
			public float _spacing;
			public float _movementTime;
			public InterpolationType _movementInterpolationType;

			private Dictionary<RectTransform, RectTransformMover> _childMovers = new Dictionary<RectTransform, RectTransformMover>();
			private List<GameObject> _activeChildren = new List<GameObject>();

			private void OnEnable()
			{
				UpdateChildren(-1f);
			}

			private void LateUpdate()
			{
				if (Application.isPlaying)
					UpdateChildren(_movementTime);
				else
					UpdateChildrenInEditMode();
			}

			private void UpdateChildren(float movementTime)
			{
				float xPos = GetInitalX();

				List<GameObject> activeChildren = new List<GameObject>();

				foreach (RectTransform child in this.transform)
				{
					if (child != null && child.gameObject.activeSelf)
					{
						if (!_childMovers.TryGetValue(child, out RectTransformMover mover) || mover == null)
						{
							mover = child.GetComponent<RectTransformMover>();
							_childMovers.Add(child, mover);
						}

						activeChildren.Add(child.gameObject);

						if (mover != null)
						{
							bool wasActive = _activeChildren.Contains(child.gameObject);
							mover.SetX(xPos, wasActive ? movementTime : -1f, _movementInterpolationType);
						}
						else
						{
							RectTransformUtils.SetX(child, xPos);
						}

						xPos += RectTransformUtils.GetWidth(child) + _spacing;
					}
				}

				_activeChildren = activeChildren;
			}

			private void UpdateChildrenInEditMode()
			{
				float xPos = GetInitalX();

				foreach (RectTransform child in this.transform)
				{
					if (child.gameObject.activeSelf)
					{
						RectTransformUtils.SetX(child, xPos);
						xPos += RectTransformUtils.GetWidth(child) + _spacing;
					}
				}
			}

			private float GetInitalX()
			{
				switch (_alignment)
				{
					case TextAnchor.UpperCenter:
					case TextAnchor.MiddleCenter:
					case TextAnchor.LowerCenter:
						{
							float contentWidth = GetContentWidth();
							float width = ((RectTransform)this.transform).rect.width;
							return (width * 0.5f) - (contentWidth * 0.5f);
						}
					case TextAnchor.UpperRight:
					case TextAnchor.MiddleRight:
					case TextAnchor.LowerRight:
						{
							float contentWidth = GetContentWidth();
							float width = ((RectTransform)this.transform).rect.width;
							return width - contentWidth;
						}
					
					case TextAnchor.UpperLeft:
					case TextAnchor.MiddleLeft:
					case TextAnchor.LowerLeft:
					default:
						{
							return 0f;
						}
				}
			}

			public float GetContentWidth()
			{
				float width = 0f;

				foreach (RectTransform child in this.transform)
				{
					if (child != null && child.gameObject.activeSelf)
					{
						width += RectTransformUtils.GetWidth(child) + _spacing;
					}
				}

				return width - _spacing;
			}
		}
	}
}