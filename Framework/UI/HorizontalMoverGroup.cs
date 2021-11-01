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
			public bool _includeDisabledGameObjects;

			private Dictionary<RectTransform, RectTransformMover> _childMovers = new Dictionary<RectTransform, RectTransformMover>();
			private List<GameObject> _activeChildren = new List<GameObject>();

			private void OnEnable()
			{
				UpdateChildren(-1f);
			}

			private void LateUpdate()
			{
				UpdateChildren();
			}

			public void UpdateChildren()
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
					if (child != null && (_includeDisabledGameObjects || child.gameObject.activeSelf))
					{
						if (!_childMovers.TryGetValue(child, out RectTransformMover mover) || mover == null)
						{
							mover = child.GetComponent<RectTransformMover>();
							_childMovers[child] = mover;
						}

						activeChildren.Add(child.gameObject);

						float x = xPos + child.pivot.x * RectTransformUtils.GetWidth(child);

						if (mover != null)
						{
							bool wasActive = _activeChildren.Contains(child.gameObject);
							mover.SetX(x, wasActive ? movementTime : -1f, _movementInterpolationType);
						}
						else
						{
							RectTransformUtils.SetX(child, x);
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
					if (_includeDisabledGameObjects || child.gameObject.activeSelf)
					{
						float x = xPos + child.pivot.x * RectTransformUtils.GetWidth(child);

						RectTransformUtils.SetX(child, x);
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
							return (contentWidth) * -0.5f;
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

				bool first = true;

				foreach (RectTransform child in this.transform)
				{
					if (child != null && child.gameObject.activeSelf)
					{
						if (first)
							first = false;
						else
							width += _spacing;

						width += RectTransformUtils.GetWidth(child);
					}
				}

				return width;
			}
		}
	}
}