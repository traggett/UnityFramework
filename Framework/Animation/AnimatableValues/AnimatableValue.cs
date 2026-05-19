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
				private float _easeInT;
				private float _easeInSpeed;
				private InterpolationType _interpolationType;
				private AnimationCurve _animationCurve;
				#endregion

				#region Public Interface
				public float Weight	{ get { return _weight; } }

				public bool FadeInWeight(float deltaTime)
				{
					if (_easeInT < 1f)
					{
						_easeInT += deltaTime * _easeInSpeed;
						bool done = _easeInT >= 1f;

						_easeInT = Mathf.Clamp01(_easeInT);

						if (_animationCurve == null)
						{
							_weight = Interpolation.Interpolate(_interpolationType, 0f, 1f, _easeInT);
						}
						else
						{
							_weight = _animationCurve.Evaluate(_easeInT);
						}

						return done;
					}

					return true;
				}
				#endregion

				#region Protected Functions
				protected void OnStartLayer(InterpolationType easeInInterpolation, float easeInTime)
				{
					if (easeInTime > 0f)
					{
						_weight = 0f;
						_easeInT = 0f;
						_easeInSpeed = 1f / easeInTime;
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
						_easeInT = 0f;
						_easeInSpeed = 1f / easeInTime;
						_animationCurve = easeInCurve;
					}
					else
					{
						_weight = 1f;
						_easeInT = 1f;
						_easeInSpeed = 0f;
						_animationCurve = easeInCurve;
					}
				}
				#endregion

				#region Virtual Interface
				public abstract T Value { get; }
				public virtual bool IsFinished { get { return _easeInT >= 1f; } }
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

				// Fade in layer weights
				for (int i = _currentLayers.Length -1; i >= 0; i--)
				{
					// Fade in layer, if full weight then remove all layers beneath
					if (_currentLayers[i].FadeInWeight(deltaTime))
					{
						//Remove all other layers
						AnimationLayer[] animationLayers = new AnimationLayer[_currentLayers.Length - i];
						
						for (int j = 0; j < animationLayers.Length; j++)
						{
							animationLayers[j] = _currentLayers[i + j];
						}

						break;
					}
				}

				// Work out value
				_value = _currentLayers[0].Value;

				for (int i = 1; i < _currentLayers.Length; i++)
				{
					_value = Lerp(_value, _currentLayers[i].Value, _currentLayers[i].Weight);
				}

				// Check top layer is done and trigger callback
				AnimationLayer topLayer = _currentLayers[^1];

				if (_onDone != null && topLayer.IsFinished)
				{
					_onDone?.Invoke();
					_onDone = null;
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