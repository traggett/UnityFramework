using UnityEditor;

namespace Framework
{
	using Serialization;

	namespace StateMachineSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(StateMachine), true)]
			public sealed class StateMachineInspector : UnityEditor.Editor
			{
			}
		}
	}
}