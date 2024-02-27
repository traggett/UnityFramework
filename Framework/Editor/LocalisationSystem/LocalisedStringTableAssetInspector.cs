using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(LocalisedStringSourceAssetCollection), true)]
			public class LocalisedStringTableAssetInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					if (GUILayout.Button("Edit"))
					{
						LocalisedStringSourceAssetCollection sourceAsset = (LocalisedStringSourceAssetCollection)target;
						LocalisationEditorWindow.Load(sourceAsset);
					}

					//Show summary? Num entries, langauges?
				}
			}
		}
	}
}