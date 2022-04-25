using System;
using UnityEngine;

namespace Framework
{
	using Utils;
	
	namespace StateMachineSystem
	{
		[Serializable]
		public struct StateRef
		{
			#region Serialized Data
			[SerializeField]
			private int _stateId;
			#endregion

			#region Private Data
			private StateMachine _parentStateMachine;
			private State _state;
			#endregion

			#region Public Interface
			public void SetParentStatemachine(StateMachine stateMachine)
			{
				_parentStateMachine = stateMachine;
				_state = null;
			}

			public bool IsValid()
			{
				return _stateId != -1;
			}

			public State GetState()
			{
				if (_state == null)
				{
					if (_parentStateMachine != null)
					{
						_state = _parentStateMachine.GetState(_stateId);
					}
					else
					{
						throw new Exception("StateRefProperty need to be fixed up by a StateMachine before allowing access.");
					}
				}

				return _state;
			}

#if UNITY_EDITOR
			public StateRef(int stateId)
			{
				_stateId = stateId;
				_parentStateMachine = null;
				_state = null;
			}

			public int GetStateID()
			{
				return _stateId;
			}
#endif
			#endregion

			#region Private Functions
#if UNITY_EDITOR
			public string GetDescription(State state)
			{
				if (state._editorAutoDescription)
					return StringUtils.GetFirstLine(state.GetEditorDescription());

				return StringUtils.GetFirstLine(state._editorDescription);
			}
#endif
			#endregion
		}
	}
}