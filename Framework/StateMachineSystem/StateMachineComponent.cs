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
			private bool _forceExit;
			#endregion

			#region Unity Messages
			protected virtual void OnDisable()
			{
				Stop();
			}
			#endregion

			#region Public Interface
			public void RunStateMachine(StateMachine stateMachine, GameObject sourceObject = null)
			{
				GoToState(stateMachine._entryState._initialState, sourceObject);
			}

			public void GoToState(StateRef stateRef, GameObject sourceObject = null)
			{
				State state = stateRef.GetState(sourceObject);

				if (state != null)
				{
#if UNITY_EDITOR && DEBUG
					string debugFileName = stateRef.GetExternalFile().GetFilePath();
					StateMachineDebug.OnStateStarted(this, state, debugFileName);
#endif
					GoToState(state.PerformState(this));
				}
				else
				{
					Stop();
				}
			}

			public void GoToState(IEnumerator state)
			{
				if (_runState == RunState.NotRunning)
				{
					_current = state;
					_process = StartCoroutine(Run());
				}
				else
				{
					_next = state;
					_forceExit = true;
				}
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
					_forceExit = false;
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
				_forceExit = false;
				_runState = RunState.NotRunning;
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

						if (_forceExit)
						{
							break;
						}

						while (_runState == RunState.Paused)
						{
							yield return null;
						}
					}

					_forceExit = false;

					if (_next != null)
					{
						_current = _next;
						_next = null;
					}
					else
					{
						_current = null;
						break;
					}
				}

				_runState = RunState.NotRunning;
			}
			#endregion
		}
	}
}