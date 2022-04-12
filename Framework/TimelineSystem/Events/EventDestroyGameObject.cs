using UnityEngine;
using System;

namespace Framework
{
	using StateMachineSystem;
	using Utils;

	namespace TimelineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventDestroyGameObject : Event
		{
			#region Public Data
			public GameObjectRef _gameObject;
			#endregion

			#region Event
			public override void Trigger()
			{
				GameObject gameObject = _gameObject.GetGameObject();

				if (gameObject != null)
				{
					GameObject.Destroy(gameObject);
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(0.3f, 0.6f, 0.8f);
			}

			public override string GetEditorDescription()
			{
				return "Destroy GameObject (<b>" + _gameObject + "</b>)";
			}
#endif
			#endregion
		}
	}
}
