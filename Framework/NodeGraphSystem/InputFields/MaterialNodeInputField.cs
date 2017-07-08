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
			public MaterialRef _value;
			
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
				_value = null;
			}
#endif
		}
	}
}