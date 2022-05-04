using System.Collections;
using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		public class CoroutineStateMachine : MonoBehaviour
		{
			#region Private Data
			private bool _isRunning;
			private Coroutine _process;
			private IEnumerator _current;
			private IEnumerator _next;
			private readonly bool[] _abort = new bool[2];
			private int _runIndex = 0;
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
				if (state != null)
				{
					if (_process != null)
					{
						StopCoroutine(_process);
						_abort[_runIndex] = true;
					}

					_current = state;
					_next = null;

					OnEnterState(state);

					_runIndex = _runIndex == 0 ? 1 : 0;
					_isRunning = true;
					
					_process = StartCoroutine(Run());
				}
				else
				{
					Stop();
				}
			}

			public void SetNextState(IEnumerator state)
			{
				if (IsRunning())
				{
					_next = state;
				}
				else
				{
					GoToState(state);
				}
			}

			public void Stop()
			{
				if (_process != null)
				{
					StopCoroutine(_process);
					_abort[_runIndex] = true;
					_process = null;
				}

				_current = null;
				_next = null;
				_isRunning = false;

				OnStopped();
			}

			public bool IsRunning()
			{
				return _isRunning;
			}

			public IEnumerator GetNextState()
			{
				return _next;
			}
			#endregion

			#region Virtual Interface
			protected virtual void OnEnterState(IEnumerator state)
			{

			}

			protected virtual void OnStopped()
			{

			}
			#endregion

			#region Private Functions
			private IEnumerator Run()
			{
				int runIndex = _runIndex;
				_abort[runIndex] = false;

				while (true)
				{
					while (_current != null && _current.MoveNext())
					{
						if (_abort[runIndex])
						{
							yield break;
						}

						yield return _current.Current;
					}

					if (_abort[runIndex])
					{
						yield break;
					}

					if (_next != null)
					{
						_current = _next;
						_next = null;

						OnEnterState(_current);
					}
					else
					{
						_current = null;
						break;
					}
				}

				_isRunning = false;
				OnStopped();
			}
			#endregion
		}
	}
}