using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector4")]
		[Serializable]
		public class Vector4XValueNode : Node, IValueSource<float>
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
				return _input.GetValue().x;
			}
			#endregion
		}
	}
}