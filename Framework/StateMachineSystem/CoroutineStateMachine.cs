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

			// Multiple processes can be running as one coroutine can be aborting whilst we start the next
			private const int Max_Processes = 8;
			private readonly ProcessData[] _processes = new ProcessData[Max_Processes];

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
					}

					_currentIndex = (_currentIndex + 1) % _processes.Length;

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
				if (_isRunning && _process == null)
				{
					_isRunning = false;
				}

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
					// Run throught the current states iterator
					while (!_processes[index]._abort 
						&& _processes[index]._current != null
						&& _processes[index]._current.MoveNext())
					{
						yield return _processes[index]._current.Current;
					}

					// Check should move on to next state if any
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
							break;
						}
					}
				}

				_processes[index]._current = null;

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