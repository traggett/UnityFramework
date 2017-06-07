using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		public class StateMachine : MonoBehaviour
		{
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

			private static List<IStateMachineMsg> _messages = new List<IStateMachineMsg>();
			private static List<IStateMachineMsg> _messagesToAdd = new List<IStateMachineMsg>();
			#endregion

			#region Public Interface
			public void GoToState(IEnumerator state)
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

#if DEBUG
				_debugIsGoingToBranchingState = false;
#endif
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

			#region Messaging
			public static void TriggerMessage(IStateMachineMsg msg)
			{
				_messagesToAdd.Add(msg);
			}

			public static List<IStateMachineMsg> GetMessages()
			{
				return _messages;
			}

			public static void UseMessage(IStateMachineMsg msg)
			{
				_messages.Remove(msg);
			}

			public static void ClearMessages()
			{
				_messages.Clear();
				_messages.AddRange(_messagesToAdd);
				_messagesToAdd.Clear();
			}
			#endregion

			#region Branching
			public void GoToBranchingState(params IBranch[] branches)
			{
				GoToState(RunBranchingState(branches));

#if DEBUG
				_debugIsGoingToBranchingState = true;
#endif
			}
			#endregion

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

			private IEnumerator RunBranchingState(params IBranch[] branches)
			{
				foreach (IBranch branch in branches)
				{
					branch.OnBranchingStarted(this);
				}

				bool branchTaken = false;

				while (!branchTaken)
				{
					foreach (IBranch branch in branches)
					{
						if (branch.ShouldBranch(this))
						{
							IEnumerator goToState = branch.GetGoToState(this);
							if (goToState != null)
							{
								GoToState(goToState);
							}

							branchTaken = true;
							break;
						}
					}

					yield return null;
				}

				foreach (IBranch branch in branches)
				{
					branch.OnBranchingFinished(this);
				}
			}
			#endregion

#if DEBUG
			private bool _debugIsGoingToBranchingState;

			public bool IsGoingToBranchingState()
			{
				return _debugIsGoingToBranchingState;
			}
#endif
		}
	}
}