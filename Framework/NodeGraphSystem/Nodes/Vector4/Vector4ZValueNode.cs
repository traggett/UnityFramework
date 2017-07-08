using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector4")]
		[Serializable]
		public class Vector4ZValueNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<Vector4> _input = Vector4.zero;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return Vector4Nodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<float>
			public float GetValue()
			{
				return _input.GetValue().z;
			}
			#endregion
		}
	}
}