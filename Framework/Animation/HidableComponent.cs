using System;
using UnityEngine;

namespace Framework
{
	namespace Animations
	{
		public abstract class HidableComponent : MonoBehaviour
		{
			#region Public Data
			public bool _showOnEnable;
			public bool _disableOnHidden;

			public float _showTime;
			public float _hideTime;

			public bool _unscaledTime;

			public Action _onShown;
			public Action _onHidden;

			public bool Active
			{
				get
				{
					return _shouldBeShowing;
				}
				set
				{
					if (value != _shouldBeShowing || !_initialised)
					{
						_shouldBeShowing = value;

						if (value)
						{
							if (!_initialised)
							{
								_showOnEnable = true;
							}

							this.gameObject.SetActive(true);

							if (_showTime > 0f)
							{
								_showLerpSpeed = 1f / _showTime;
								OnStartShowAnimation();
							}
							else
							{
								OnStartShowAnimation();
								OnShown();
							}
						}
						else
						{
							if (!_initialised)
							{
								_showOnEnable = false;
							}

							if (_hideTime > 0f && _showLerp > 0f)
							{
								_showLerpSpeed = 1f / _hideTime;
								OnStartHideAnimation();
							}
							else
							{
								OnHidden();
							}
						}
					}
				}
			}
			#endregion

			#region Private Data
			private bool _shouldBeShowing;
			private float _showLerp;
			private float _showLerpSpeed;
			private bool _initialised;
			#endregion

			#region Unity Messages
			protected virtual void Awake()
			{
				_initialised = true;
				_shouldBeShowing = false;
				_showLerp = 0f;
				OnUpdateAnimations(0f, false);
			}

			protected virtual void OnEnable()
			{
				if (_showOnEnable)
					Active = true;
			}

			protected virtual void OnDisable()
			{
				HideInstant();
			}

			protected virtual void Update()
			{
				UpdateAnimations(_unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
			}
			#endregion

			#region Public Interface
			public void ShowInstant()
			{
				if (!_shouldBeShowing || _showLerp < 1f)
				{
					this.gameObject.SetActive(true);
					OnShown();
				}
			}

			public void HideInstant()
			{
				if (_shouldBeShowing || _showLerp > 0f)
				{
					OnHidden();
				}
			}

			public bool IsFullyShowing()
			{
				return _shouldBeShowing && _showLerp >= 1f;
			}

			public bool IsVisible()
			{
				return _showLerp > 0f;
			}
			#endregion

			#region Virtual Interface
			protected virtual void OnStartShowAnimation()
			{
				UpdateAnimations(0f);
			}

			protected virtual void OnStartHideAnimation()
			{

			}

			protected abstract void OnUpdateAnimations(float showLerp, bool showing);

			protected virtual void OnUpdateFullyShown()
			{

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
							OnShown();
						}
						else
						{
							OnUpdateAnimations(_showLerp, true);
						}
					}
					else
					{
						OnUpdateFullyShown();
					}
				}
				//Should hide...
				else
				{
					if (_showLerp > 0f)
					{
						_showLerp -= deltaTime * _showLerpSpeed;

						if (_showLerp <= 0f)
						{
							OnHidden();
						}
						else
						{
							OnUpdateAnimations(_showLerp, false);
						}
					}
				}
			}

			private void OnShown()
			{
				_shouldBeShowing = true;
				_showLerp = 1f;
				OnUpdateAnimations(1f, true);

				_onShown?.Invoke();
			}

			private void OnHidden()
			{
				_shouldBeShowing = false;
				_showLerp = 0f;
				OnUpdateAnimations(0f, false);

				_onHidden?.Invoke();

				if (_disableOnHidden)
					this.gameObject.SetActive(false);
			}
			#endregion
		}
	}
}