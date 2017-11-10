using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;
	
	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class FloatCurveNode : Node, IValueSource<float>
		{
			#region Public Data	
			public NodeInputField<AnimationCurve> _curve = new NodeInputField<AnimationCurve>();
			public NodeInputField<float> _input = 0.0f;
			#endregion

			#region Private Data
			private float _value;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				AnimationCurve curve = _curve;

				if (curve != null)
					_value = curve.Evaluate(_input);
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