using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Serialization;
	using LocalisationSystem;
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		public class StateMachine : ISerializationCallbackReceiver
		{
			#region Public Data
			public string _name;
			public State[] _states = new State[0];

#if UNITY_EDITOR
			public StateMachineNote[] _editorNotes = new StateMachineNote[0];
#endif
			#endregion

			#region Prviate Data
			private static List<IStateMachineMsg> _messages = new List<IStateMachineMsg>();
			private static List<IStateMachineMsg> _messagesToAdd = new List<IStateMachineMsg>();
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
				FixUpStates(this);
#if DEBUG
				foreach (State state in _states)
					state._debugParentStateMachine = this;
#endif
			}
			#endregion

			#region Public Interface
			public static StateMachine FromTextAsset(TextAsset asset, GameObject sourceObject = null)
			{
				StateMachine timelineStateMachine = Serializer.FromTextAsset<StateMachine>(asset);

				if (sourceObject != null)
					GameObjectRef.FixUpGameObjectRefsInObject(timelineStateMachine, sourceObject);

				return timelineStateMachine;
			}

			public State GetState(int timelineId)
			{
				foreach (State state in _states)
				{
					if (state._stateId == timelineId)
					{
						return state;
					}
				}

				return null;
			}

			public static IEnumerator Run(StateMachineComponent stateMachine, StateRef startState, GameObject sourceObject = null)
			{
				State state = startState.GetState(sourceObject != null ? sourceObject : stateMachine.gameObject);

				if (state != null)
				{
#if UNITY_EDITOR && DEBUG
					string debugFileName = startState.GetExternalFile().GetFilePath();
					StateMachineDebug.OnStateStarted(stateMachine, state, debugFileName);
#endif
					return state.PerformState(stateMachine);
				}

				return null;
			}

			public static IEnumerator Run(StateMachineComponent stateMachine, StateRefProperty startState, GameObject sourceObject = null)
			{
				State state = startState.LoadTimelineState(sourceObject != null ? sourceObject : stateMachine.gameObject);

				if (state != null)
				{
#if UNITY_EDITOR && DEBUG
					string debugFileName = AssetDatabase.GetAssetPath(startState.GetFile());
					StateMachineDebug.OnStateStarted(stateMachine, state, debugFileName);
#endif
					return state.PerformState(stateMachine);
				}

				return null;
			}

			public static IEnumerator Run(StateMachineComponent stateMachine, State startState, GameObject sourceObject = null)
			{
				if (startState != null)
				{
#if UNITY_EDITOR && DEBUG
					StateMachineDebug.OnStateStarted(stateMachine, startState, null);
#endif
					return startState.PerformState(stateMachine);
				}

				return null;
			}

			public static void TriggerMessage(IStateMachineMsg msg)
			{
				_messagesToAdd.Add(msg);
			}

			public static List<IStateMachineMsg> GetMessages()
			{
				return _messages;
			}

			public static void UseMessage(IStateMachineMsg msg)
			{
				_messages.Remove(msg);
			}

			public static void ClearMessages()
			{
				_messages.Clear();
				_messages.AddRange(_messagesToAdd);
				_messagesToAdd.Clear();
			}

			public void FixUpStates(object obj)
			{
				Serializer.UpdateChildObjects(obj, FixupStateRefs, this);
			}
			#endregion

			private static object FixupStateRefs(object obj, object stateMachine)
			{
				if (obj.GetType() == typeof(StateRef))
				{
					StateRef StateRef = (StateRef)obj;
					StateRef.SetParentStatemachine((StateMachine)stateMachine);
					return StateRef;
				}

				return obj;
			}
		}
	}
}