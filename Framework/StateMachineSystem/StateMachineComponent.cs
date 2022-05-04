using System.Collections;
using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		public class StateMachineComponent : MonoBehaviour
		{
			#region Private Data
			private enum RunState
			{
				NotRunning,
				Running,
				Paused,
			}
			private RunState _runState = RunState.NotRunning;
			private Coroutine _process;
			private IEnumerator _current;
			private IEnumerator _next;
			#endregion

			#region Unity Messages
			protected virtual void OnDisable()
			{
				Stop();
			}
			#endregion

			#region Public Interface
			public void RunStateMachine(StateMachine stateMachine)
			{
				GoToState(stateMachine._entryState._initialState);
			}

			public void GoToState(StateRef stateRef)
			{
				GoToState(stateRef.GetState());
			}

			public void GoToState(State state)
			{
				if (state != null)
				{
					GoToState(state.PerformState(this));

#if UNITY_EDITOR && DEBUG
					StateMachineDebug.OnEnterState(this, state);
#endif
				}
				else
				{
					Stop();
				}
			}

			public void GoToState(IEnumerator state)
			{
				Stop();

				_current = state;
				_process = StartCoroutine(Run());

#if UNITY_EDITOR && DEBUG
				StateMachineDebug.OnEnterCoroutineState(this, state);
#endif
			}

			public void SetNextState(IEnumerator state)
			{
				if (_runState == RunState.NotRunning)
				{
					GoToState(state);
				}
				else if (_runState == RunState.Running)
				{
					_next = state;
				}
			}

			public void Stop()
			{
				if (_process != null)
				{
					StopCoroutine(_process);
					_process = null;
				}

				_current = null;
				_next = null;
				_runState = RunState.NotRunning;

#if UNITY_EDITOR && DEBUG
				StateMachineDebug.OnStopped(this);
#endif
			}

			public void Pause()
			{
				_runState = RunState.Paused;
			}

			public void Resume()
			{
				_runState = RunState.Running;
			}

			public bool IsRunning()
			{
				return _runState == RunState.Running;
			}
			#endregion

			#region Protected Functions
			protected IEnumerator GetNext()
			{
				return _next;
			}
			#endregion

			#region Private Functions
			private IEnumerator Run()
			{
				_runState = RunState.Running;

				while (true)
				{
					while (_current != null && _current.MoveNext())
					{
						yield return _current.Current;

						while (_runState == RunState.Paused)
						{
							yield return null;
						}
					}

					if (_next != null)
					{
						_current = _next;
						_next = null;

#if UNITY_EDITOR && DEBUG
						StateMachineDebug.OnEnterCoroutineState(this, _current);
#endif
					}
					else
					{
						_current = null;
						break;
					}
				}

				_runState = RunState.NotRunning;

#if UNITY_EDITOR && DEBUG
				StateMachineDebug.OnStopped(this);
#endif
			}
			#endregion
		}
	}
}