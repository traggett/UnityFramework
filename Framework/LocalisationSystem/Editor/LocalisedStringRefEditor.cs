using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Serialization;
	using Utils.Editor;

	namespace LocalisationSystem
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(LocalisedStringRef), "PropertyField")]
			public static class LocalisedStringRefEditor
			{
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
				{
					LocalisedStringRef localisedString = (LocalisedStringRef)obj;
					
					bool editorCollapsed = !EditorGUILayout.Foldout(!localisedString._editorCollapsed, label);

					if (editorCollapsed != localisedString._editorCollapsed)
					{
						localisedString._editorCollapsed = editorCollapsed;
						dataChanged = true;
					}

					if (!editorCollapsed)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;
						
						//Draw list of possible keys
						int currentKeyIndex = 0;
						{
							string[] keys = Localisation.GetStringKeys();
							string currentKey = localisedString.GetLocalisationKey();

							for (int i = 0; i < keys.Length; i++)
							{
								if (keys[i] == currentKey)
								{
									currentKeyIndex = i;
									break;
								}
							}

							EditorGUI.BeginChangeCheck();
							currentKeyIndex = EditorGUILayout.Popup("Localisation Key", currentKeyIndex, keys);

							//If key has changed
							if (EditorGUI.EndChangeCheck())
							{
								if (currentKeyIndex == 0)
								{
									localisedString = new LocalisedStringRef();
								}
								else
								{
									localisedString = new LocalisedStringRef(keys[currentKeyIndex]);
								}

								dataChanged = true;
							}
						}

						//Draw buttons for adding new key
						if (currentKeyIndex == 0)
						{
							string[] folders = Localisation.GetStringFolders();
							int currentFolderIndex = 0;
							for (int i = 0; i < folders.Length; i++)
							{
								//To do - if the first bit of our folder exists then thats the current folder eg Roles/NewRole/Name - Roles/
								if (folders[i] == Localisation.GetFolderName(localisedString.GetLocalisationKey()))
								{
									currentFolderIndex = i;
									break;
								}
							}

							EditorGUILayout.BeginHorizontal();
							{
								EditorGUILayout.LabelField(new GUIContent("New Key"), GUILayout.Width(EditorUtils.GetLabelWidth()));

								string editorParentName = localisedString.GetAutoNameParentName();

								EditorGUI.BeginChangeCheck();
								int newFolderIndex = EditorGUILayout.Popup(currentFolderIndex, folders);
								string currentFolder = newFolderIndex == 0 ? "" : folders[newFolderIndex];
								if (EditorGUI.EndChangeCheck())
								{
									if (newFolderIndex != 0)
									{
										localisedString = new LocalisedStringRef(currentFolder + "/" + Localisation.GetKeyWithoutFolder(localisedString.GetLocalisationKey()));
										localisedString.SetAutoNameParentName(editorParentName);
										dataChanged = true;
									}
									else if (currentFolderIndex != 0)
									{
										localisedString = new LocalisedStringRef(Localisation.GetKeyWithoutFolder(localisedString.GetLocalisationKey()));
										localisedString.SetAutoNameParentName(editorParentName);
										dataChanged = true;
									}
								}

								EditorGUILayout.LabelField(new GUIContent("/"), GUILayout.Width(44));

								EditorGUI.BeginChangeCheck();
								string newKey = EditorGUILayout.TextField(newFolderIndex != 0 ? Localisation.GetKeyWithoutFolder(localisedString.GetLocalisationKey()) : localisedString.GetLocalisationKey());
								if (EditorGUI.EndChangeCheck())
								{
									localisedString = new LocalisedStringRef(newFolderIndex != 0 ? currentFolder + "/" + newKey : newKey);
									localisedString.SetAutoNameParentName(editorParentName);
									dataChanged = true;
								}							

								if (GUILayout.Button("Auto", GUILayout.Width(36)))
								{
									newKey = localisedString.GetAutoKey();
									localisedString = new LocalisedStringRef(newKey);
									localisedString.SetAutoNameParentName(editorParentName);
									dataChanged = true;
								}

								if (GUILayout.Button("Add", GUILayout.Width(32)) && !string.IsNullOrEmpty(localisedString.GetLocalisationKey()))
								{
									if (!Localisation.IsKeyInTable(localisedString.GetLocalisationKey()))
									{
										Localisation.UpdateString(localisedString.GetLocalisationKey(), Localisation.GetCurrentLanguage(), string.Empty);
										LocalisationEditorWindow.EditString(localisedString.GetLocalisationKey());
									}
									dataChanged = true;
								}
							}
							EditorGUILayout.EndHorizontal();
						}

						//Draw actual localised text (can be edited to update localisation file)
						{
							string currentKey = localisedString.GetLocalisationKey();

							//Only display if have a valid key
							if (!string.IsNullOrEmpty(currentKey) && Localisation.IsKeyInTable(currentKey))
							{
								EditorGUI.BeginChangeCheck();
								string text;
								if (style != null)
									text = EditorGUILayout.TextArea(Localisation.GetUnformattedString(currentKey), style);
								else
									text = EditorGUILayout.TextArea(Localisation.GetUnformattedString(currentKey));
								if (EditorGUI.EndChangeCheck())
								{
									Localisation.UpdateString(currentKey, Localisation.GetCurrentLanguage(), text);
								}
							}
						}

						EditorGUI.indentLevel = origIndent;
					}

					return localisedString;
				}
				#endregion
			}
		}
	}
}