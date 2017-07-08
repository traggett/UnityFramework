using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Transform")]
		[Serializable]
		public class TransformGetScaleNode : Node, IValueSource<Vector3>
		{
			#region Public Data		
			public TransformNodeInputField _transform = new TransformNodeInputField();	
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
					return transform.localScale;
				}

				return Vector3.one;
			}
			#endregion
		}
	}
}