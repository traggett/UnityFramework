using UnityEngine;
using System;

namespace Framework
{
	using Utils;

	namespace TimelineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventSetActive : Event
		{
			#region Public Data
			public GameObjectRef _target;
			public bool _active = false;
			#endregion

			#region Event
			public override void Trigger()
			{
				GameObject target = _target.GetGameObject();

				if (target != null)
				{
					target.SetActive(_active);
				}

			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(0.3f, 0.6f, 0.8f);
			}

			public override string GetEditorDescription()
			{
				return (_active ? "Activate" : "Deactivate") + " GameObject (<b>"+_target+"</b>)";
			}
#endif
			#endregion
		}
	}
}
