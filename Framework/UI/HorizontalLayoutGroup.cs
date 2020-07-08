using Framework.Maths;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	namespace UI
	{
		[ExecuteInEditMode]
		public class HorizontalLayoutGroup : MonoBehaviour
		{
			public float _spacing;
			public float _movementTime;
			public InterpolationType _movementInterpolationType;

			private Dictionary<RectTransform, RectTransformMover> _childMovers = new Dictionary<RectTransform, RectTransformMover>();

			private void OnEnable()
			{
				UpdateChildren(-1f);
			}

			private void Update()
			{
				if (Application.isPlaying)
					UpdateChildren(_movementTime);
				else
					UpdateChildrenInEditMode();
			}

			private void UpdateChildren(float movementTime)
			{
				float xPos = 0f;

				foreach (RectTransform child in this.transform)
				{
					if (child.gameObject.activeSelf)
					{
						if (!_childMovers.TryGetValue(child, out RectTransformMover mover) || mover == null)
						{
							mover = child.GetComponent<RectTransformMover>();
							_childMovers.Add(child, mover);
						}

						if (mover != null)
						{
							mover.SetX(xPos, _movementTime, _movementInterpolationType);
						}

						xPos += RectTransformUtils.GetWidth(child) + _spacing;
					}
				}

				//Also set this width to be content size
				RectTransformUtils.SetWidth((RectTransform)this.transform, xPos - _spacing);
			}

			private void UpdateChildrenInEditMode()
			{
				float xPos = 0f;

				foreach (RectTransform child in this.transform)
				{
					if (child.gameObject.activeSelf)
					{
						RectTransformUtils.SetX(child, xPos);
						xPos += RectTransformUtils.GetWidth(child) + _spacing;
					}
				}

				//Also set this width to be content size
				RectTransformUtils.SetWidth((RectTransform)this.transform, xPos - _spacing);
			}
		}
	}
}