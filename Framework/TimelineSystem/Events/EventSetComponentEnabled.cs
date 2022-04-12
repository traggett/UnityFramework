using UnityEngine;
using System;

namespace Framework
{
	using Utils;

	namespace TimelineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventSetComponentEnabled : Event
		{
			#region Public Data
			public ComponentRef<MonoBehaviour> _target;
			public bool _enabled = false;
			#endregion

			#region Event
			public override void Trigger()
			{
				MonoBehaviour target = _target.GetComponent();

				if (target != null)
				{
					target.enabled = _enabled;
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(0.3f, 0.6f, 0.8f);
			}

			public override string GetEditorDescription()
			{
				return (_enabled ? "Enable" : "Disable") + " Component (<b>"+_target+"</b>)";
			}
#endif
			#endregion
		}
	}
}
