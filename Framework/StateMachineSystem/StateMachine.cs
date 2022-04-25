using System;
using UnityEngine;

namespace Framework
{
	using Serialization;
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		public class StateMachine : ScriptableObject, ISerializationCallbackReceiver
		{
			#region Public Data
			public string _name;
			public StateMachineEntryState _entryState;
			public State[] _states = new State[0];

#if UNITY_EDITOR
			public StateMachineNote[] _editorNotes = new StateMachineNote[0];
#endif
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
				FixUpStates(this);
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
					if (state._stateId == stateId)
					{
						return state;
					}
				}

				return null;
			}

			public void FixUpStates(object obj)
			{
				Serializer.UpdateChildObjects(obj, FixupStateRefs, this);
			}
			#endregion

			private static object FixupStateRefs(object obj, object stateMachine)
			{
				if (obj.GetType() == typeof(StateRef))
				{
					StateRef StateRef = (StateRef)obj;
					StateRef.SetParentStatemachine((StateMachine)stateMachine);
					return StateRef;
				}

				return obj;
			}
		}
	}
}