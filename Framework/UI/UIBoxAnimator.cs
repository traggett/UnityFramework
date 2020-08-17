using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	using Maths;
	using Utils;

	namespace UI
	{
		public class UIBoxAnimator : MonoBehaviour
		{
			#region Public Data
			public RectTransform _rectTransform;
			public Image _backer;
			public Graphic _content;
			public Vector2 _size;
			public Vector2 _hiddenSize;

			//Animation stuff
			public float _showTime;
			public float _hideTime;
			public float _resizeTime;
			public AnimationCurve _widthCurve;
			public AnimationCurve _heightCurve;
			public AnimationCurve _backerAlphaCurve;
			public AnimationCurve _contentAlphaCurve;
			public AnimationCurve _resizeCurve;

			public bool _disableOnHidden;
			public Action _onHidden;

			public float _targetTolerance = 1f;

			public bool Showing
			{
				get
				{
					return _showLerp > 0f;
				}
				set
				{
					if (value != _shouldBeShowing)
					{
						_shouldBeShowing = value;

						if (value)
						{
							this.gameObject.SetActive(true);

							if (_showTime > 0f)
							{
								_showLerpSpeed = 1f / _showTime;
								UpdateAnimations(0f);
							}
							else
							{
								ShowInstant();
							}
						}
						else
						{
							if (_hideTime > 0f && _showLerp > 0f)
							{
								_showLerpSpeed = 1f / _hideTime;

								if (_resizeLerp > 0f)
								{
									_size = _rectTransform.sizeDelta;
									_resizeLerp = 0f;
								}
							}
							else
							{
								HideInstant();
							}
						}
					}
				}
			}
			#endregion

			#region Private Data
			private bool _shouldBeShowing = true;
			private float _showLerp;
			private float _showLerpSpeed;
			private float _resizeLerp;
			private float _resizeLerpSpeed;
			private Vector2 _resizeFromSize;
			#endregion

			#region MonoBehaviour
			private void Awake()
			{
				ShowInstant();
			}

			private void OnDisable()
			{
				HideInstant();
			}

			private void Update()
			{
				UpdateAnimations(Time.deltaTime);
			}
			#endregion

			#region Private Functions
			private void UpdateAnimations(float deltaTime)
			{
				if (_shouldBeShowing)
				{
					//If still showing
					if (_showLerp < 1f)
					{
						_showLerp += deltaTime * _showLerpSpeed;

						if (_showLerp >= 1f)
						{
							ShowInstant();
						}
						else
						{
							_backer.color = ColorUtils.SetAlpha(_backer.color, _backerAlphaCurve.Evaluate(_showLerp));

							if (_content != null)
								_content.color = ColorUtils.SetAlpha(_content.color, _contentAlphaCurve.Evaluate(_showLerp));

							RectTransformUtils.SetWidth(_rectTransform, Mathf.Lerp(_hiddenSize.x, _size.x, _widthCurve.Evaluate(_showLerp)));
							RectTransformUtils.SetHeight(_rectTransform, Mathf.Lerp(_hiddenSize.y, _size.y, _heightCurve.Evaluate(_showLerp)));
						}
					}
					else
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
				}
				//Should hide...
				else
				{
					if (_showLerp > 0f)
					{
						_showLerp -= Time.deltaTime * _showLerpSpeed;

						if (_showLerp <= 0f)
						{
							HideInstant();
						}
						else
						{
							_backer.color = ColorUtils.SetAlpha(_backer.color, _backerAlphaCurve.Evaluate(_showLerp));

							if (_content != null)
								_content.color = ColorUtils.SetAlpha(_content.color, _contentAlphaCurve.Evaluate(_showLerp));

							RectTransformUtils.SetWidth(_rectTransform, Mathf.Lerp(_hiddenSize.x, _size.x, _widthCurve.Evaluate(_showLerp)));
							RectTransformUtils.SetHeight(_rectTransform, Mathf.Lerp(_hiddenSize.y, _size.y, _heightCurve.Evaluate(_showLerp)));
						}
					}
				}
			}

			private void ShowInstant()
			{
				_shouldBeShowing = true;
				_showLerp = 1f;
				_backer.color = ColorUtils.SetAlpha(_backer.color, 1f);

				if (_content != null)
					_content.color = ColorUtils.SetAlpha(_content.color, 1f);

				_rectTransform.sizeDelta = _size;
			}

			private void HideInstant()
			{
				_shouldBeShowing = false;
				_showLerp = 0f;
				_backer.color = ColorUtils.SetAlpha(_backer.color, 0f);

				if (_content != null)
					_content.color = ColorUtils.SetAlpha(_content.color, 0f);

				_rectTransform.sizeDelta = _hiddenSize;

				_onHidden?.Invoke();

				if (_disableOnHidden)
					this.gameObject.SetActive(false);
			}
			#endregion
		}
	}
}