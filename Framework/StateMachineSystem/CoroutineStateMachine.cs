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
			private struct ProcessData
			{
				public IEnumerator _current;
				public IEnumerator _next;
				public bool _abort;
			}
			private readonly ProcessData[] _processes = new ProcessData[2];
			private int _currentIndex = 0;
			#endregion

			#region Unity Messages
			protected virtual void OnDisable()
			{
				Stop();
			}
			#endregion

			#region Public Interface
			public void GoToState(Coroutine state)
			{
				GoToState(state);
			}

			public void GoToState(IEnumerator state)
			{
				if (state != null)
				{
					if (_process != null)
					{
						_processes[_currentIndex]._abort = true;
						StopCoroutine(_process);
					}

					_currentIndex = _currentIndex == 0 ? 1 : 0;

					_processes[_currentIndex]._current = state;
					_processes[_currentIndex]._next = null;
					_processes[_currentIndex]._abort = false;

					OnEnterState(state);

					_isRunning = true;
					
					_process = StartCoroutine(Run(_currentIndex));
				}
				else
				{
					Stop();
				}
			}

			public void SetNextState(Coroutine state)
			{
				SetNextState(state);
			}

			public void SetNextState(IEnumerator state)
			{
				if (IsRunning())
				{
					_processes[_currentIndex]._next = state;
				}
				else
				{
					GoToState(state);
				}
			}

			public IEnumerator GetNextState()
			{
				return _processes[_currentIndex]._next;
			}

			public void ClearNextState()
			{
				if (IsRunning())
				{
					_processes[_currentIndex]._next = null;
				}
			}

			public void Stop()
			{
				if (_process != null)
				{
					_processes[_currentIndex]._abort = true;
					StopCoroutine(_process);
					_process = null;
				}

				_isRunning = false;

				OnStopped();
			}

			public bool IsRunning()
			{
				return _isRunning;
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
			private IEnumerator Run(int index)
			{
				while (!_processes[index]._abort)
				{
					while (!_processes[index]._abort 
						&& _processes[index]._current != null
						&& _processes[index]._current.MoveNext())
					{
						if (_processes[index]._abort)
						{
							break;
						}

						yield return _processes[index]._current.Current;
					}

					if (!_processes[index]._abort)
					{
						if (_processes[index]._next != null)
						{
							_processes[index]._current = _processes[index]._next;
							_processes[index]._next = null;

							OnEnterState(_processes[index]._current);
						}
						else
						{
							_processes[index]._current = null;
							break;
						}
					}
				}

				if (!_processes[index]._abort)
				{
					_isRunning = false;
					OnStopped();
				}

				yield break;
			}
			#endregion
		}
	}
}