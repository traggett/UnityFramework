using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Serialization;

	namespace LocalisationSystem
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(LocalisedString), "PropertyField")]
			public static class LocalisedStringEditor
			{
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
				{
					LocalisedString localisedString = (LocalisedString)obj;

					/*
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
								localisedString = new LocalisedString();
							}
							else
							{
								localisedString = keys[currentKeyIndex];
							}

							dataChanged = true;
						}
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
					*/
					return localisedString;
				}
				#endregion
			}
		}
	}
}