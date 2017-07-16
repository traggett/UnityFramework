
using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Serialization;

	namespace LocalisationSystem
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(LocalisedStringRef), "PropertyField")]
			public static class LocalisedStringRefEditor
			{
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					LocalisedStringRef localisedString = (LocalisedStringRef)obj;
					
					bool foldOut = EditorGUILayout.Foldout(localisedString._editorFoldout, label);

					if (foldOut != localisedString._editorFoldout)
					{
						localisedString._editorFoldout = foldOut;
						dataChanged = true;
					}

					if (foldOut)
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
									localisedString._editorFoldout = foldOut;
								}
								else
								{
									localisedString = new LocalisedStringRef(keys[currentKeyIndex]);
									localisedString._editorFoldout = foldOut;
								}

								dataChanged = true;
							}
						}

						//Draw buttons for adding new key
						if (currentKeyIndex == 0)
						{
							EditorGUILayout.BeginHorizontal();
							{
								string newKey = localisedString.GetLocalisationKey();
								newKey = EditorGUILayout.DelayedTextField("New Key", newKey);

								if (GUILayout.Button("Auto", GUILayout.Width(36)))
								{
									newKey = localisedString.GetAutoKey();
									localisedString = new LocalisedStringRef(newKey);
									dataChanged = true;
								}

								if (GUILayout.Button("Add", GUILayout.Width(32)) && !string.IsNullOrEmpty(newKey))
								{
									if (!Localisation.IsKeyInTable(newKey))
									{
										Localisation.UpdateString(newKey, Localisation.GetCurrentLanguage(), string.Empty);
									}

									localisedString = new LocalisedStringRef(newKey);
									localisedString._editorFoldout = foldOut;

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
								string text = EditorGUILayout.TextArea(Localisation.GetString(currentKey));
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