using UnityEngine;
using System;

namespace Framework
{
	using Utils;

	namespace NodeGraphSystem
	{
		[Serializable]
		public class TransformNodeInputField : NodeInputFieldBase<Transform>
		{
			public ComponentRef<Transform> _value;

			public static implicit operator Transform(TransformNodeInputField value)
			{
				return value.GetValue();
			}

			protected override Transform GetStaticValue()
			{
				return _value.GetComponent();
			}

#if UNITY_EDITOR
			protected override void ClearStaticValue()
			{
				_value.ClearComponent();
			}
#endif
		}
	}
}