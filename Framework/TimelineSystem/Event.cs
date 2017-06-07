using System;
using UnityEngine;

namespace Framework
{
	namespace TimelineSystem
	{
		public abstract class Event : IComparable<Event>
		{
			#region Public Data
			public float _time = 0.0f;
			#endregion

			#region Public Interface
			public float GetTime()
			{
				return _time;
			}

			public void SetTime(float time)
			{
				_time = time;
			}
			#endregion

			#region Virtual Interface		
			public virtual float GetDuration()
			{
				return 0.0f;
			}
			
			protected virtual int CompareSameEventType(Event ent)
			{
				return -1;
			}

#if UNITY_EDITOR
			public virtual bool EndsTimeline() { return false; }
			public abstract Color GetColor();
			public abstract string GetEditorDescription();
#endif
			#endregion

			#region IComparable
			public int CompareTo(Event evnt)
			{
				if (evnt == null)
					return 1;

				if (evnt == this)
					return 0;

				int compare = GetTime().CompareTo(evnt.GetTime());

				//If time is the same then compare by type
				if (compare == 0)
				{
#if UNITY_EDITOR
					//Always order exit state events at the end
					if (!EndsTimeline() && evnt.EndsTimeline())
						return -1;

					if (EndsTimeline() && !evnt.EndsTimeline())
						return 1;
#endif

					compare = GetType().FullName.CompareTo(evnt.GetType().FullName);

					//If type is the same then use custom compare
					if (compare == 0)
					{
#if UNITY_EDITOR
						compare = GetEditorDescription().CompareTo(evnt.GetEditorDescription());

						if (compare == 0)
#endif
						{
							compare = CompareSameEventType(evnt);
						}
					}
				}

				return compare;
			}
			#endregion
		}
	}
}