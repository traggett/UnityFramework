#if DEBUG

using System.Collections.Generic;

namespace Framework
{
	namespace StateMachineSystem
	{
		public static class StateMachineDebug
		{ 
			public sealed class StateInfo
			{
				public StateMachine _stateMachine;
				public State _state;
			}

			private static Dictionary<StateMachineComponent, StateInfo> _stateMachineMap = new Dictionary<StateMachineComponent, StateInfo>();
			private static StateMachineComponent _lastSelectedObject;

			public static StateInfo GetStateInfo(StateMachineComponent obj)
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
				if (!_stateMachineMap.TryGetValue(stateMachine, out StateInfo stateInfo))
				{
					stateInfo = new StateInfo();
					_stateMachineMap.Add(stateMachine, stateInfo);
				}

				stateInfo._stateMachine = state.Parent;
				stateInfo._state = state;
			}
			
			public static void Clear(StateMachineComponent stateMachine)
			{
				_stateMachineMap.Remove(stateMachine);
			}
		}
	}
}
#endif