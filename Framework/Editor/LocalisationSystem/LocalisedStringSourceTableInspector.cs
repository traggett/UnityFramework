using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(LocalisedStringSourceTable), true)]
			public class LocalisedStringSourceTableInspector : UnityEditor.Editor
			{
				private string _cachedInfo = null;

				public override void OnInspectorGUI()
				{
					LocalisedStringSourceTable sourceAsset = (LocalisedStringSourceTable)target;

					if (GUILayout.Button("Edit"))
					{
						LocalisationEditorWindow.Load(sourceAsset);
					}

					if (_cachedInfo == null)
					{
						var items = sourceAsset.FindStrings();
						

						Dictionary<SystemLanguage, List<LocalisedStringSourceAsset>> itemsMap = new Dictionary<SystemLanguage, List<LocalisedStringSourceAsset>>();

						foreach ( var item in items )
						{
							var languages = item.GetLanguages();

							foreach ( var language in languages )
							{
								if (!itemsMap.ContainsKey(language))
									itemsMap[language] = new List<LocalisedStringSourceAsset>();

								itemsMap[language].Add(item);
							}
						}

						_cachedInfo = string.Format("{0} entries", items.Length);
						_cachedInfo += string.Format("\n\n{0} languages:", itemsMap.Keys.Count);

						foreach (var key in  itemsMap.Keys )
						{
							_cachedInfo += string.Format("\n- {0}: {1} translations", key, itemsMap[key].Count);
						}
					}

					EditorGUILayout.HelpBox(_cachedInfo, MessageType.None);
				}
			}
		}
	}
}