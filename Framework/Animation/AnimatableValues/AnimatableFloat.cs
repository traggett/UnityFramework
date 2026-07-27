using UnityEngine;
using System;

namespace Framework
{
	namespace Animations
	{
		using Maths;

		public sealed class AnimatableFloat : AnimatableValue<float>
		{
			#region AnimationCurveLayer
			private class AnimationCurveLayer : IAnimationLayer
			{
				#region Private Data
				protected AnimationCurve _animationCurve;
				protected float _animationTimer;
				protected float _currentValue;
				#endregion

				#region Public Interface
				public AnimationCurveLayer(AnimationCurve animationCurve)
				{
					_animationCurve = animationCurve;
					_animationTimer = 0f;
					_currentValue = default;
				}
				#endregion

				#region IAnimationLayer
				public float Value { get { return _currentValue; } }

				public bool IsFinished 
				{ 
					get 
					{
						// Layer is finished when animation curve is complete
						if (_animationCurve != null && _animationCurve.keys.Length > 0)
						{
							return _animationTimer >= _animationCurve.keys[^1].time;
						}

						return true;
					} 
				}

				public void Update(float deltaTime) 
				{ 
					_animationTimer += deltaTime;
					_currentValue = _animationCurve.Evaluate(_animationTimer);
				}
				#endregion
			}
			#endregion

			#region SinWaveLayer
			private class SinWaveLayer : IAnimationLayer
			{
				#region Private Data
				protected float _waveSpeed;
				protected float _animationTimer;
				protected float _currentValue;
				#endregion

				#region Public Interface
				public SinWaveLayer(float waveSpeed)
				{
					_waveSpeed = waveSpeed;
					_animationTimer = 0f;
				}
				#endregion

				#region AnimationLayer
				public float Value { get { return _currentValue; } }

				public bool IsFinished { get { return true; } }

				public void Update(float deltaTime)
				{
					_animationTimer += deltaTime * _waveSpeed;
					_currentValue = Mathf.Sin(_animationTimer);
				}
				#endregion
			}
			#endregion

			#region Public Interface
			public static implicit operator AnimatableFloat(float value)
			{
				AnimatableFloat interpolatableValue = new AnimatableFloat();
				interpolatableValue.Set(value);
				return interpolatableValue;
			}

			// Animates this float value using animationCurve (stops when reaches last key frame) 
			public void Animate(AnimationCurve animationCurve, InterpolationType easeInInterpolation = InterpolationType.Linear, float easeInTime = 0f, Action onFullyShowing = null)
			{
				if (animationCurve != null)
				{
					// TO DO - have pool of theses layers??
					AnimationCurveLayer animationLayer = new AnimationCurveLayer(animationCurve);
					AddAnimationLayer(animationLayer, easeInTime, easeInInterpolation, onFullyShowing);
				}
			}
			#endregion

			#region InterpolatableValue
			protected override float Lerp(float from, float to, float t)
			{
				return Mathf.Lerp(from, to, t);
			}
			#endregion
		}
	}
}