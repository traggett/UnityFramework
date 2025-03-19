using UnityEngine;

namespace Framework
{
	using Framework.Utils;
	using Maths;
	using System;

	namespace Animations
	{
		public abstract class AnimatableValue<T>
		{
			#region AnimationLayer
			protected abstract class AnimationLayer
			{
				#region Private Data
				protected float _weight;
				private float _lerp;
				private float _lerpSpeed;
				private InterpolationType _interpolationType;
				private AnimationCurve _animationCurve;
				#endregion

				#region Public Interface
				public float Weight	{ get { return _weight; } }

				public bool FadeInWeight(float deltaTime)
				{
					_lerp += deltaTime * _lerpSpeed;
					bool done = _lerp >= 1f;

					_lerp = Mathf.Clamp01(_lerp);

					if (_animationCurve == null)
					{
						_weight = Interpolation.Interpolate(_interpolationType, 0f, 1f, _lerp);
					}
					else
					{
						_weight = _animationCurve.Evaluate(_lerp);
					}

					return done;
				}
				#endregion

				#region Protected Functions
				protected void OnStartLayer(InterpolationType easeInInterpolation, float easeInTime)
				{
					if (easeInTime > 0f)
					{
						_weight = 0f;
						_lerp = 0f;
						_lerpSpeed = 1f / easeInTime;
						_interpolationType = easeInInterpolation;
						_animationCurve = null;
					}
					else
					{
						_weight = 1f;
						_animationCurve = null;
					}
				}

				protected void OnStartLayer(AnimationCurve easeInCurve, float easeInTime)
				{
					if (easeInTime > 0f)
					{
						_weight = 0f;
						_lerp = 0f;
						_lerpSpeed = 1f / easeInTime;
						_animationCurve = easeInCurve;
					}
					else
					{
						_weight = 1f;
						_lerp = 1f;
						_lerpSpeed = 0f;
						_animationCurve = easeInCurve;
					}
				}
				#endregion

				#region Virtual Interface
				public abstract T Value { get; }
				public virtual void Update(float deltaTime) { }
				#endregion
			}
			#endregion

			#region ConstantValueLayer
			protected class ConstantValueLayer : AnimationLayer
			{
				#region Private Data
				protected T _value;
				#endregion

				#region Public Interface
				public void StartLayer(T value, InterpolationType easeInInterpolation, float easeInTime)
				{
					OnStartLayer(easeInInterpolation, easeInTime);
					_value = value;
				}

				public void StartLayer(T value, AnimationCurve easeInCurve, float easeInTime)
				{
					OnStartLayer(easeInCurve, easeInTime);
					_value = value;
				}
				#endregion

				#region AnimationLayer
				public override T Value { get { return _value; } }
				#endregion
			}
			#endregion

			#region Private Data
			private T _value;
			private readonly ConstantValueLayer _baseConstantValueLayer = new ConstantValueLayer();
			private AnimationLayer[] _currentLayers;
			private Action _onDone;
			#endregion

			#region Public Interface
			public static implicit operator T(AnimatableValue<T> property)
			{
				return property.Get();
			}

			public void Set(T value)
			{
				_baseConstantValueLayer.StartLayer(value, InterpolationType.Linear, -1f);
				_currentLayers = new AnimationLayer[] { _baseConstantValueLayer };
				_value = value;
				_onDone = null;
			}

			public T Get()
			{
				return _value;
			}

			public void LerpTo(T value, float time, Action onFullyShowing = null)
			{
				InterpolateTo(value, InterpolationType.Linear, time, onFullyShowing);
			}

			public void InterpolateTo(T value, InterpolationType interpolationType, float time, Action onFullyShowing = null)
			{
				if (time <= 0f)
				{
					Set(value);
				}
				else
				{
					// TO DO - have pool of theses layers??
					ConstantValueLayer constantValueLayer = new ConstantValueLayer();
					constantValueLayer.StartLayer(value, interpolationType, time);
					AddAnimationLayer(constantValueLayer, onFullyShowing);
				}
			}

			public void InterpolateTo(T value, AnimationCurve interpolationCurve, float time, Action onFullyShowing = null)
			{
				if (interpolationCurve == null || time <= 0f)
				{
					Set(value);
				}
				else
				{
					// TO DO - have pool of theses layers??
					ConstantValueLayer constantValueLayer = new ConstantValueLayer();
					constantValueLayer.StartLayer(value, interpolationCurve, time);
					AddAnimationLayer(constantValueLayer, onFullyShowing);
				}
			}

			public void Update(float deltaTime)
			{
				// Update all layers
				for (int i = 0; i < _currentLayers.Length; i++)
				{
					_currentLayers[i].Update(deltaTime);
				}

				// If we have a layer fade in progress (ie multiple layers)
				if (_currentLayers.Length > 1)
				{
					AnimationLayer topLayer = _currentLayers[^1];

					// Fade in top layer (returns true if fully faded in)
					if (topLayer.FadeInWeight(deltaTime))
					{
						_value = topLayer.Value;
						_currentLayers = new AnimationLayer[] { topLayer };
						_onDone?.Invoke();
						_onDone = null;
					}
					// Otherwise work out current value based on layer weights
					else
					{
						_value = _currentLayers[0].Value;

						for (int i = 1; i < _currentLayers.Length; i++)
						{
							_value = Lerp(_value, _currentLayers[i].Value, _currentLayers[i].Weight);
						}
					}
				}
				else
				{
					_value = _currentLayers[0].Value;
				}
			}
			#endregion

			#region Virtual Interface
			protected abstract T Lerp(T from, T to, float t);
			#endregion

			#region Protected Functions
			protected void AddAnimationLayer(AnimationLayer layer, Action onDone = null)
			{
				ArrayUtils.Add(ref _currentLayers, layer);
				_onDone = onDone;
			}
			#endregion
		}
	}
}