using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Transform")]
		[Serializable]
		public class TransformPositionSetterNode : Node
		{
			#region Public Data
			public TransformNodeInputField _transform;
			public NodeInputField<Vector3> _position = Vector3.zero;
			public Space _space = Space.Self;
			[Tooltip("If set instead of setting an absolute value the value will be applied as an offset from the original value.")]
			public NodeInputField<bool> _offsetFromOriginal = false;
			#endregion

			#region Private Data 
			private Vector3 _origPosition;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				Transform target = _transform;

				if (target != null)
				{
					if (IsFirstUpdate())
					{
						switch (_space)
						{
							case Space.Self:
								_origPosition = target.transform.localPosition;
								break;
							case Space.World:
								_origPosition = target.transform.position;
								break;
						}
					}

					Vector3 position = _position;

					if (_offsetFromOriginal)
						position += _origPosition;		

					switch (_space)
					{
						case Space.Self:
							target.transform.localPosition = position;
							break;
						case Space.World:
							target.transform.position = position;
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