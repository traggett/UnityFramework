using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class FloatMultiplierNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<float> _a = 1.0f;
			public NodeInputField<float> _b = 1.0f;
			#endregion

			#region Node
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
				return _a * _b;
			}
			#endregion
		}
	}
}