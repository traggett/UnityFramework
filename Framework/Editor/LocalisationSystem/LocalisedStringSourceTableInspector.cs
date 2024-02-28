using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(LocalisedStringSourceTable), true)]
			public class LocalisedStringSourceTableInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					if (GUILayout.Button("Edit"))
					{
						LocalisedStringSourceTable sourceAsset = (LocalisedStringSourceTable)target;
						LocalisationEditorWindow.Load(sourceAsset);
					}

					//Show summary? Num entries, langauges?
				}
			}
		}
	}
}