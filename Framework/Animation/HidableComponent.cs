using System;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
	namespace Animations
	{
		public abstract class HidableComponent : MonoBehaviour
		{
			#region Serialised Data
			[Header("Animation Params")]
			[SerializeField] private bool _showOnEnable;
			[SerializeField] private bool _disableOnHidden;
			[SerializeField] private float _showTime;
			[SerializeField] private float _hideTime;
			[SerializeField] private bool _unscaledTime;

			[Serializable]
			public class AnimtationEvent : UnityEvent { }

			[Header("Animation Events")]
			
			[Tooltip("Event that is triggered when the show animations start.")]
			[SerializeField] private AnimtationEvent _onShow;
			
			[Tooltip("Event that is triggered when the component is fully shown.")]
			[SerializeField] private AnimtationEvent _onShown;
			
			[Tooltip("Event that is triggered when the hide animations start.")]
			[SerializeField] private AnimtationEvent _onHide;
			
			[Tooltip("Event that is triggered when the component is fully hidden.")]
			[SerializeField] private AnimtationEvent _onHidden;
			#endregion

			#region Public Properties
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
							this.gameObject.SetActive(true);

							if (_showTime > 0f)
							{
								_showLerpSpeed = 1f / _showTime;
								OnStartShowAnimation();
							}
							else
							{
								OnStartShowAnimation();
								OnShowComplete();
							}
						}
						else
						{
							if (_hideTime > 0f && _showLerp > 0f)
							{
								_showLerpSpeed = 1f / _hideTime;
								OnStartHideAnimation();
							}
							else
							{
								OnHideComplete();
							}
						}
					}
				}
			}

			public AnimtationEvent OnShow
			{
				get { return _onShow; }
			}

			public AnimtationEvent OnShown
			{
				get { return _onShown; }
			}

			public AnimtationEvent OnHide
			{
				get { return _onHide; }
			}

			public AnimtationEvent OnHidden
			{
				get { return _onHidden; }
			}

			public float ShowTime
			{
				get { return _showTime; }

				set 
				{
					_showTime = value;

					if (!_shouldBeShowing && _showLerp < 1f && _showTime > 0f)
					{
						_showLerpSpeed = 1f / _showTime;
					}
				}
			}

			public float HideTime
			{
				get { return _hideTime; }

				set
				{
					_hideTime = value;

					if (!_shouldBeShowing && _showLerp > 0f && _hideTime > 0f)
					{
						_showLerpSpeed = 1f / _hideTime;
					}				
				}
			}

			public bool UnscaledTime
			{
				get { return _unscaledTime; }

				set
				{
					_unscaledTime = value;
				}
			}
			#endregion

			#region Private Data
			private bool _shouldBeShowing = false;
			private float _showLerp = 0f;
			private float _showLerpSpeed = 0f;
			private bool _initialised = false;
			#endregion

			#region Unity Messages
			protected virtual void Awake()
			{
				_initialised = true;

				if (!_shouldBeShowing && !_showOnEnable)
				{
					OnHideComplete();
				}
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
					OnShowComplete();
				}
			}

			public void HideInstant()
			{
				if (_shouldBeShowing || _showLerp > 0f)
				{
					OnHideComplete();
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
				_onShow?.Invoke();
			}

			protected virtual void OnStartHideAnimation()
			{
				_onHide?.Invoke();
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
							OnShowComplete();
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
							OnHideComplete();
						}
						else
						{
							OnUpdateAnimations(_showLerp, false);
						}
					}
				}
			}

			private void OnShowComplete()
			{
				_shouldBeShowing = true;
				_showLerp = 1f;
				OnUpdateAnimations(1f, true);

				_onShown?.Invoke();
			}

			private void OnHideComplete()
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