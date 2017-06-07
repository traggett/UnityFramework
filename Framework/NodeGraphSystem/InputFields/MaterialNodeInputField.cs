using UnityEngine;
using System;

namespace Framework
{
	using Utils;

	namespace NodeGraphSystem
	{
		[Serializable]
		public class MaterialNodeInputField : NodeInputFieldBase<Material>
		{
			public MaterialRef _value = new MaterialRef();
			
			public static implicit operator Material(MaterialNodeInputField value)
			{
				return value.GetValue();
			}

			protected override Material GetStaticValue()
			{
				return _value;
			}

#if UNITY_EDITOR
			protected override void ClearStaticValue()
			{
				_value = new MaterialRef();
			}

			protected override bool RenderStaticValueProperty()
			{
				return _value.RenderObjectProperties(new GUIContent("Value"));
			}
#endif
		}
	}
}