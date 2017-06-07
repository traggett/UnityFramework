using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;
	
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

			#region IValueSource<float>
			public float GetValue()
			{
				AnimationCurve curve = _curve;

				if (curve != null)
				{
					return curve.Evaluate(_input);
				}

				return _input;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return FloatNodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}