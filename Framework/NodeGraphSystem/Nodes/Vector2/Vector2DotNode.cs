using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector2")]
		[Serializable]
		public class Vector2DotNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<Vector2> _a = Vector2.zero;
			public NodeInputField<Vector2> _b = Vector2.zero;
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
				return Vector2.Dot(_a, _b);
			}
			#endregion
		}
	}
}