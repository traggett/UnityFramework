using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class SquareWaveNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<float> _frequency = 0.5f;
			public NodeInputField<float> _offset = 0.0f;
			#endregion

			#region Private Data 
			private float _value;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				float wave = (time + _offset) * _frequency;
				_value = (wave % 2.0f) > 1.0f ? 1.0f : 0.0f;
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