using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{ 
		namespace Timelines
		{
			public class TimelineStateStandardTimer : ITimelineStateTimer
			{
				#region Singleton Stuff
				private static TimelineStateStandardTimer _instance = null;

				public static TimelineStateStandardTimer Instance
				{
					get
					{
						if (_instance == null)
						{
							_instance = new TimelineStateStandardTimer();
						}

						return _instance;
					}
				}
				#endregion

				#region ITimelineStateMachineTimer
				public float GetDeltaTime()
				{
					return Time.deltaTime;
				}
				#endregion
			}
		}
	}
}