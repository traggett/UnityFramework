#if UNITY_EDITOR

using System;

namespace Framework
{
	using Serialization;
	
	namespace StateMachineSystem
	{
		public struct StateMachineEditorLink
		{
			private object _object;
			private SerializedObjectMemberInfo _memberInfo;
			private string _description;

			public StateMachineEditorLink(object obj, string propertyName, string description)
			{
				_object = obj;
				SerializedObjectMemberInfo.FindSerializedField(obj.GetType(), propertyName, out _memberInfo);
				_description = description;
			}

			public StateRef GetStateRef()
			{
				if (_object != null)
					return (StateRef)_memberInfo.GetValue(_object);

				return new StateRef();
			}

			public void SetStateRef(StateRef value)
			{
				_memberInfo.SetValue(_object, value);
			}

			public string GetDescription()
			{
				return _description;
			}
		}
	}
}

#endif