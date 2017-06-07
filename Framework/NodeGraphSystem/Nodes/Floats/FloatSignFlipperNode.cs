using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class FloatSignFlipperNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<float> _input = 1.0f;
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
				return -_input;
			}
			#endregion
		}
	}
}