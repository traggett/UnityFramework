using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector3")]
		[Serializable]
		public class Vector3AddNode : Node, IValueSource<Vector3>
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

			#region IValueSource<Vector3>
			public Vector3 GetValue()
			{
				return (Vector3)_a + (Vector3)_b;
			}
			#endregion
		}
	}
}