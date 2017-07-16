using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils;

	namespace LocalisationSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(LocalisedStringRef))]
			public class LocalisedStringRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					SerializedProperty localisationkeyProperty = property.FindPropertyRelative("_localisationKey");

					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

					property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, property.displayName + " (Localised String)");
					
					if (property.isExpanded)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						float yPos = position.y;

						//Draw list of possible keys
						int currentKey = 0;
						{
							string[] keys = Localisation.GetStringKeys();
							
							for (int i=0;i<keys.Length; i++)
							{
								if (keys[i] == localisationkeyProperty.stringValue)
								{
									currentKey = i;
									break;
								}
							}

							Rect typePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
							yPos += typePosition.height;

							EditorGUI.BeginChangeCheck();
							currentKey = EditorGUI.Popup(typePosition, "Localisation Key", currentKey, keys);
							
							if (EditorGUI.EndChangeCheck())
							{
								if (currentKey == 0)
								{
									localisationkeyProperty.stringValue = null;
								}
								else
								{
									localisationkeyProperty.stringValue = keys[currentKey];
								}	
							}
						}

						//Draw button for adding new key
						if (currentKey == 0)
						{
							float buttonWidth = 48.0f;

							Rect addKeyText = new Rect(position.x, yPos, position.width - buttonWidth, EditorGUIUtility.singleLineHeight);
							localisationkeyProperty.stringValue = EditorGUI.DelayedTextField(addKeyText, "New Key", localisationkeyProperty.stringValue);
							yPos += addKeyText.height;

							Rect addKeyButton = new Rect(position.x + (position.width - buttonWidth), yPos, buttonWidth, EditorGUIUtility.singleLineHeight);
							yPos += addKeyButton.height;

							if (GUI.Button(addKeyButton, "Add") && !string.IsNullOrEmpty(localisationkeyProperty.stringValue))
							{
								if (!Localisation.IsKeyInTable(localisationkeyProperty.stringValue))
								{
									Localisation.UpdateString(localisationkeyProperty.stringValue, Localisation.GetCurrentLanguage(), string.Empty);
								}
							}
						}

						//Draw displayed text (can be edited to update localization file)
						{
							//Only display if have a valid key
							if (!string.IsNullOrEmpty(localisationkeyProperty.stringValue))
							{
								string text = Localisation.GetString(localisationkeyProperty.stringValue);
								int numLines = StringUtils.GetNumberOfLines(text);
								float height = (EditorGUIUtility.singleLineHeight - 2.0f) * numLines + 4.0f;

								Rect textPosition = new Rect(position.x, yPos, position.width, height);
								yPos += height;

								EditorGUI.BeginChangeCheck();
								text = EditorGUI.TextArea(textPosition, text);
								if (EditorGUI.EndChangeCheck())
								{
									Localisation.UpdateString(localisationkeyProperty.stringValue, Localisation.GetCurrentLanguage(), text);
								}
							}
						}

						EditorGUI.indentLevel = origIndent;
					}
					
					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					if (property.isExpanded)
					{
						SerializedProperty localisationkeyProperty = property.FindPropertyRelative("_localisationKey");
						
						float height = EditorGUIUtility.singleLineHeight * 2;

						int currentKey = 0;
						string[] keys = Localisation.GetStringKeys();

						for (int i = 0; i < keys.Length; i++)
						{
							if (keys[i] == localisationkeyProperty.stringValue)
							{
								currentKey = i;
								break;
							}
						}

						if (currentKey == 0)
						{
							height += EditorGUIUtility.singleLineHeight * 2;
						}

						string text = Localisation.GetString(localisationkeyProperty.stringValue);
						int numLines = StringUtils.GetNumberOfLines(text);
						height += (EditorGUIUtility.singleLineHeight - 2.0f) * numLines + 4.0f;

						return height;
					}

					return EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}