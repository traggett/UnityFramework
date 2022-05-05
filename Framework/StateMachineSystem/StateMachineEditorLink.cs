#if UNITY_EDITOR

using System.Reflection;

namespace Framework
{
	namespace StateMachineSystem
	{
		public class StateMachineEditorLink
		{
			public string _label;
			public object _object;
			public FieldInfo _objectField;
			public int _objectFieldArrayindex;
			
			public StateMachineEditorLink(string label, object obj, FieldInfo field, int arrayIndex = -1)
			{
				_label = label;
				_object = obj;
				_objectField = field;
				_objectFieldArrayindex = arrayIndex;
			}

			public StateRef GetStateRef()
			{
				if (_objectFieldArrayindex == -1)
				{
					return (StateRef)_objectField.GetValue(_object);
				}
				else
				{
					StateRef[] stateRefs = (StateRef[])_objectField.GetValue(_object);
					return stateRefs[_objectFieldArrayindex];
				}
			}

			public void SetStateRef(StateRef value)
			{
				if (_objectFieldArrayindex == -1)
				{
					_objectField.SetValue(_object, value);
				}
				else
				{
					StateRef[] stateRefs = (StateRef[])_objectField.GetValue(_object);
					stateRefs[_objectFieldArrayindex] = value;
					_objectField.SetValue(_object, stateRefs);
				}
			}
		}
	}
}

#endif