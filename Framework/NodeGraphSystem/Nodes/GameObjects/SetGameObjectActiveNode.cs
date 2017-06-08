using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("GameObjects")]
		[Serializable]
		public class SetGameObjectActiveNode : Node
		{
			#region Public Data
			public GameObjectNodeInputField _gameObject = new GameObjectNodeInputField();
			public NodeInputField<bool> _active = false;
			#endregion

			#region Public Data
			private float _origIntensity;
			#endregion

			#region Node
			public override void Update(float deltaTime)
			{
				GameObject gameObject = _gameObject;
				gameObject.SetActive(_active);
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