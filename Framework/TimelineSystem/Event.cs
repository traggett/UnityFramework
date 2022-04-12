using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace TimelineSystem
	{
		public abstract class Event : IComparable<Event>
		{
			#region Public Data
			[SerializeField, HideInInspector]
			private float _time = 0.0f;

			public float Time
			{
				get
				{
					return _time;
				}

#if UNITY_EDITOR
				set
				{
					_time = value;
				}
#endif
			}
			#endregion

			#region Public Interface
#if UNITY_EDITOR
			public static float RenderTimeField(GUIContent label, float time, out bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				time = EditorGUILayout.FloatField(label, time);
				dataChanged = EditorGUI.EndChangeCheck();
				return time;
			}

			public static float RenderDaysHoursMinsField(GUIContent label, float time, out bool dataChanged)
			{
				dataChanged = false;

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(label);

					TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, (int)(time * 1000.0f));

					int days = EditorGUILayout.IntField(timeSpan.Days, GUILayout.Width(24));
					EditorGUILayout.LabelField("day(s)", GUILayout.Width(42));
					int hours = EditorGUILayout.IntField(timeSpan.Hours, GUILayout.Width(24));
					EditorGUILayout.LabelField("h", GUILayout.Width(16));
					int minutes = EditorGUILayout.IntField(timeSpan.Minutes, GUILayout.Width(24));
					EditorGUILayout.LabelField("m", GUILayout.Width(18));
					int seconds = EditorGUILayout.IntField(timeSpan.Seconds, GUILayout.Width(24));

					timeSpan = new TimeSpan(days, hours, minutes, seconds, timeSpan.Milliseconds);

					float newTime = (float)timeSpan.TotalSeconds;

					if (time != newTime)
					{
						time = newTime;
						dataChanged = true;
					}
				}
				EditorGUILayout.EndHorizontal();

				return time;
			}
#endif
			#endregion

			#region Virtual Interface
			public abstract void Trigger();
			
			public virtual void Update(float eventTime) { }
			
			public virtual void End() { }

			public virtual float GetDuration()
			{
				return 0.0f;
			}
			
			protected virtual int CompareSameEventType(Event ent)
			{
				return -1;
			}

#if UNITY_EDITOR			
			public abstract Color GetEditorColor();
			public abstract string GetEditorDescription();
			public virtual bool GetEditorShouldBeLastEventInTimeline() { return false; }
			public virtual string GetEditorShortDescription() { return GetEditorDescription(); }
#endif
			#endregion

			#region IComparable
			public int CompareTo(Event evnt)
			{
				if (evnt == null)
					return 1;

				if (evnt == this)
					return 0;

				int compare = Time.CompareTo(evnt.Time);

				//If time is the same then compare by type
				if (compare == 0)
				{
#if UNITY_EDITOR
					//Always order exit state events at the end
					if (!GetEditorShouldBeLastEventInTimeline() && evnt.GetEditorShouldBeLastEventInTimeline())
						return -1;

					if (GetEditorShouldBeLastEventInTimeline() && !evnt.GetEditorShouldBeLastEventInTimeline())
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