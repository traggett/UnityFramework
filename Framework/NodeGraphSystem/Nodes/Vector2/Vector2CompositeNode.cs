using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector2")]
		[Serializable]
		public class Vector2CompositeNode : Node, IValueSource<Vector2>
		{
			#region Public Data
			public NodeInputField<float> _x = 0.0f;
			public NodeInputField<float> _y = 0.0f;
			#endregion

			#region Private Data
			private Vector2 _value;
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
				return new Vector2(_x, _y);
			}
			#endregion
		}
	}
}