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
							Localisation.GetFolderIndex(localisedString.GetLocalisationKey(), out int currentFolderIndex, out string keyWithoutFolder);

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
										localisedString = new LocalisedStringRef(currentFolder + "/" + keyWithoutFolder);
										localisedString.SetAutoNameParentName(editorParentName);
										dataChanged = true;
									}
								}

								EditorGUILayout.LabelField(new GUIContent("/"), GUILayout.Width(44));

								EditorGUI.BeginChangeCheck();
								keyWithoutFolder = EditorGUILayout.TextField(keyWithoutFolder);
								if (EditorGUI.EndChangeCheck())
								{
									localisedString = new LocalisedStringRef(currentFolder + "/" + keyWithoutFolder);
									localisedString.SetAutoNameParentName(editorParentName);
									dataChanged = true;
								}							

								if (GUILayout.Button("Auto", GUILayout.Width(36)))
								{
									string newKey = localisedString.GetAutoKey();
									localisedString = new LocalisedStringRef(newKey);
									localisedString.SetAutoNameParentName(editorParentName);
									dataChanged = true;
								}

								if (GUILayout.Button("Add", GUILayout.Width(32)) && !string.IsNullOrEmpty(localisedString.GetLocalisationKey()))
								{
									if (!Localisation.Exists(localisedString.GetLocalisationKey()))
									{
										Localisation.Set(localisedString.GetLocalisationKey(), Localisation.GetCurrentLanguage(), string.Empty);
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
							if (!string.IsNullOrEmpty(currentKey) && Localisation.Exists(currentKey))
							{
								EditorGUI.BeginChangeCheck();
								string text;
								if (style != null)
									text = EditorGUILayout.TextArea(Localisation.GetRawString(currentKey, Localisation.GetCurrentLanguage()), style);
								else
									text = EditorGUILayout.TextArea(Localisation.GetRawString(currentKey, Localisation.GetCurrentLanguage()));
								if (EditorGUI.EndChangeCheck())
								{
									Localisation.Set(currentKey, Localisation.GetCurrentLanguage(), text);
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