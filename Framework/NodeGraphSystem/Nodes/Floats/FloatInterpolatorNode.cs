using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;
	using Maths;

	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class FloatInterpolatorNode : Node, IValueSource<float>
		{
			#region Public Data	
			public NodeInputField<float> _from = 0.0f;
			public NodeInputField<float> _to = 1.0f;
			public NodeInputField<float> _t = 0.0f;
			public InterpolationType _interpolationtype = InterpolationType.Linear;
			#endregion

			#region Private Data
			private float _value;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				_value = MathUtils.Interpolate(_interpolationtype, _from, _to, _t);
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