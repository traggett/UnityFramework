using System;
using UnityEngine;

namespace Framework
{
	using Serialization;
	
	namespace StateMachineSystem
	{
		[Serializable]
		public class StateMachine : ScriptableObject, ISerializationCallbackReceiver
		{
			#region Public Data
			[HideInInspector]
			public StateMachineEntryState _entryState;
			[HideInInspector]
			public State[] _states = new State[0];

#if UNITY_EDITOR
			[HideInInspector]
			public StateMachineNote[] _editorNotes = new StateMachineNote[0];
#endif
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
#if DEBUG
				foreach (State state in _states)
				{
					if (state != null)
						state._debugParentStateMachine = this;
				}			
#endif
			}
			#endregion

			#region Public Interface
			public State GetState(int stateId)
			{
				foreach (State state in _states)
				{
					if (state != null && state._stateId == stateId)
					{
						return state;
					}
				}

				return null;
			}
			#endregion
		}
	}
}