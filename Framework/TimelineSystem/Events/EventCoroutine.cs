using System;
using UnityEngine;

namespace Framework
{
	using Utils;

	namespace TimelineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventCoroutine : Event
		{
			#region Public Data
			public CoroutineRef _coroutine = new CoroutineRef();
			#endregion

			#region Event
			public override void Trigger()
			{
				_coroutine.RunCoroutine();
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(219.0f / 255.0f, 64.0f / 255.0f, 11.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return "StartCoroutine: <b>'" + _coroutine + "'</b>";
			}
#endif
			#endregion
		}
	}
}
