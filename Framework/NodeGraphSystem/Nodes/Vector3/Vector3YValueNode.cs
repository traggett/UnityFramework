using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector3")]
		[Serializable]
		public class Vector3YValueNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<Vector3> _input = Vector3.zero;
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
				return _input.GetValue().y;
			}
			#endregion
		}
	}
}