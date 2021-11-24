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

			public RectTransform RectTransform
			{
				get
				{
					if (_rectTransform == null)
						_rectTransform = (RectTransform)this.transform;

					return _rectTransform;
				}
			}
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
			private Target _sizeDeltaTarget;
			private RectTransform _rectTransform;
			#endregion

			#region Public Interface
			public void SetAnchoredPosition(Vector2 anchoredPosition, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				Vector2 currentAnchoredPosition = RectTransform.anchoredPosition;

				if (time > 0f)
				{
					_anchorTarget.SetTarget(anchoredPosition, time, currentAnchoredPosition, _targetTolerance, interpolationType);
				}
				else
				{
					_anchorTarget.Clear();
					RectTransform.anchoredPosition = anchoredPosition;
				}
			}

			public void SetOffsetMin(Vector2 offsetMin, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				Vector2 currentOffsetMin = RectTransform.offsetMin;

				if (time > 0f)
				{
					_offsetMinTarget.SetTarget(offsetMin, time, currentOffsetMin, _targetTolerance, interpolationType);
				}
				else
				{
					_offsetMinTarget.Clear();
					RectTransform.offsetMin = offsetMin;
				}
			}

			public void SetOffsetMax(Vector2 offsetMax, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				Vector2 currentOffsetMax = RectTransform.offsetMax;

				if (time > 0f)
				{
					_offsetMaxTarget.SetTarget(offsetMax, time, currentOffsetMax, _targetTolerance, interpolationType);
				}
				else
				{
					_offsetMaxTarget.Clear();
					RectTransform.offsetMax = offsetMax;
				}
			}

			public void SetSizeDelta(Vector2 sizeDelta, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				Vector2 currentSizeDelta = RectTransform.sizeDelta;

				if (time > 0f)
				{
					_sizeDeltaTarget.SetTarget(sizeDelta, time, currentSizeDelta, _targetTolerance, interpolationType);
				}
				else
				{
					_sizeDeltaTarget.Clear();
					RectTransform.sizeDelta = sizeDelta;
				}
			}

			public void SetX(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetAnchoredPosition(new Vector2(target, RectTransform.anchoredPosition.y), time, interpolationType);
			}

			public void SetY(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetAnchoredPosition(new Vector2(RectTransform.anchoredPosition.x, target), time, interpolationType);
			}

			public void SetBottom(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetOffsetMin(new Vector2(RectTransform.offsetMin.x, target), time, interpolationType);
			}

			public void SetTop(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetOffsetMax(new Vector2(RectTransform.offsetMax.x, -target), time, interpolationType);
			}

			public void SetLeft(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetOffsetMin(new Vector2(target, RectTransform.offsetMin.y), time, interpolationType);
			}

			public void SetRight(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetOffsetMax(new Vector2(-target, RectTransform.offsetMax.y), time, interpolationType);
			}

			public void SetWidth(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetSizeDelta(new Vector2(target, RectTransform.sizeDelta.y), time, interpolationType);
			}

			public void SetHeight(float target, float time = -1f, InterpolationType interpolationType = InterpolationType.InOutCubic)
			{
				SetSizeDelta(new Vector2(RectTransform.sizeDelta.x, target), time, interpolationType);
			}
			#endregion

			#region Unity Messages
			private void Update()
			{
				float deltaTime = Time.deltaTime;

				RectTransform rectTransform = RectTransform;

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

				if (_sizeDeltaTarget.IsLerping())
				{
					rectTransform.sizeDelta = _sizeDeltaTarget.Update(deltaTime);
				}
			}
			#endregion
		}
	}
}