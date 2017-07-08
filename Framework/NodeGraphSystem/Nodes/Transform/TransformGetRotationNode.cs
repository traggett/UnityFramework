using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Transform")]
		[Serializable]
		public class TransformGetRotationNode : Node, IValueSource<Quaternion>
		{
			#region Public Data		
			public TransformNodeInputField _transform;
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

			#region IValueSource<Quaternion>
			public Quaternion GetValue()
			{
				Transform transform = _transform;

				if (transform != null)
				{
					switch (_space)
					{
						case Space.Self: return transform.localRotation;
						case Space.World: return transform.rotation;
					}
				}

				return Quaternion.identity;
			}
			#endregion
		}
	}
}