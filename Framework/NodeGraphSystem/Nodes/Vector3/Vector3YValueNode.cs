using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector3")]
		[Serializable]
		public class Vector3YValueNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<Vector3> _input = Vector3.zero;
			#endregion

			#region Private Data
			private float _value;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				_value = _input.GetValue().y;
			}

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
				return _value;
			}
			#endregion
		}
	}
}