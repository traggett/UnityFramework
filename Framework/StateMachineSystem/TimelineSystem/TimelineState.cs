using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{
	using Utils;
	using TimelineSystem;

	namespace StateMachineSystem
	{
		namespace Timelines
		{
			[Serializable]
			public class TimelineState : State
			{
				#region Public Data			
				[HideInInspector]
				public Timeline _timeline = new Timeline();
				public StateRef _goToState;
				#endregion

				#region Public Interface
#if UNITY_EDITOR
				public override StateMachineEditorLink[] GetEditorLinks()
				{
					StateMachineEditorLink[] links = new StateMachineEditorLink[1];
					links[0] = new StateMachineEditorLink(this, "goToState", "Go To");
					return links;
				}

				public override string GetAutoDescription()
				{
					string label = null;

					foreach (Event evnt in _timeline._events)
					{
						string eventDescription = evnt.GetEditorShortDescription();

						if (!string.IsNullOrEmpty(eventDescription))
						{
							if (label == null)
							{
								label = eventDescription;
							}
							else
							{
								label += '\n' + eventDescription;
							}
						}
					}

					return label;
				}

				public override string GetStateIdLabel()
				{
					return "Timeline (State" + _stateId.ToString("00") + ")";
				}
#endif

				public override IEnumerator PerformState(StateMachineComponent stateMachine)
				{
					if (_timeline != null && _timeline._events.Length > 0)
					{
						ITimelineStateTimer timer = GetTimer(stateMachine.gameObject);

						float currentTime = 0.0f;
						List<Event> nonInstantEvents = new List<Event>();

						int eventIndex = 0;
						Event currentEvent = _timeline._events[eventIndex];

						while (currentEvent != null || nonInstantEvents.Count > 0)
						{
							float nextEventTime = currentEvent != null ? _timeline._events[eventIndex].Time : 0.0f;

							//Wait until event time
							while (currentTime < nextEventTime || (currentEvent == null && nonInstantEvents.Count > 0))
							{
								currentTime += timer.GetDeltaTime();
#if DEBUG
								StateMachineDebug.OnTimelineStateTimeProgress(stateMachine, this, currentTime);
#endif

								//Updated non instant events, if any now wants to exit the state then break out of coroutine
								if (UpdateNonInstantEvents(stateMachine, ref nonInstantEvents, currentTime))
								{
									EndNonInstantEvents(stateMachine, ref nonInstantEvents);
									yield break;
								}

								yield return null;
							}

							if (currentEvent == null)
								break;

							//Trigger event
							currentEvent.Trigger();

							if (currentEvent.GetDuration() > 0f)
							{
								//Track timed event, move on to next event
								nonInstantEvents.Add(currentEvent);
							}

							//Get next
							currentEvent = ++eventIndex < _timeline._events.Length ? _timeline._events[eventIndex] : null;
						}
					}

#if DEBUG
					StateMachineDebug.OnTimelineStateStoped(stateMachine);
#endif

					stateMachine.GoToState(StateMachine.Run(stateMachine, _goToState));

					yield break;
				}
				#endregion

				#region Private Functions
				private static bool UpdateNonInstantEvents(StateMachineComponent stateMachine, ref List<Event> nonInstantEvents, float currentTime)
				{
					for (int i = 0; i < nonInstantEvents.Count;)
					{
						Event evnt = nonInstantEvents[i];

						float eventTime = currentTime - evnt.Time;
						bool timeUp = eventTime >= evnt.GetDuration();

						if (timeUp)
						{
							evnt.End();
							nonInstantEvents.RemoveAt(i);
						}
						else
						{
							evnt.Update(eventTime);
							i++;
						}
					}

					return false;
				}

				private static void EndNonInstantEvents(StateMachineComponent stateMachine, ref List<Event> nonInstantEvents)
				{
					foreach (Event evnt in nonInstantEvents)
					{
						evnt.End();
					}

					nonInstantEvents.Clear();
				}

				public static ITimelineStateTimer GetTimer(GameObject gameObject)
				{
					ITimelineStateTimer timer = GameObjectUtils.GetComponent<ITimelineStateTimer>(gameObject);

					if (timer == null)
					{
						timer = TimelineStateStandardTimer.Instance;
					}

					return timer;
				}
				#endregion
			}
		}
	}
}