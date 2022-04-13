using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Materials")]
		[Serializable]
		public class MaterialTextureSetterNode : MaterialNode
		{
			#region Public Data
			public TextureNodeInputField _value;
			#endregion

			#region Private Data
			private Texture _cachedValue;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				Texture value = _value;

				//Only updated the material if value has changed for performance
				if (UpdateCachedShader() || _cachedValue != value)
				{
					Material material = _material;
					if (material != null)
					{
						material.SetTexture(_cachedShaderID, value);
						_cachedValue = value;
					}
				}
			}
			#endregion
		}
	}
}