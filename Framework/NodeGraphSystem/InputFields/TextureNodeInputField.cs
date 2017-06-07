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
			public AssetRef<Texture> _value = new AssetRef<Texture>();
			
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
				_value.ClearAsset();
			}

			protected override bool RenderStaticValueProperty()
			{
				return _value.RenderObjectProperties(new GUIContent("Value"));
			}
#endif
		}
	}
}