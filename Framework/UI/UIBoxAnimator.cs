using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	using Maths;
	using Utils;
	using AnimationSystem;

	namespace UI
	{
		public class UIBoxAnimator : HidableComponent
		{
			#region Public Data
			public RectTransform _rectTransform;
			public Image _backer;
			public Graphic _content;
			public Vector2 _size;
			public Vector2 _hiddenSize;
			public float _resizeTime;
			public AnimationCurve _widthCurve;
			public AnimationCurve _heightCurve;
			public AnimationCurve _backerAlphaCurve;
			public AnimationCurve _contentAlphaCurve;
			public AnimationCurve _resizeCurve;

			public float _targetTolerance = 1f;
			#endregion

			#region Private Data
			private float _resizeLerp;
			private float _resizeLerpSpeed;
			private Vector2 _resizeFromSize;
			#endregion

			#region HidableComponent
			protected override void OnStartHideAnimation()
			{
				if (_resizeLerp > 0f)
				{
					_size = _rectTransform.sizeDelta;
					_resizeLerp = 0f;
				}
			}

			protected override void OnUpdateAnimations(float showLerp, bool showing)
			{
				_backer.color = ColorUtils.SetAlpha(_backer.color, _backerAlphaCurve.Evaluate(showLerp));

				if (_content != null)
					_content.color = ColorUtils.SetAlpha(_content.color, _contentAlphaCurve.Evaluate(showLerp));

				RectTransformUtils.SetWidth(_rectTransform, Mathf.Lerp(_hiddenSize.x, _size.x, _widthCurve.Evaluate(showLerp)));
				RectTransformUtils.SetHeight(_rectTransform, Mathf.Lerp(_hiddenSize.y, _size.y, _heightCurve.Evaluate(showLerp)));
			}

			protected override void OnUpdateFullyShown()
			{
				if (_resizeLerp > 0f)
				{
					_resizeLerp -= Time.deltaTime * _resizeLerpSpeed;

					if (_resizeLerp <= 0f)
					{
						_rectTransform.sizeDelta = _size;
					}
					else
					{
						_rectTransform.sizeDelta = Vector2.Lerp(_resizeFromSize, _size, _resizeCurve.Evaluate(1f - _resizeLerp));
					}
				}
				else if (!MathUtils.Approximately(_rectTransform.sizeDelta, _size, _targetTolerance))
				{
					_resizeLerp = 1f;
					_resizeFromSize = _rectTransform.sizeDelta;
					_resizeLerpSpeed = 1f / _resizeTime;
				}
			}
			#endregion
		}
	}
}