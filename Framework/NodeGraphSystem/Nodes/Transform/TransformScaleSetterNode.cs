using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Transform")]
		[Serializable]
		public class TransformScaleSetterNode : Node
		{
			#region Public Data
			
			public TransformNodeInputField _transform = new TransformNodeInputField();
			
			public NodeInputField<Vector3> _scale = Vector3.one;
			
			public NodeInputField<bool> _offsetFromOriginal = false;
			#endregion

			#region Private Data 
			private Vector3 _origScale;
			#endregion

			#region Node
			public override void Init()
			{
				Transform target = _transform;

				if (target != null)
				{
					_origScale = target.transform.localScale;
				}
			}

			public override void Update(float deltaTime)
			{
				Transform target = _transform;

				if (target != null)
				{
					Vector3 scale = _scale;

					if (_offsetFromOriginal)
						scale = Vector3.Scale(scale, _origScale);

					target.transform.localScale = scale;
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