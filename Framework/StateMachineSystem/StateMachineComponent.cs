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
			private enum eState
			{
				NotRunning,
				Running,
				Paused,
			}
			private eState _state = eState.NotRunning;
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
				_state = eState.NotRunning;
			}
			#endregion

			#region Public Interface
			public void GoToState(IEnumerator state)
			{
				_current = state;

				if (_state == eState.NotRunning)
				{
					StartCoroutine(Run());
				}
			}

			public void SetNextState(IEnumerator state)
			{
				if (_state == eState.NotRunning)
				{
					_current = state;
					StartCoroutine(Run());
				}
				else if (_state == eState.Running)
				{
					_next = state;
				}
			}

			public void Stop()
			{
				StopAllCoroutines();
				_state = eState.NotRunning;
			}

			public void Pause()
			{
				if (_state != eState.Running)
				{
					throw new System.InvalidOperationException("Unable to pause coroutine in state: " + _state);
				}

				_state = eState.Paused;
			}

			public void Resume()
			{
				if (_state != eState.Paused)
				{
					throw new System.InvalidOperationException("Unable to resume coroutine in state: " + _state);
				}

				_state = eState.Running;
			}

			public bool IsRunning()
			{
				return _state == eState.Running;
			}
			#endregion

			#region Private Functions
			private IEnumerator Run()
			{
				if (_state != eState.NotRunning)
				{
					throw new System.InvalidOperationException("Unable to start coroutine in state: " + _state);
				}

				while (_current != null)
				{
					_state = eState.Running;
					while (_current.MoveNext())
					{
						yield return _current.Current;

						while (_state == eState.Paused)
						{
							yield return null;
						}

						if (_state == eState.NotRunning)
						{
							yield break;
						}
					}

					_current = _next;
					_next = null;
				}

				_state = eState.NotRunning;
			}
			#endregion
		}
	}
}