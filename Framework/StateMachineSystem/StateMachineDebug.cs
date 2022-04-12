#if DEBUG
using UnityEngine;

using System.Collections.Generic;

namespace Framework
{
	using Utils;

	namespace StateMachineSystem
	{
		public static class StateMachineDebug
		{ 
			public sealed class StateInfo
			{
				public StateMachine _stateMachine;
				public State _state;
				public string _fileName;
				public float _time;
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

				StateInfo stateInfo = null;

				if (obj != null && _stateMachineMap.TryGetValue(obj, out stateInfo))
				{
					return stateInfo;
				}

				return null;
			}

			public static void OnStateStarted(StateMachineComponent stateMachine, State state, string fileName)
			{
				StateMachine parentStateMachine = state._debugParentStateMachine;

				if (parentStateMachine != null)
				{
					if (parentStateMachine != null)
					{
						StateInfo stateInfo;

						if (!_stateMachineMap.TryGetValue(stateMachine.gameObject, out stateInfo))
						{
							stateInfo = new StateInfo();
							_stateMachineMap.Add(stateMachine.gameObject, stateInfo);
						}

						stateInfo._stateMachine = parentStateMachine;
						stateInfo._state = state;
						stateInfo._time = 0.0f;
						if (!string.IsNullOrEmpty(fileName))
							stateInfo._fileName = fileName;
					}
				}
			}

			public static void OnTimelineStateTimeProgress(StateMachineComponent stateMachine, State intialState, float time)
			{
				StateInfo stateInfo;

				if (_stateMachineMap.TryGetValue(stateMachine.gameObject, out stateInfo))
				{
					_stateMachineMap[stateMachine.gameObject]._time = time;
				}
			}

			public static void OnTimelineStateStoped(StateMachineComponent stateMachine)
			{
				_stateMachineMap.Remove(stateMachine.gameObject);
			}
		}
	}
}
#endif