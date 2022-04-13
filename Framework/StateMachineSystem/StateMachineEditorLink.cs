#if UNITY_EDITOR

namespace Framework
{
	using Serialization;
	
	namespace StateMachineSystem
	{
		public struct StateMachineEditorLink
		{
			public object _object;
			public SerializedObjectMemberInfo _memberInfo;
			public string _description;

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
		}
	}
}

#endif