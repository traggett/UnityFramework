using UnityEngine;
using UnityEngine.Playables;
using System;

namespace Framework
{
	using Utils;

	namespace TimelineSystem
	{
		[Serializable]
		[EventCategory("Playables")]
		public class EventStopPlayableDirector : Event
		{
			#region Public Data
			public ComponentRef<PlayableDirector> _director;
			#endregion

			#region Event
			public override void Trigger()
			{
				PlayableDirector director = _director.GetComponent();

				if (director != null)
				{
					director.Stop();
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(0.859f, 0.439f, 0.576f);
			}

			public override string GetEditorDescription()
			{
				return "Stop (<b>"+ _director + "</b>)";
			}
#endif
			#endregion
		}
	}
}
