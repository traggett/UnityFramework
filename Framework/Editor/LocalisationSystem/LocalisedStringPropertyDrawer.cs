using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils.Editor;
	using Utils;

	namespace LocalisationSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(LocalisedString))]
			public class LocalisedStringPropertyDrawer : PropertyDrawer
			{
				private static readonly float _editbuttonWidth = 50.0f;
				private static readonly float _buttonSpace = 2.0f;
				private static readonly GUIContent _editLabel = new GUIContent("Edit");
				
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					bool hasChanges = false;

					SerializedProperty localisationGUIDProperty = property.FindPropertyRelative("_localisationGUID");
					string guid = localisationGUIDProperty.stringValue;
					LocalisedStringSourceAsset sourceAsset;

					float yPos = position.y;

					//Draw source asset field
					{
						Rect typePosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);
						yPos += EditorGUIUtility.singleLineHeight;

						var path = AssetDatabase.GUIDToAssetPath(guid);
						sourceAsset = AssetDatabase.LoadAssetAtPath(path, typeof(LocalisedStringSourceAsset)) as LocalisedStringSourceAsset;

						sourceAsset = EditorGUI.ObjectField(typePosition, label.text, sourceAsset, typeof(LocalisedStringSourceAsset), false) as LocalisedStringSourceAsset;

						localisationGUIDProperty.stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sourceAsset));
					}

					//Draw text preview (can be edited to update localization file)
					if (sourceAsset != null && !localisationGUIDProperty.hasMultipleDifferentValues)
					{
						string text = StringUtils.GetFirstLine(sourceAsset.GetText(Localisation.GetCurrentLanguage()));
						float height = EditorGUIUtility.singleLineHeight;
						float labelWidth = EditorUtils.GetLabelWidth();

						Rect textPosition = new Rect(position.x + labelWidth, yPos, position.width - labelWidth - _editbuttonWidth - _buttonSpace, height);
						EditorGUI.LabelField(textPosition, text, EditorUtils.ReadOnlyTextBoxStyle);
						Rect editTextPosition = new Rect(textPosition.x + textPosition.width + _buttonSpace, yPos, _editbuttonWidth, EditorGUIUtility.singleLineHeight);

						if (GUI.Button(editTextPosition, _editLabel))
						{
							LocalisationEditorWindow.EditString(sourceAsset);
						}
					}

					EditorGUI.EndProperty();

					if (hasChanges)
					{
						property.serializedObject.ApplyModifiedProperties();
						property.serializedObject.Update();
					}
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					SerializedProperty localisationGUIDProperty = property.FindPropertyRelative("_localisationGUID");

					if (string.IsNullOrEmpty(localisationGUIDProperty.stringValue))
					{
						return EditorGUIUtility.singleLineHeight;
					}
					else
					{
						return EditorGUIUtility.singleLineHeight * 2;
					}
				}
			}
		}
	}
}