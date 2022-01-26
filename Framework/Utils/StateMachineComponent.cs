using System.Collections;

using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public class StateMachineComponent : MonoBehaviour
		{
			#region Private Data
			private enum State
			{
				NotRunning,
				Running,
				Paused,
			}
			private State _state = State.NotRunning;
			private IEnumerator _current;
			private IEnumerator _next;
			private Coroutine _process;
			#endregion

			#region Unity Messages
			protected virtual void OnDisable()
			{
				Stop();
			}
			#endregion

			#region Public Interface
			public void GoToState(IEnumerator state)
			{
				Stop();

				_current = state;
				_process = StartCoroutine(Run());
			}

			public void SetNextState(IEnumerator state)
			{
				if (_state == State.NotRunning)
				{
					GoToState(state);
				}
				else if (_state == State.Running)
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
				_state = State.NotRunning;
			}

			public void Pause()
			{
				_state = State.Paused;
			}

			public void Resume()
			{
				_state = State.Running;
			}

			public bool IsRunning()
			{
				return _state == State.Running;
			}
			#endregion

			protected IEnumerator GetNext()
			{
				return _next;
			}

			#region Private Functions
			private IEnumerator Run()
			{
				_state = State.Running;

				while (true)
				{
					while (_current != null && _current.MoveNext())
					{
						yield return _current.Current;

						while (_state == State.Paused)
						{
							yield return null;
						}
					}

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

				_state = State.NotRunning;
			}
			#endregion
		}
	}
}