using UnityEngine;
using System;

namespace Framework
{
	using Utils;

	namespace NodeGraphSystem
	{
		[Serializable]
		public class TextureNodeInputField : NodeInputFieldBase<Texture>
		{
			public AssetRef<Texture> _value;
			
			public static implicit operator Texture(TextureNodeInputField value)
			{
				return value.GetValue();
			}

			protected override Texture GetStaticValue()
			{
				return _value;
			}

#if UNITY_EDITOR
			protected override void ClearStaticValue()
			{
				_value = new AssetRef<Texture>();
			}
#endif
		}
	}
}