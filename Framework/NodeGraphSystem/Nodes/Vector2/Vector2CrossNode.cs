using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;
	using Maths;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector2")]
		[Serializable]
		public class Vector2CrossNode : Node, IValueSource<Vector2>
		{
			#region Public Data
			public NodeInputField<Vector2> _vector = Vector2.zero;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return Vector2Nodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<Vector2>
			public Vector2 GetValue()
			{
				return MathUtils.Cross(_vector);
			}
			#endregion
		}
	}
}