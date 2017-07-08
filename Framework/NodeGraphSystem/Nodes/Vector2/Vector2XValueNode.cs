using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector2")]
		[Serializable]
		public class Vector2XValueNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<Vector2> _input = Vector2.zero;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return Vector2Nodes.kNodeColor;
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