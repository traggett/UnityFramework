using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	using Maths;
	using Utils;

	namespace UI
	{
		public class IconAnimator : MonoBehaviour
		{
			#region Public Data
			public Image _backer;
			public float _size = 128f;

			//Animation stuff
			public float _showTime;
			public float _hideTime;
			public InterpolationType _showInterpolation;
			public InterpolationType _hideInterpolation;
			public AnimationCurve _alphaCurve;

			public bool _disableOnHidden;
			public Action _onHidden;

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
			private float _showLerp;
			private float _showLerpSpeed;
			private bool _shouldBeShowing;
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
					if (_showLerp < 1f)
					{
						_showLerp += deltaTime * _showLerpSpeed;

						if (_showLerp >= 1f)
						{
							ShowInstant();
						}
						else
						{
							_backer.color = ColorUtils.SetAlpha(_backer.color, _alphaCurve.Evaluate(_showLerp));

							float size = Interpolation.Interpolate(_showInterpolation, 0f, _size, _showLerp);
							((RectTransform)this.transform).sizeDelta = new Vector2(size, size);
						}
					}
				}
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
							_backer.color = ColorUtils.SetAlpha(_backer.color, _alphaCurve.Evaluate(_showLerp));

							float size = Interpolation.Interpolate(_hideInterpolation, 0f, _size, _showLerp);
							((RectTransform)this.transform).sizeDelta = new Vector2(size, size);
						}
					}
				}
			}

			private void ShowInstant()
			{
				_shouldBeShowing = true;
				_showLerp = 1f;
				_backer.color = ColorUtils.SetAlpha(_backer.color, 1f);
				((RectTransform)this.transform).sizeDelta = new Vector2(_size, _size);
			}

			private void HideInstant()
			{
				_shouldBeShowing = false;
				_showLerp = 0f;
				_backer.color = ColorUtils.SetAlpha(_backer.color, 0f);
				((RectTransform)this.transform).sizeDelta = Vector2.zero;

				_onHidden?.Invoke();

				if (_disableOnHidden)
					this.gameObject.SetActive(false);
			}
			#endregion
		}
	}
}