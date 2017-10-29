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
						//show drop downs for where to put the key - enum list of all current folders + empty.
						//select one to preapend 
						if (currentKey == 0)
						{
							float folderNameWidth = 160.0f;
							float autoKeySlashFakeWidth = 12.0f;
							float autoKeySlashWidth = 40.0f;
							float autoKeybuttonWidth = 42.0f;
							float addButtonWidth = 38.0f;
							float buttonSpace = 2.0f;
							float fudge = 13.0f;
							float keyTextWidth = position.width - EditorUtils.GetLabelWidth() - (folderNameWidth + buttonSpace + autoKeybuttonWidth + buttonSpace + addButtonWidth);
							float buttonWidth = autoKeySlashFakeWidth + keyTextWidth + buttonSpace + autoKeybuttonWidth + buttonSpace + addButtonWidth;


							//Get list of current folder options
							Rect folderText = new Rect(position.x, yPos, position.width - buttonWidth, EditorGUIUtility.singleLineHeight);

							string[] folders = Localisation.GetStringFolders();
							int currentFolderIndex = 0;
							string currentFolder;
							for (int i = 0; i < folders.Length; i++)
							{
								//To do - if the first bit of our folder exisist then thats the current folder eg Roles/NewRole/Name - Roles/
								if (folders[i] == Localisation.GetFolderName(localisationkeyProperty.stringValue))
								{
									currentFolderIndex = i;
									break;
								}
							}
							EditorGUI.BeginChangeCheck();
							int newFolderIndex = EditorGUI.Popup(folderText, "New Key", currentFolderIndex, folders);
							currentFolder = newFolderIndex == 0 ? "" : folders[newFolderIndex];
							if (EditorGUI.EndChangeCheck())
							{
								if (newFolderIndex != 0)
									localisationkeyProperty.stringValue = currentFolder + "/" + Localisation.GetKeyWithoutFoldder(localisationkeyProperty.stringValue);
								else if (currentFolderIndex != 0)
									localisationkeyProperty.stringValue = Localisation.GetKeyWithoutFoldder(localisationkeyProperty.stringValue);
							}

							Rect addKeySlash = new Rect(position.x + (position.width - buttonWidth) - fudge, yPos, autoKeySlashWidth, EditorGUIUtility.singleLineHeight);
							EditorGUI.LabelField(addKeySlash, new GUIContent("/"));

							Rect addKeyText = new Rect(position.x + (position.width - buttonWidth) - fudge + autoKeySlashFakeWidth, yPos, keyTextWidth + fudge, EditorGUIUtility.singleLineHeight);
							if (newFolderIndex != 0)
							{
								string newAddKey = EditorGUI.TextField(addKeyText, Localisation.GetKeyWithoutFoldder(localisationkeyProperty.stringValue));
								localisationkeyProperty.stringValue = currentFolder + "/" + newAddKey;
							}
							else
							{
								string newAddKey = EditorGUI.TextField(addKeyText, localisationkeyProperty.stringValue);
								localisationkeyProperty.stringValue = newAddKey;
							}

							Rect autoKeyButton = new Rect(position.x + (position.width - buttonWidth) + autoKeySlashFakeWidth + buttonSpace + keyTextWidth, yPos, autoKeybuttonWidth, EditorGUIUtility.singleLineHeight);
							Rect addKeyButton = new Rect(position.x + (position.width - buttonWidth) + autoKeySlashFakeWidth + buttonSpace + keyTextWidth + buttonSpace + autoKeybuttonWidth, yPos, addButtonWidth, EditorGUIUtility.singleLineHeight);

							yPos += addKeyButton.height;

							if (GUI.Button(autoKeyButton, "Auto"))
							{
								LocalisedStringRef localisedStringRef = (LocalisedStringRef)EditorUtils.GetTargetObjectOfProperty(property);

								Component component =  property.serializedObject.targetObject as Component;
								if (component != null)
								{
									string parentName = component.gameObject.name;

									if (component.gameObject.scene.IsValid())
										parentName = component.gameObject.scene.name + "_" + parentName;

									localisedStringRef.SetAutoNameParentName(parentName);
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