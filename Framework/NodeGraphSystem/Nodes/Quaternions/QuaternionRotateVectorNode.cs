using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Quaternions")]
		[Serializable]
		public class QuaternionRotateVectorNode : Node, IValueSource<Vector3>
		{
			#region Public Data	
			public NodeInputField<Quaternion> _rotation = Quaternion.identity;
			public NodeInputField<Vector3> _vector = Vector3.zero;
			#endregion

			#region IValueSource<Quaternion>
			public Vector3 GetValue()
			{
				return (Quaternion)_rotation * _vector;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return QuaternionNodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}