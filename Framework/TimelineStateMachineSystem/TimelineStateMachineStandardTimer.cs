using UnityEngine;

namespace Framework
{
	namespace TimelineStateMachineSystem
	{
		public class TimelineStateMachineStandardTimer : ITimelineStateMachineTimer
		{ 
			#region Singleton Stuff
			private static TimelineStateMachineStandardTimer _instance = null;

			public static TimelineStateMachineStandardTimer Instance
			{
				get
				{
					if (_instance == null)
					{
						_instance = new TimelineStateMachineStandardTimer();
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