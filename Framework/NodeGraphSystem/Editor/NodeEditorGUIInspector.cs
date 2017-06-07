using UnityEditor;

namespace Framework
{
	using Serialization;

	namespace NodeGraphSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(NodeEditorGUI), true)]
			public sealed class NodeEditorGUIInspector : SerializedObjectEditorGUIInspector<Node>
			{
			}
		}
	}
}