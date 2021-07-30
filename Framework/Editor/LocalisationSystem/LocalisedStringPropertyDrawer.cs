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

					SerializedProperty localisationkeyProperty = property.FindPropertyRelative("_localisationKey");
					string localisationkey = localisationkeyProperty.stringValue;

					float yPos = position.y;

					//Draw list of possible keys
					int currentKey = 0;
					{
						string[] keys = Localisation.GetStringKeys();
						GUIContent[] keyLabels = new GUIContent[keys.Length];

						for (int i = 0; i < keys.Length; i++)
						{
							keyLabels[i] = new GUIContent(keys[i]);

							if (keys[i] == localisationkey)
							{
								currentKey = i;
							}
						}

						Rect typePosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);
						yPos += EditorGUIUtility.singleLineHeight;

						EditorUtils.SetBoldDefaultFont(localisationkeyProperty.prefabOverride);

						EditorGUI.BeginChangeCheck();
						currentKey = EditorGUI.Popup(typePosition, label, currentKey, keyLabels);
						if (EditorGUI.EndChangeCheck())
						{
							if (currentKey == 0)
							{
								localisationkey = UpdateNewLocalisedStringRef(property, null);
								hasChanges = true;
							}
							else
							{
								localisationkey = UpdateNewLocalisedStringRef(property, keys[currentKey]);
								hasChanges = true;
							}
						}

						EditorUtils.SetBoldDefaultFont(false);
					}

					//Draw text preview (can be edited to update localization file)
					if (!string.IsNullOrEmpty(localisationkey) && Localisation.Exists(localisationkey) && !localisationkeyProperty.hasMultipleDifferentValues)
					{
						string text = StringUtils.GetFirstLine(Localisation.GetRawString(localisationkey, Localisation.GetCurrentLanguage()));
						float height = EditorGUIUtility.singleLineHeight;
						float labelWidth = EditorUtils.GetLabelWidth();

						Rect textPosition = new Rect(position.x + labelWidth, yPos, position.width - labelWidth - _editbuttonWidth - _buttonSpace, height);
						EditorGUI.LabelField(textPosition, text, EditorUtils.ReadOnlyTextBoxStyle);
						Rect editTextPosition = new Rect(textPosition.x + textPosition.width + _buttonSpace, yPos, _editbuttonWidth, EditorGUIUtility.singleLineHeight);

						if (GUI.Button(editTextPosition, _editLabel))
						{
							LocalisationEditorWindow.EditString(localisationkey);
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
					SerializedProperty localisationkeyProperty = property.FindPropertyRelative("_localisationKey");

					if (string.IsNullOrEmpty(localisationkeyProperty.stringValue))
					{
						return EditorGUIUtility.singleLineHeight;
					}
					else
					{
						return EditorGUIUtility.singleLineHeight * 2;
					}
				}

				private string UpdateNewLocalisedStringRef(SerializedProperty property, string key)
				{
					LocalisedString localisedString = key;
					SerializedPropertyUtils.SetSerializedPropertyValue(property, localisedString);
					return key;
				}
			}
		}
	}
}