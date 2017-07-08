using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector4")]
		[Serializable]
		public class Vector4CompositeNode : Node, IValueSource<Vector4>
		{
			#region Public Data
			public NodeInputField<float> _x = 0.0f;
			public NodeInputField<float> _y = 0.0f;
			public NodeInputField<float> _z = 0.0f;
			public NodeInputField<float> _w = 0.0f;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return Vector4Nodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<Vector4>
			public Vector4 GetValue()
			{
				return new Vector4(_x, _y, _z, _w);
			}
			#endregion
		}
	}
}