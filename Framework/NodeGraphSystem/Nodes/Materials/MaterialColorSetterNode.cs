using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Materials")]
		[Serializable]
		public class MaterialColorSetterNode : MaterialNode
		{
			#region Public Data			
			public NodeInputField<Color> _value = Color.clear;
			#endregion

			#region Private Data
			private Color _cachedValue;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				Color value = _value;

				//Only updated the material if value has changed for performance
				if (UpdateCachedShader() || _cachedValue != value)
				{
					Material material = _material;
					if (material != null)
					{
						material.SetColor(_cachedShaderID, value);
						_cachedValue = value;
					}
				}
			}
			#endregion
		}
	}
}