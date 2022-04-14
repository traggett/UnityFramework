#if UNITY_EDITOR

namespace Framework
{
	using Serialization;
	using System.Reflection;

	namespace StateMachineSystem
	{
		public struct StateMachineEditorLink
		{
			public object _object;
			public FieldInfo _fieldInfo;
			public int _arrayIndex;
			public string _description;

			public StateRef GetStateRef()
			{
				if (_arrayIndex == -1)
				{
					return (StateRef)_fieldInfo.GetValue(_object);
				}
				else
				{
					StateRef[] stateRefs = (StateRef[])_fieldInfo.GetValue(_object);
					return stateRefs[_arrayIndex];
				}
			}

			public void SetStateRef(StateRef value)
			{
				if (_arrayIndex == -1)
				{
					_fieldInfo.SetValue(_object, value);
				}
				else
				{
					StateRef[] stateRefs = (StateRef[])_fieldInfo.GetValue(_object);
					stateRefs[_arrayIndex] = value;
					_fieldInfo.SetValue(_object, stateRefs);
				}
			}
		}
	}
}

#endif