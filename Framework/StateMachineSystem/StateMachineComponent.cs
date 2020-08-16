using System.Collections;

using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		public class StateMachineComponent : MonoBehaviour
		{
			#region Public Data
			public StateRefProperty _initialState;
			#endregion

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
			#endregion

			#region MonoBehaviour
			void Start()
			{
				if (_initialState.IsValid())
					GoToState(StateMachine.Run(this, _initialState));
			}

			private void OnDisable()
			{
				_state = State.NotRunning;
			}
			#endregion

			#region Public Interface
			public void GoToState(IEnumerator state)
			{
				_current = state;
				_next = null;

				if (_state == State.NotRunning)
				{
					StartCoroutine(Run());
				}
			}

			public void SetNextState(IEnumerator state)
			{
				if (_state == State.NotRunning)
				{
					_current = state;
					StartCoroutine(Run());
				}
				else if (_state == State.Running)
				{
					_next = state;
				}
			}

			public void Stop()
			{
				StopAllCoroutines();
				_state = State.NotRunning;
			}

			public void Pause()
			{
				if (_state != State.Running)
				{
					throw new System.InvalidOperationException("Unable to pause coroutine in state: " + _state);
				}

				_state = State.Paused;
			}

			public void Resume()
			{
				if (_state != State.Paused)
				{
					throw new System.InvalidOperationException("Unable to resume coroutine in state: " + _state);
				}

				_state = State.Running;
			}

			public bool IsRunning()
			{
				return _state == State.Running;
			}
			#endregion

			#region Private Functions
			private IEnumerator Run()
			{
				if (_state != State.NotRunning)
				{
					throw new System.InvalidOperationException("Unable to start coroutine in state: " + _state);
				}

				while (_current != null)
				{
					_state = State.Running;
					while (_current.MoveNext())
					{
						yield return _current.Current;

						while (_state == State.Paused)
						{
							yield return null;
						}

						if (_state == State.NotRunning)
						{
							yield break;
						}
					}

					_current = _next;
					_next = null;
				}

				_state = State.NotRunning;
			}
			#endregion
		}
	}
}