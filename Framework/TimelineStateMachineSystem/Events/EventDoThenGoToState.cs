using System.Collections;
using System;
using UnityEngine;

namespace Framework
{
	using StateMachineSystem;
	using TimelineSystem;
	using Utils;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventDoThenGoToState : Event, ITimelineStateEvent
		{
			public enum eStateType
			{
				Timeline,
				Coroutine
			}
			#region Public Data
			public eStateType _stateType = eStateType.Timeline;
			public StateRef _state;
			public CoroutineRef _coroutine = new CoroutineRef();
			public eStateType _preStateType = eStateType.Timeline;
			public StateRef _preState;
			public CoroutineRef _preCoroutine = new CoroutineRef();
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				switch (_preStateType)
				{
					case eStateType.Coroutine:
						{
							switch (_stateType)
							{
								case eStateType.Coroutine:
									{
										stateMachine.StartCoroutine(DoStateThenGoTo(stateMachine, _preCoroutine, _coroutine));
									}
									break;
								case eStateType.Timeline:
									{
										stateMachine.StartCoroutine(DoStateThenGoTo(stateMachine, _preCoroutine, _state));
									}
									break;
							}
						}
						break;
					case eStateType.Timeline:
						{
							switch (_stateType)
							{
								case eStateType.Coroutine:
									{
										stateMachine.StartCoroutine(DoStateThenGoTo(stateMachine, _preState, _coroutine));
									}
									break;
								case eStateType.Timeline:
									{
										stateMachine.StartCoroutine(DoStateThenGoTo(stateMachine, _preState, _state));
									}
									break;
							}
						}
						break;
				}

				return eEventTriggerReturn.EventFinishedExitState;
			}

			public eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachineComponent stateMachine) { }

#if UNITY_EDITOR
			public override bool EndsTimeline()
			{
				return true;
			}

			public override Color GetColor()
			{
				return new Color(217.0f / 255.0f, 80.0f / 255.0f, 58.0f / 255.0f);
			}

			public StateMachineEditorLink[] GetEditorLinks()
			{
				StateMachineEditorLink[] links = new StateMachineEditorLink[1];

				links[0] = new StateMachineEditorLink();
				links[0]._timeline = _state;

				switch (_preStateType)
				{
					case eStateType.Coroutine:
						links[0]._description = "Run " + _preCoroutine + " then";
						break;
					case eStateType.Timeline:
						links[0]._description = "Run " + _preState + " then";
						break;
				}

				return links;
			}

			public override string GetEditorDescription()
			{
				string text1 = string.Empty;
				string text2 = string.Empty;

				switch (_preStateType)
				{
					case eStateType.Timeline:
						{
							text1 = "First Run: <b>" + _preState + "</b>, ";
						}
						break;
					case eStateType.Coroutine:
						{
							text1 = "First Run: <b>" + _preCoroutine + "</b>, ";
						}
						break;
				}

				switch (_stateType)
				{
					case eStateType.Timeline:
						{
							text2 = "Then Go To: <b>" + _state + "</b>";
						}
						break;
					case eStateType.Coroutine:
						{
							text2 = "Then Go To: <b>" + _coroutine + "</b>";
						}
						break;
				}
				
				return text1 + text2;
			}
#endif
			#endregion

			private static IEnumerator DoStateThenGoTo(StateMachineComponent stateMachine, CoroutineRef coroutine, CoroutineRef goToState)
			{
				yield return stateMachine.StartCoroutine(coroutine.RunCoroutine());

				stateMachine.StartCoroutine(goToState.RunCoroutine());

				yield break;
			}

			private static IEnumerator DoStateThenGoTo(StateMachineComponent stateMachine, CoroutineRef coroutine, StateRef goToState)
			{
				yield return stateMachine.StartCoroutine(coroutine.RunCoroutine());
				
				stateMachine.GoToState(StateMachine.Run(stateMachine, goToState));

				yield break;
			}

			private static IEnumerator DoStateThenGoTo(StateMachineComponent stateMachine, StateRef TimelineState, CoroutineRef goToState)
			{
				yield return stateMachine.StartCoroutine(StateMachine.Run(stateMachine, TimelineState));

				stateMachine.StartCoroutine(goToState.RunCoroutine());

				yield break;
			}

			private static IEnumerator DoStateThenGoTo(StateMachineComponent stateMachine, StateRef TimelineState, StateRef goToState)
			{
				yield return stateMachine.StartCoroutine(StateMachine.Run(stateMachine, TimelineState));

				stateMachine.GoToState(StateMachine.Run(stateMachine, goToState));

				yield break;
			}
		}
	}
}