using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector3")]
		[Serializable]
		public class Vector3CompositeNode : Node, IValueSource<Vector3>
		{
			#region Public Data
			public NodeInputField<float> _x = 0.0f;
			public NodeInputField<float> _y = 0.0f;
			public NodeInputField<float> _z = 0.0f;
			#endregion

			#region Private Data
			private Vector3 _value;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				_value = new Vector3(_x, _y, _z);
			}

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
				return _value;
			}
			#endregion
		}
	}
}