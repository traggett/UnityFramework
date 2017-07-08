using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using TimelineSystem;
	using StateMachineSystem;
	using LocalisationSystem;
	using Utils;
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public class TimelineStateMachine : ISerializationCallbackReceiver
		{
			#region Public Data
			public string _name;
			public TimelineState[] _states = new TimelineState[0];

#if UNITY_EDITOR
			public TimelineStateMachineNote[] _editorNotes = new TimelineStateMachineNote[0];
#endif
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
				FixupStateRefs(this, this);
#if DEBUG
				foreach (TimelineState state in _states)
					state._debugParentStateMachine = this;
#endif
			}
			#endregion

			#region Public Interface
			public static TimelineStateMachine FromTextAsset(TextAsset asset, GameObject sourceObject = null)
			{
				TimelineStateMachine timelineStateMachine = Serializer.FromTextAsset<TimelineStateMachine>(asset);

				if (sourceObject != null)
					GameObjectRef.FixupGameObjectRefs(sourceObject, timelineStateMachine);

				return timelineStateMachine;
			}

			public TimelineState GetTimelineState(int timelineId)
			{
				foreach (TimelineState state in _states)
				{
					if (state._stateId == timelineId)
					{
						return state;
					}
				}

				return null;
			}
			
			public static IEnumerator Run(StateMachine stateMachine, TimelineStateRef stateRef, GameObject sourceObject = null)
			{
				TimelineState state = stateRef.GetTimelineState(sourceObject != null ? sourceObject : stateMachine.gameObject);

				if (state != null)
				{
#if UNITY_EDITOR && DEBUG
					string debugFileName = stateRef._file._filePath;
					TimelineStateMachineDebug.OnTimelineStateStarted(stateMachine, state, debugFileName);
#endif
					return PerformState(stateMachine, state, state._timeline);
				}

				return null;
			}

			public static IEnumerator Run(StateMachine stateMachine, TimelineStateRefProperty stateRef, GameObject sourceObject = null)
			{
				TimelineState state = stateRef.LoadTimelineState(sourceObject != null ? sourceObject : stateMachine.gameObject);

				if (state != null)
				{
#if UNITY_EDITOR && DEBUG
					string debugFileName = AssetDatabase.GetAssetPath(stateRef.GetFile());
					TimelineStateMachineDebug.OnTimelineStateStarted(stateMachine, state, debugFileName);
#endif
					return PerformState(stateMachine, state, state._timeline);
				}

				return null;
			}
			
			public static IEnumerator Run(StateMachine stateMachine, Timeline timeline)
			{
#if UNITY_EDITOR && DEBUG
				TimelineStateMachineDebug.OnTimelineStateStarted(stateMachine, null, null);
#endif
				return PerformState(stateMachine, null, timeline);
			}
			
			public static void FixupStateRefs(TimelineStateMachine timeLineStateMachine, object obj)
			{
				if (obj != null)
				{
					object[] nodeFieldObjects = SerializedObjectMemberInfo.GetSerializedFieldInstances(obj);

					foreach (object nodeFieldObject in nodeFieldObjects)
					{
						TimelineStateRef stateRefProperty = nodeFieldObject as TimelineStateRef;

						if (stateRefProperty != null)
						{
							stateRefProperty.FixUpRef(timeLineStateMachine);
						}
						else
						{
#if UNITY_EDITOR
							LocalisedStringRef localisedstring = nodeFieldObject as LocalisedStringRef;

							if (localisedstring != null)
							{
								localisedstring.SetEditorStateMachine(timeLineStateMachine);
							}
#endif
						}

						FixupStateRefs(timeLineStateMachine, nodeFieldObject);
					}
				}
			}

			public static ITimelineStateMachineTimer GetTimer(GameObject gameObject)
			{
				ITimelineStateMachineTimer timer = GameObjectUtils.GetComponent<ITimelineStateMachineTimer>(gameObject);

				if (timer == null)
				{
					timer = TimelineStateMachineStandardTimer.Instance;
				}

				return timer;
			}
			#endregion

			#region Private Functions
			private static IEnumerator PerformState(StateMachine stateMachine, TimelineState state, Timeline timeline)
			{
				if (timeline != null && timeline._events.Length > 0)
				{
					ITimelineStateMachineTimer timer = TimelineStateMachine.GetTimer(stateMachine.gameObject);

					float currentTime = 0.0f;
					List<Event> nonInstantEvents = new List<Event>();

					int eventIndex = 0;
					Event currentEvent = timeline._events[eventIndex];

					while (currentEvent != null || nonInstantEvents.Count > 0)
					{
						IStateMachineEvent currentStateMachineEvent = currentEvent as IStateMachineEvent;

						if (currentStateMachineEvent == null && currentEvent != null)
						{
							throw new System.Exception("Event doesn't implement IStateMachineEvent");
						}

						float nextEventTime = currentEvent != null ? timeline._events[eventIndex].GetTime() : 0.0f;

						//Wait until event time
						while (currentTime < nextEventTime || (currentEvent == null && nonInstantEvents.Count > 0))
						{
							currentTime += timer.GetDeltaTime();
#if DEBUG
							TimelineStateMachineDebug.OnTimelineStateTimeProgress(stateMachine, state, currentTime);
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
						eEventTriggerReturn status = currentStateMachineEvent.Trigger(stateMachine);

						switch (status)
						{
							case eEventTriggerReturn.EventFinished:
								//Do nothing, just move on to next event
								break;
							case eEventTriggerReturn.EventFinishedExitState:
								//Exit state so break out of coroutine
								EndNonInstantEvents(stateMachine, ref nonInstantEvents);
								yield break;
							case eEventTriggerReturn.EventOngoing:
								//Track timed event, move on to next event
								nonInstantEvents.Add(currentEvent);
								break;
						}

						//Get next
						currentEvent = ++eventIndex < timeline._events.Length ? timeline._events[eventIndex] : null;
					}
				}

#if DEBUG
				TimelineStateMachineDebug.OnTimelineStateStoped(stateMachine);
#endif

				yield break;
			}

			private static bool UpdateNonInstantEvents(StateMachine stateMachine, ref List<Event> nonInstantEvents, float currentTime)
			{
				for (int i=0; i<nonInstantEvents.Count;)
				{
					Event evnt = nonInstantEvents[i];
					IStateMachineEvent stateMachineEvent = evnt as IStateMachineEvent;

					float eventTime = currentTime - evnt.GetTime();
					bool timeUp = eventTime >= evnt.GetDuration();

					if (timeUp)
					{
						stateMachineEvent.End(stateMachine);
						nonInstantEvents.RemoveAt(i);
					}
					else
					{
						eEventTriggerReturn status = stateMachineEvent.Update(stateMachine, eventTime);

						switch (status)
						{
							case eEventTriggerReturn.EventFinished:
								nonInstantEvents.RemoveAt(i);
								break;
							case eEventTriggerReturn.EventOngoing:
								break;
							case eEventTriggerReturn.EventFinishedExitState:
								return true;
						}

						i++;
					}
				}
				
				return false;
			}

			private static void EndNonInstantEvents(StateMachine stateMachine, ref List<Event> nonInstantEvents)
			{
				foreach (IStateMachineEvent ent in nonInstantEvents)
				{
					ent.End(stateMachine);
				}

				nonInstantEvents.Clear();
			}
			#endregion
		}
	}
}