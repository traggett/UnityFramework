using System;

namespace Framework
{
	namespace TimelineSystem
	{
		[Serializable]
		public class Timeline
		{
			public Event[] _events;
			
			public Timeline()
			{
				_events = new Event[0];
			}

			public Timeline(Event[] events)
			{
				_events = events;
			}
		}
	}
}