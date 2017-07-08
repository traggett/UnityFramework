using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Transform")]
		[Serializable]
		public class TransformGetPositionNode : Node, IValueSource<Vector3>
		{
			#region Public Data		
			public TransformNodeInputField _transform = new TransformNodeInputField();
			public Space _space = Space.World;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return TransformNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<Vector3>
			public Vector3 GetValue()
			{
				Transform transform = _transform;

				if (transform != null)
				{
					switch (_space)
					{
						case Space.Self: return transform.localPosition;
						case Space.World: return transform.position;
					}
				}

				return Vector3.zero;
			}
			#endregion
		}
	}
}