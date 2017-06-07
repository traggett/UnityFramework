using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		namespace Editor
		{
			[Serializable]
			public sealed class NodeGraphEditorPrefs
			{
				public string _fileName = string.Empty;
				public float _zoom = 1.0f;
			}
		}
	}
}