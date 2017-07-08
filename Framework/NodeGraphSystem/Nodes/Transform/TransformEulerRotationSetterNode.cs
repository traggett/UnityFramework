using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Transform")]
		[Serializable]
		public class TransformEulerRotationSetterNode : Node
		{
			#region Public Data
			public TransformNodeInputField _transform;
			public NodeInputField<Vector3> _eulerAngles = Vector3.zero;
			public Space _space = Space.Self;
			[Tooltip("If set instead of setting an absolute value the value will be applied as an offset from the original value.")]
			public NodeInputField<bool> _offsetFromOriginal = false;
			#endregion

			#region Private Data 
			private Quaternion _origRotation;
			#endregion

			#region Node
			public override void Init()
			{
				Transform target = _transform;

				switch (_space)
				{
					case Space.Self:
						_origRotation = target.localRotation;
						break;
					case Space.World:
						_origRotation = target.rotation;
						break;
				}
			}

			public override void Update(float time, float deltaTime)
			{
				Transform target = _transform;

				if (target != null)
				{
					Quaternion rotation;

					if (_offsetFromOriginal)
						rotation = _origRotation * Quaternion.Euler(_eulerAngles);
					else
						rotation = Quaternion.Euler(_eulerAngles);

					switch (_space)
					{
						case Space.Self:
							target.localRotation = rotation;
							break;
						case Space.World:
							target.rotation = rotation;
							break;
					}
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