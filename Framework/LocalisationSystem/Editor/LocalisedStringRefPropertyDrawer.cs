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
			[CustomPropertyDrawer(typeof(LocalisedStringRef))]
			public class LocalisedStringRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					SerializedProperty localisationkeyProperty = property.FindPropertyRelative("_localisationKey");

					float yPos = position.y;

					Rect foldoutPosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);

					property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, property.displayName + " (Localised String)");
					yPos += EditorGUIUtility.singleLineHeight;

					if (property.isExpanded)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

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

							Rect typePosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);
							yPos += EditorGUIUtility.singleLineHeight;

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
							float autoKeybuttonWidth = 42.0f;
							float addButtonWidth = 38.0f;
							float buttonSpace = 2.0f;
							float buttonWidth = autoKeybuttonWidth + buttonSpace + addButtonWidth; 

							Rect addKeyText = new Rect(position.x, yPos, position.width - buttonWidth, EditorGUIUtility.singleLineHeight);
							localisationkeyProperty.stringValue = EditorGUI.DelayedTextField(addKeyText, "New Key", localisationkeyProperty.stringValue);

							Rect autoKeyButton = new Rect(position.x + (position.width - buttonWidth), yPos, autoKeybuttonWidth, EditorGUIUtility.singleLineHeight);
							Rect addKeyButton = new Rect(position.x + (position.width - buttonWidth) + buttonSpace + autoKeybuttonWidth, yPos, addButtonWidth, EditorGUIUtility.singleLineHeight);

							yPos += addKeyButton.height;

							if (GUI.Button(autoKeyButton, "Auto"))
							{
								LocalisedStringRef localisedStringRef = (LocalisedStringRef)EditorUtils.GetTargetObjectOfProperty(property);

								Component component =  property.serializedObject.targetObject as Component;
								if (component != null)
								{
									localisedStringRef.SetAutoNameParentName(component.gameObject.scene.name + "_" + component.gameObject.name);
								}

								localisationkeyProperty.stringValue = localisedStringRef.GetAutoKey();
							}

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
							if (!string.IsNullOrEmpty(localisationkeyProperty.stringValue) && Localisation.IsKeyInTable(localisationkeyProperty.stringValue))
							{
								string text = Localisation.GetString(localisationkeyProperty.stringValue);
								int numLines = StringUtils.GetNumberOfLines(text);
								float height = (EditorGUIUtility.singleLineHeight - 2.0f) * numLines + 4.0f;
								float labelWidth = EditorUtils.GetLabelWidth();

								Rect textPosition = new Rect(position.x + labelWidth, yPos, position.width - labelWidth, height);
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
						
						float height = EditorGUIUtility.singleLineHeight * 3;

						if (Localisation.IsKeyInTable(localisationkeyProperty.stringValue))
						{
							string text = Localisation.GetString(localisationkeyProperty.stringValue);
							int numLines = StringUtils.GetNumberOfLines(text);
							height += (EditorGUIUtility.singleLineHeight - 2.0f) * numLines + 4.0f;
						}						

						return height;
					}

					return EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}