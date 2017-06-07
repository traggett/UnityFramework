using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace Framework
{
	using ValueSourceSystem;

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
			private float _time;
			private float _value;
			private float _xSeed;
			private float _ySeed;
			private float _xSpeed;
			private float _ySpeed;
			private float _xySpeedVariance;
			#endregion

			#region Node
			public override void Init()
			{
				_time = 0.0f;
				_xSeed = Random.value;
				_ySeed = Random.value;
				_xySpeedVariance = Random.Range(-kSpeedVariation, kSpeedVariation);
			}

			public override void Update(float deltaTime)
			{
				float speed = _speed.GetValue();
				_xSpeed = speed * (1.0f + _xySpeedVariance);
				_ySpeed = speed * (1.0f - _xySpeedVariance);

				_time += deltaTime;
				_value = Mathf.PerlinNoise(_xSeed + _time * _xSpeed, _ySeed + _time * _ySpeed);
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