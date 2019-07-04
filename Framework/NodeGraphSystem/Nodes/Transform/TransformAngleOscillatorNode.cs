using UnityEngine;
using System;

namespace Framework
{
	using Maths;
	
	namespace NodeGraphSystem
	{
		[NodeCategory("Transform")]
		[Serializable]
		public class TransformAngleOscillatorNode : Node
		{
			#region Public Data
			public TransformNodeInputField _transform;
			public NodeInputField<float> _amplitude = 0.0f;
			public NodeInputField<float> _axisDuration = 0.0f;
			public NodeInputField<float> _axisDeltaSpeed = 0.0f;
			public NodeInputField<FloatRange> _xAngleRange = new FloatRange(0.0f, 0.0f);
			public NodeInputField<FloatRange> _yAngleRange = new FloatRange(0.0f, 0.0f);
			public NodeInputField<FloatRange> _zAngleRange = new FloatRange(0.0f, 0.0f);
			#endregion

			#region Private Data 
			private Quaternion _origRotation;
			private Vector3 _currentEulerDirection;
			private Vector3 _prevTargetEulerDirection;
			private Vector3 _targetEulerDirection;
			private float _timer;
			private float _prevAmplitude;
			private float _directionLerp;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				Transform target = _transform;

				if (target != null)
				{
					if (IsFirstUpdate())
					{
						_origRotation = target.localRotation;
						_prevAmplitude = 0.0f;
						PickOscillationAxis();
					}

					float amplitude = _amplitude;

					//Update axis
					{
						if (_timer > 0.0f)
							_timer -= deltaTime;

						if (_timer <= 0.0f && Mathf.Sign(amplitude) != Mathf.Sign(_prevAmplitude))
						{
							PickOscillationAxis();
						}
					}

					if (_directionLerp < 1.0f)
					{
						_directionLerp += _axisDeltaSpeed * deltaTime;
						_currentEulerDirection = MathUtils.Interpolate(InterpolationType.InOutSine, _prevTargetEulerDirection, _targetEulerDirection, Mathf.Clamp01(_directionLerp));
					}

					target.localRotation = _origRotation * Quaternion.Euler(_currentEulerDirection * amplitude);
					_prevAmplitude = amplitude;
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return TransformNodes.kNodeColor;
			}
#endif
			#endregion

			#region Private Functions
			private void PickOscillationAxis()
			{
				Vector3 axis = new Vector3(_xAngleRange.GetValue().GetRandomSignedValue(), _yAngleRange.GetValue().GetRandomSignedValue(), _zAngleRange.GetValue().GetRandomSignedValue());
				_timer = _axisDuration;

				if (_timer > 0.0f)
				{
					_prevTargetEulerDirection = _targetEulerDirection;
					_targetEulerDirection = axis;
					_directionLerp = 0.0f;
				}
				else
				{
					_directionLerp = 1.0f;
					_currentEulerDirection = axis;
				}
			}
			#endregion
		}
	}
}