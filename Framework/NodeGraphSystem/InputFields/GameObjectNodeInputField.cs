using UnityEngine;
using System;

namespace Framework
{
	using Utils;

	namespace NodeGraphSystem
	{
		[Serializable]
		public class GameObjectNodeInputField : NodeInputFieldBase<GameObject>
		{
			public GameObjectRef _value;

			public static implicit operator GameObject(GameObjectNodeInputField value)
			{
				return value.GetValue();
			}

			protected override GameObject GetStaticValue()
			{
				return _value.GetGameObject();
			}

#if UNITY_EDITOR
			protected override void ClearStaticValue()
			{
				_value.ClearGameObject();
			}
#endif
		}
	}
}