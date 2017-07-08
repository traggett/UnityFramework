using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector3")]
		[Serializable]
		public class Vector3DotNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<Vector3> _a = Vector3.zero;
			public NodeInputField<Vector3> _b = Vector3.zero;
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
				return Vector3.Dot(_a, _b);
			}
			#endregion
		}
	}
}