using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(LocalisedStringSourceAsset), true)]
			public class LocalisedStringSourceAssetInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					LocalisedStringSourceAsset sourceAsset = (LocalisedStringSourceAsset)target;
					string text = sourceAsset.GetText(Localisation.GetCurrentLanguage());

					//Show preview text and edit buttonRect textPosition = new Rect(position.x + labelWidth, yPos, position.width - labelWidth - _editbuttonWidth - _buttonSpace, height);
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.TextArea(text);
					EditorGUI.EndDisabledGroup();

					if (GUILayout.Button("Edit"))
					{
						LocalisationEditorWindow.EditString(sourceAsset);
					}
				}
			}
		}
	}
}