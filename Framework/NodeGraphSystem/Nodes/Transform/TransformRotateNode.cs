using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Transform")]
		[Serializable]
		public class RotateTransformNode : Node
		{
			#region Public Data
			public TransformNodeInputField _transform;		
			public NodeInputField<Vector3> _axis = Vector3.zero;
			public NodeInputField<float> _angle = 0.0f;
			public Space _space = Space.Self;
			#endregion
			
			#region Node
			public override void Update(float time, float deltaTime)
			{
				Transform target = _transform;

				if (target != null)
				{
					Vector3 axis = _axis;
					target.Rotate(axis, _angle, _space);
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return TransformNodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}