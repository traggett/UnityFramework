using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Quaternions")]
		[Serializable]
		public class QuaternionLookRotationNode : Node, IValueSource<Quaternion>
		{
			#region Public Data	
			public NodeInputField<Vector3> _forward = Vector3.forward;
			public NodeInputField<Vector3> _up = Vector3.up;
			#endregion

			#region IValueSource<Quaternion>
			public Quaternion GetValue()
			{
				return Quaternion.LookRotation(_forward, _up);
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