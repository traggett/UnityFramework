using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(LocalisedStringTableAsset), true)]
			public class LocalisedStringTableAssetInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					if (GUILayout.Button("Edit"))
					{
						LocalisedStringTableAsset sourceAsset = (LocalisedStringTableAsset)target;
						LocalisationEditorWindow.Load(sourceAsset);
					}

					//Show summary? Num entries, langauges?
				}
			}
		}
	}
}