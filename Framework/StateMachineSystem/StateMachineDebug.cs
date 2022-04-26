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

			public static void OnStateStarted(StateMachineComponent stateMachine, State state)
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
					}
				}
			}
		}
	}
}
#endif