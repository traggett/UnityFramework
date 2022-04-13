using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class PerlinNoiseFloatNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<float> _speed = 0.5f;
			#endregion

			#region Private Data 
			private static readonly float kSpeedVariation = 0.05f;
			private float _value;
			private float _xSeed = Random.value;
			private float _ySeed = Random.value;
			private float _xSpeed;
			private float _ySpeed;
			private float _xySpeedVariance = Random.Range(-kSpeedVariation, kSpeedVariation);
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				float speed = _speed.GetValue();
				_xSpeed = speed * (1.0f + _xySpeedVariance);
				_ySpeed = speed * (1.0f - _xySpeedVariance);
				
				_value = Mathf.PerlinNoise(_xSeed + time * _xSpeed, _ySeed + time * _ySpeed);
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return FloatNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<float>
			public float GetValue()
			{
				return _value;
			}
			#endregion
		}
	}
}