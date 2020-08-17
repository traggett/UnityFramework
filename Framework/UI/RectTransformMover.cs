using UnityEngine;

namespace Framework
{
	using Maths;

	namespace UI
	{
		public class RectTransformMover : MonoBehaviour
		{
			#region Public Data
			public float _targetTolerance = 1f;
			#endregion

			#region Private Data
			private struct Target
			{
				public Vector2 _from;
				public Vector2 _target;
				public float _lerp;
				public float _lerpSpeed;
				public InterpolationType _interpolationType;

				public void SetTarget(Vector2 target, float moveTime, Vector2 current, float tolerance, InterpolationType interpolationType)
				{
					if (!IsLerping() || _interpolationType != interpolationType || !MathUtils.Approximately(target, _target, tolerance))
					{
						_lerp = 1f;
						_from = current;
						_interpolationType = interpolationType;
					}

					_lerpSpeed = 1f / moveTime;
					_target = target;
				}

				public void Clear()
				{
					_lerp = 0f;
				}

				public bool IsLerping()
				{
					return _lerp > 0f;
				}

				public Vector2 Update(float deltaTime)
				{
					_lerp -= _lerpSpeed * deltaTime;

					if (_lerp < 0f)
						return _target;

					return MathUtils.Interpolate(_interpolationType, _from, _target, 1f - _lerp);
				}
			}

			private Target _anchorTarget;
			private Target _offsetMinTarget;
			private Target _offsetMaxTarget;
			private RectTransform _rectTransform;
			#endregion

			#region Public Interface
			public void SetAnchoredPosition(Vector2 anchoredPosition, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				Vector2 currentAnchoredPosition = GetRectTransform().anchoredPosition;

				if (time > 0f)
				{
					_anchorTarget.SetTarget(anchoredPosition, time, currentAnchoredPosition, _targetTolerance, interpolationType);
				}
				else
				{
					_anchorTarget.Clear();
					GetRectTransform().anchoredPosition = anchoredPosition;
				}
			}

			public void SetOffsetMin(Vector2 offsetMin, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				Vector2 currentOffsetMin = GetRectTransform().offsetMin;

				if (time > 0f)
				{
					_offsetMinTarget.SetTarget(offsetMin, time, currentOffsetMin, _targetTolerance, interpolationType);
				}
				else
				{
					_offsetMinTarget.Clear();
					GetRectTransform().offsetMin = offsetMin;
				}
			}

			public void SetOffsetMax(Vector2 offsetMax, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				Vector2 currentOffsetMax = GetRectTransform().offsetMax;

				if (time > 0f)
				{
					_offsetMaxTarget.SetTarget(offsetMax, time, currentOffsetMax, _targetTolerance, interpolationType);
				}
				else
				{
					_offsetMaxTarget.Clear();
					GetRectTransform().offsetMax = offsetMax;
				}
			}

			public void SetX(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetAnchoredPosition(new Vector2(target, GetRectTransform().anchoredPosition.y), time, interpolationType);
			}

			public void SetY(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetAnchoredPosition(new Vector2(GetRectTransform().anchoredPosition.x, target), time, interpolationType);
			}

			public void SetBottom(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetOffsetMin(new Vector2(GetRectTransform().offsetMin.x, target), time, interpolationType);
			}

			public void SetTop(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetOffsetMax(new Vector2(GetRectTransform().offsetMax.x, -target), time, interpolationType);
			}

			public void SetLeft(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetOffsetMin(new Vector2(target, GetRectTransform().offsetMin.y), time, interpolationType);
			}

			public void SetRight(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetOffsetMax(new Vector2(-target, GetRectTransform().offsetMax.y), time, interpolationType);
			}

			public RectTransform GetRectTransform()
			{
				if (_rectTransform == null)
					_rectTransform = (RectTransform)this.transform;

				return _rectTransform;
			}
			#endregion

			#region MonoBehaviour
			private void Update()
			{
				float deltaTime = Time.deltaTime;

				RectTransform rectTransform = GetRectTransform();

				if (_anchorTarget.IsLerping())
				{
					rectTransform.anchoredPosition = _anchorTarget.Update(deltaTime);
				}

				if (_offsetMinTarget.IsLerping())
				{
					rectTransform.offsetMin = _offsetMinTarget.Update(deltaTime);
				}

				if (_offsetMaxTarget.IsLerping())
				{
					rectTransform.offsetMax = _offsetMaxTarget.Update(deltaTime);
				}
			}
			#endregion
		}
	}
}