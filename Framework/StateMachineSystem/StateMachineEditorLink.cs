#if UNITY_EDITOR

namespace Framework
{
	using System.Reflection;

	namespace StateMachineSystem
	{
		public struct StateMachineEditorLink
		{
			public State _state;
			public FieldInfo _fieldInfo;
			public int _arrayIndex;
			public string _description;

			public StateRef GetStateRef()
			{
				if (_arrayIndex == -1)
				{
					return (StateRef)_fieldInfo.GetValue(_state);
				}
				else
				{
					StateRef[] stateRefs = (StateRef[])_fieldInfo.GetValue(_state);
					return stateRefs[_arrayIndex];
				}
			}

			public void SetStateRef(StateRef value)
			{
				if (_arrayIndex == -1)
				{
					_fieldInfo.SetValue(_state, value);
				}
				else
				{
					StateRef[] stateRefs = (StateRef[])_fieldInfo.GetValue(_state);
					stateRefs[_arrayIndex] = value;
					_fieldInfo.SetValue(_state, stateRefs);
				}
			}
		}
	}
}

#endif