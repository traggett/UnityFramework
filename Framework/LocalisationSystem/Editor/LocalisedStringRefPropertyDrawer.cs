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
				private static readonly float folderNameWidth = 160.0f;
				private static readonly float autoKeySlashFakeWidth = 12.0f;
				private static readonly float autoKeySlashWidth = 40.0f;
				private static readonly float autoKeybuttonWidth = 42.0f;
				private static readonly float editbuttonWidth = 50.0f;
				private static readonly float addButtonWidth = 38.0f;
				private static readonly float buttonSpace = 2.0f;
				private static readonly float fudge = 13.0f;

				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					bool hasChanges = false;

					SerializedProperty localisationkeyProperty = property.FindPropertyRelative("_localisationKey");
					string localisationkey = localisationkeyProperty.stringValue;

					float yPos = position.y;

					Rect foldoutPosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);

					property.isExpanded = !EditorGUI.Foldout(foldoutPosition, !property.isExpanded, property.displayName + " (Localised String)");
					yPos += EditorGUIUtility.singleLineHeight;

					if (!property.isExpanded)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						//Draw list of possible keys
						int currentKey = 0;
						{
							string[] keys = Localisation.GetStringKeys();
							
							for (int i=0;i<keys.Length; i++)
							{
								if (keys[i] == localisationkey)
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
									localisationkey = UpdateNewLocalisedStringRef(property, null);
									hasChanges = true;
								}
								else
								{
									localisationkey = UpdateNewLocalisedStringRef(property, keys[currentKey]);
									hasChanges = true;
								}	
							}
						}

						//Draw button for adding new key
						if (currentKey == 0)
						{
							string[] folders = Localisation.GetStringFolders();
							int currentFolderIndex = 0;
							string keyWithoutFolder;
							Localisation.GetFolderIndex(localisationkey, out currentFolderIndex, out keyWithoutFolder);

							float keyTextWidth = position.width - EditorUtils.GetLabelWidth() - (folderNameWidth + buttonSpace + autoKeybuttonWidth + buttonSpace + addButtonWidth);
							float buttonWidth = autoKeySlashFakeWidth + keyTextWidth + buttonSpace + autoKeybuttonWidth + buttonSpace + addButtonWidth;

							//Get list of current folder options
							Rect folderText = new Rect(position.x, yPos, position.width - buttonWidth, EditorGUIUtility.singleLineHeight);
							
							EditorGUI.BeginChangeCheck();
							int newFolderIndex = EditorGUI.Popup(folderText, "New Key", currentFolderIndex, folders);
							string currentFolder = newFolderIndex == 0 ? "" : folders[newFolderIndex];
							if (EditorGUI.EndChangeCheck())
							{
								if (newFolderIndex != 0)
								{
									localisationkey = UpdateNewLocalisedStringRef(property, currentFolder + "/" + keyWithoutFolder);
									hasChanges = true;
								}
							}

							Rect addKeySlash = new Rect(position.x + (position.width - buttonWidth) - fudge, yPos, autoKeySlashWidth, EditorGUIUtility.singleLineHeight);
							EditorGUI.LabelField(addKeySlash, new GUIContent("/"));

							Rect addKeyText = new Rect(position.x + (position.width - buttonWidth) - fudge + autoKeySlashFakeWidth, yPos, keyTextWidth + fudge, EditorGUIUtility.singleLineHeight);
							if (newFolderIndex != 0)
							{
                                EditorGUI.BeginChangeCheck();
                                keyWithoutFolder = EditorGUI.TextField(addKeyText, keyWithoutFolder);
                                
                                if (EditorGUI.EndChangeCheck())
								{
									localisationkey = UpdateNewLocalisedStringRef(property, currentFolder + "/" + keyWithoutFolder);
									hasChanges = true;
								}
							}
							else
							{
                                EditorGUI.BeginChangeCheck();
                                localisationkey = EditorGUI.TextField(addKeyText, localisationkey);

								if (EditorGUI.EndChangeCheck())
								{
									UpdateNewLocalisedStringRef(property, localisationkey);
									hasChanges = true;
								}
							}

							Rect autoKeyButton = new Rect(position.x + (position.width - buttonWidth) + autoKeySlashFakeWidth + buttonSpace + keyTextWidth, yPos, autoKeybuttonWidth, EditorGUIUtility.singleLineHeight);
							Rect addKeyButton = new Rect(position.x + (position.width - buttonWidth) + autoKeySlashFakeWidth + buttonSpace + keyTextWidth + buttonSpace + autoKeybuttonWidth, yPos, addButtonWidth, EditorGUIUtility.singleLineHeight);

							yPos += addKeyButton.height;

							if (GUI.Button(autoKeyButton, "Auto"))
							{
								LocalisedStringRef localisedStringRef = SerializedPropertyUtils.GetSerializedPropertyValue<LocalisedStringRef>(property);

								Component component =  property.serializedObject.targetObject as Component;
								if (component != null)
								{
									string parentName = component.gameObject.name;

									if (component.gameObject.scene.IsValid())
										parentName = component.gameObject.scene.name + "_" + parentName;

									localisedStringRef.SetAutoNameParentName(parentName);
								}

								localisationkey = UpdateNewLocalisedStringRef(property, localisedStringRef.GetAutoKey());
								hasChanges = true;
							}

							if (GUI.Button(addKeyButton, "Add") && !string.IsNullOrEmpty(localisationkey))
							{
								if (!Localisation.Exists(localisationkey))
								{
									Localisation.Set(localisationkey, Localisation.GetCurrentLanguage(), string.Empty);
									LocalisationEditorWindow.EditString(localisationkey);
									hasChanges = true;
								}
							}
						}

						//Draw displayed text (can be edited to update localization file)
						{
							//Only display if have a valid key
							if (!string.IsNullOrEmpty(localisationkey) && Localisation.Exists(localisationkey))
							{
								string text = StringUtils.GetFirstLine(Localisation.GetRawString(localisationkey, Localisation.GetCurrentLanguage()));
								float height = EditorGUIUtility.singleLineHeight;
								float labelWidth = EditorUtils.GetLabelWidth();

								Rect textPosition = new Rect(position.x + labelWidth + 2.0f, yPos, position.width - labelWidth - 2.0f - editbuttonWidth - buttonSpace, height);
								EditorGUI.LabelField(textPosition, text, EditorUtils.ReadonlyTextBoxStyle);
								Rect editTextPosition = new Rect(textPosition.x + textPosition.width + buttonSpace, yPos, editbuttonWidth, EditorGUIUtility.singleLineHeight);

								if (GUI.Button(editTextPosition, "Edit"))
								{
									LocalisationEditorWindow.EditString(localisationkey);
								}

								yPos += height;

							}
						}

						EditorGUI.indentLevel = origIndent;
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
					if (!property.isExpanded)
					{
                        return EditorGUIUtility.singleLineHeight * 3;
                    }

					return EditorGUIUtility.singleLineHeight;
				}

				private string UpdateNewLocalisedStringRef(SerializedProperty property, string key)
				{
					SerializedPropertyUtils.SetSerializedPropertyValue(property, new LocalisedStringRef(key));
					return key;
				}
			}
		}
	}
}