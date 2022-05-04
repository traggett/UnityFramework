#if DEBUG
using UnityEngine;

using System.Collections.Generic;

namespace Framework
{
	using System.Collections;
	using Utils;

	namespace StateMachineSystem
	{
		public static class StateMachineDebug
		{ 
			public sealed class StateInfo
			{
				public StateMachine _stateMachine;
				public State _state;
			}

			private static Dictionary<GameObject, StateInfo> _stateMachineMap = new Dictionary<GameObject, StateInfo>();
			private static GameObject _lastSelectedObject;

			public static StateInfo GetStateInfo(GameObject obj)
			{
				if (obj == null)
				{
					obj = _lastSelectedObject;
				}
				else
				{
					_lastSelectedObject = obj;
				}

				if (obj != null && _stateMachineMap.TryGetValue(obj, out StateInfo stateInfo))
				{
					return stateInfo;
				}

				return null;
			}

			public static void OnEnterState(StateMachineComponent stateMachine, State state)
			{
				StateInfo stateInfo;

				if (!_stateMachineMap.TryGetValue(stateMachine.gameObject, out stateInfo))
				{
					stateInfo = new StateInfo();
					_stateMachineMap.Add(stateMachine.gameObject, stateInfo);
				}

				stateInfo._stateMachine = state._debugParentStateMachine;
				stateInfo._state = state;
			}

			public static void OnEnterCoroutineState(StateMachineComponent stateMachine, IEnumerator coroutineState)
			{
				StateInfo stateInfo;

				if (!_stateMachineMap.TryGetValue(stateMachine.gameObject, out stateInfo))
				{
					stateInfo = new StateInfo();
					_stateMachineMap.Add(stateMachine.gameObject, stateInfo);
				}

				stateInfo._stateMachine = null;
				stateInfo._state = null;
			}

			public static void OnStopped(StateMachineComponent stateMachine)
			{
				StateInfo stateInfo;

				if (!_stateMachineMap.TryGetValue(stateMachine.gameObject, out stateInfo))
				{
					stateInfo = new StateInfo();
					_stateMachineMap.Add(stateMachine.gameObject, stateInfo);
				}

				stateInfo._stateMachine = null;
				stateInfo._state = null;
			}
		}
	}
}
#endif