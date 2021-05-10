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
				private static readonly float editbuttonWidth = 50.0f;
				private static readonly float buttonSpace = 2.0f;
				
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

						for (int i = 0; i < keys.Length; i++)
						{
							if (keys[i] == localisationkey)
							{
								currentKey = i;
								break;
							}
						}

						Rect typePosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);
						yPos += EditorGUIUtility.singleLineHeight;

						EditorUtils.SetBoldDefaultFont(localisationkeyProperty.prefabOverride);

						EditorGUI.BeginChangeCheck();
						currentKey = EditorGUI.Popup(typePosition, property.displayName + " (Localised String)", currentKey, keys);
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

						Rect textPosition = new Rect(position.x + labelWidth + 2.0f, yPos, position.width - labelWidth - 2.0f - editbuttonWidth - buttonSpace, height);
						EditorGUI.LabelField(textPosition, text, EditorUtils.ReadOnlyTextBoxStyle);
						Rect editTextPosition = new Rect(textPosition.x + textPosition.width + buttonSpace, yPos, editbuttonWidth, EditorGUIUtility.singleLineHeight);

						if (GUI.Button(editTextPosition, "Edit"))
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