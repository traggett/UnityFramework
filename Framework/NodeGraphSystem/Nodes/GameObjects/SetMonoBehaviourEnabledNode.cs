using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("GameObjects")]
		[Serializable]
		public class SetMonoBehaviourEnabledNode : Node
		{
			#region Public Data
			public ComponentNodeInputField<MonoBehaviour> _component = new ComponentNodeInputField<MonoBehaviour>();
			public NodeInputField<bool> _active = false;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				MonoBehaviour component = _component;
				if (component != null)
					component.enabled = _active;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return GameObjectNodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}