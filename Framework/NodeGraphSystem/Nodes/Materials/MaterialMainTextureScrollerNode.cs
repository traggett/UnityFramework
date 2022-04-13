using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Materials")]
		[Serializable]
		public class MaterialMainTextureScrollerNode : Node
		{
			#region Public Data
			public MaterialNodeInputField _material;
			public NodeInputField<Vector2> _speed = Vector2.zero;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				Material material = _material;

				if (material != null)
				{
					material.mainTextureOffset += (Vector2)_speed * deltaTime;
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return MaterialNodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}