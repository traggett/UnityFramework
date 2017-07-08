using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("GameObjects")]
		[Serializable]
		public class ComponentGetGameObjectNode : Node, IValueSource<GameObject>
		{
			#region Public Data
			public ComponentNodeInputField<Component> _component = new ComponentNodeInputField<Component>();
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return GameObjectNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<GameObject>
			public GameObject GetValue()
			{
				Component component = _component;

				if (component != null)
				{
					return component.gameObject;
				}

				return null;
			}
			#endregion
		}
	}
}