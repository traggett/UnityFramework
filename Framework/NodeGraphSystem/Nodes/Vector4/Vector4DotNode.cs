using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector4")]
		[Serializable]
		public class Vector4DotNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<Vector4> _a = Vector4.zero;
			public NodeInputField<Vector4> _b = Vector4.zero;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return Vector3Nodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<float>
			public float GetValue()
			{
				return Vector4.Dot(_a, _b);
			}
			#endregion
		}
	}
}