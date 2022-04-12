using UnityEngine;
using System;

namespace Framework
{
	using Utils;

	namespace TimelineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventRunMethod : Event
		{
			#region Public Data
			public ComponentVoidMethodRef _method = new ComponentVoidMethodRef();
			#endregion

			#region Event
			public override void Trigger()
			{
				_method.RunMethod();
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(219.0f / 255.0f, 64.0f / 255.0f, 11.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return _method;
			}
#endif
			#endregion
		}
	}
}
