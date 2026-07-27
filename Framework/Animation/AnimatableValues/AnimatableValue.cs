namespace Framework
{
	using Maths;
	using System;
	using UnityEngine;

	namespace Animations
	{
		public abstract class AnimatableValue<T>
		{
			#region AnimationLayer
			protected interface IAnimationLayer
			{
				public T Value { get; }
				public bool IsFinished { get; }
				public void Update(float deltaTime);
			}
			#endregion

			#region ConstantValueLayer
			protected class ConstantValueLayer : IAnimationLayer
			{
				#region Private Data
				protected T _value;
				#endregion

				#region Public Interface
				public ConstantValueLayer(T value)
				{
					_value = value;
				}
				#endregion

				#region IAnimationLayer
				public T Value { get { return _value; } }
				public bool IsFinished { get { return true; } }
				public void Update(float deltaTime)
				{
					
				}
				#endregion
			}
			#endregion

			#region Private Data

			// Make this setable in code??
			private const uint MAX_LAYERS = 8;

			private struct LayerData
			{
				public IAnimationLayer _layer;
				public float _weight;
				public float _blendProgress;
				public float _blendSpeed;
				public InterpolationType _blendInterpolation;
			}

			private T _value;
			private readonly LayerData[] _animationLayers;
			private Action _onDone;

			#endregion

			#region Public Interface
			public static implicit operator T(AnimatableValue<T> property)
			{
				return property.Get();
			}

			protected AnimatableValue()
			{
				_animationLayers = new LayerData[MAX_LAYERS];
				_value = default;
			}

			public void Set(T value)
			{
				ConstantValueLayer constantValueLayer = new ConstantValueLayer(value);

				for (int i = 0; i < _animationLayers.Length; i++)
				{
					ClearLayer(i);
				}

				_animationLayers[0]._layer = constantValueLayer;
				_animationLayers[0]._blendProgress = 1f;
				_animationLayers[0]._weight = 1f;

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
					ConstantValueLayer constantValueLayer = new ConstantValueLayer(value);
					AddAnimationLayer(constantValueLayer, time, interpolationType, onFullyShowing);
				}
			}

			public void Update(float deltaTime)
			{
				// Update all layers in top down order
				for (int i = _animationLayers.Length -1; i >= 0; i--)
				{
					if (_animationLayers[i]._layer != null)
					{
						_animationLayers[i]._layer.Update(deltaTime);

						// If layer isn't fully blended in then update the blend
						if (_animationLayers[i]._blendProgress < 1f)
						{
							_animationLayers[i]._blendProgress += deltaTime * _animationLayers[i]._blendSpeed;
							bool done = _animationLayers[i]._blendProgress >= 1f;

							_animationLayers[i]._blendProgress = Mathf.Clamp01(_animationLayers[i]._blendProgress);
							_animationLayers[i]._weight = Interpolation.Interpolate(_animationLayers[i]._blendInterpolation, 0f, 1f, _animationLayers[i]._blendProgress);

							// If layer is no fully blended then remove all layers beneath it
							if (done)
							{
								ShiftLayersDown(i);
								break;
							}
						}
					}
				}

				// Work out current value
				_value = default;

				for (int i = 0; i < _animationLayers.Length; i++)
				{
					if (_animationLayers[i]._layer != null)
					{
						if (_animationLayers[i]._blendProgress >= 1f)
						{
							_value = _animationLayers[i]._layer.Value;
						}
						else
						{
							_value = Lerp(_value, _animationLayers[i]._layer.Value, _animationLayers[i]._weight);
						}	
					}
				}

				// Check top layer is done and trigger callback
				for (int i = _animationLayers.Length - 1; i >= 0; i--)
				{
					if (_animationLayers[i]._layer != null)
					{
						if (_onDone != null && _animationLayers[i]._blendProgress >= 1f && _animationLayers[i]._layer.IsFinished)
						{
							_onDone?.Invoke();
							_onDone = null;
						}

						break;
					}
				}
			}
			#endregion

			#region Virtual Interface
			protected abstract T Lerp(T from, T to, float t);
			#endregion

			#region Protected Functions
			protected void AddAnimationLayer(IAnimationLayer layer, float blendTime, InterpolationType blendInterpolation, Action onDone = null)
			{
				int layerIndex = -1;

				// Find first free layer
				for (int i = 0; i < _animationLayers.Length; i++)
				{
					if (_animationLayers[i]._layer == null)
					{
						layerIndex = i;
						break;
					}
				}

				// If no free layer found then remove oldest and set top to be this one
				if (layerIndex == -1)
				{
					//Shift all layers down by one
					ShiftLayersDown(1);

					layerIndex = _animationLayers.Length - 1;
				}

				_animationLayers[layerIndex]._layer = layer;
				_animationLayers[layerIndex]._weight = blendTime > 0f ? 0f : 1f;
				_animationLayers[layerIndex]._blendProgress = blendTime > 0f ? 0f : 1f;
				_animationLayers[layerIndex]._blendSpeed = blendTime > 0f ? 1f / blendTime : 0f;
				_animationLayers[layerIndex]._blendInterpolation = blendInterpolation;

				_onDone = onDone;
			}
			#endregion

			#region Private Functions
			private void ShiftLayersDown(int shiftAmount)
			{
				for (int i = 0; i < _animationLayers.Length; i++)
				{
					if (i + shiftAmount < _animationLayers.Length)
					{
						ClearLayer(i);
						_animationLayers[i] = _animationLayers[i + shiftAmount];
					}
					else
					{
						ClearLayer(i);
					}
				}
			}

			private void ClearLayer(int layerIndex)
			{
				//TO DO - use pooled struct system??

				_animationLayers[layerIndex]._layer = null;
			}
			#endregion
		}
	}
}