using System;

namespace Framework
{
	using StateMachineSystem;
	using Utils;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[Serializable]
			public sealed class TimelineStateMachineEditorPrefs
			{
				public string _fileName;
				public int _stateId = -1;
				public float _zoom = 1.0f;
				public bool _debug = true;
				public bool _debugLockFocus = true;
				public ComponentRef<StateMachine> _debugObject = new ComponentRef<StateMachine>();
			}
		}
	}
}