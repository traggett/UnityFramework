using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Materials")]
		[Serializable]
		public class MaterialFloatSetterNode : MaterialNode
		{
			#region Public Data
			public NodeInputField<float> _value = 0.0f;
			[Tooltip("If set instead of setting an absolute value the value will be multiplied with the original value.")]
			public NodeInputField<bool> _multiplyWithOriginal = false;
			#endregion

			#region Private Data
			private float _cachedValue;
			private float _originalValue;
			#endregion

			#region Node
			public override void Init()
			{
				UpdateCachedShader();
				Material material = _material;
				if (material != null)
					_originalValue = material.GetFloat(_cachedShaderID);
			}

			public override void Update(float time, float deltaTime)
			{
				float value = _value;

				if (_multiplyWithOriginal)
					value *= _originalValue;

				//Only updated the material if value has changed for performance
				if (UpdateCachedShader() || _cachedValue != value)
				{
					Material material = _material;
					if (material != null)
					{
						material.SetFloat(_cachedShaderID, value);
						_cachedValue = value;
					}
				}
			}
			#endregion
		}
	}
}