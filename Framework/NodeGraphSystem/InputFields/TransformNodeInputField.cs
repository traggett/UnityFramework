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
			public ComponentRef<Transform> _value = new ComponentRef<Transform>();

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

			protected override bool RenderStaticValueProperty()
			{
				return _value.RenderObjectProperties(new GUIContent("Value"));
			}
#endif
		}
	}
}