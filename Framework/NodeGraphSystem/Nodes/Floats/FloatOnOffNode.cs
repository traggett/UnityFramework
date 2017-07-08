using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;
	
	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class FloatOnOffNode : Node, IValueSource<float>
		{
			#region Public Data
			[Tooltip("If the input value is true the node will return 1 otherwise it will return 0.")]
			public NodeInputField<bool> _input = false;
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
				return _input ? 1.0f : 0.0f;
			}
			#endregion
		}
	}
}