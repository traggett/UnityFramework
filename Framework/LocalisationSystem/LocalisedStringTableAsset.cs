using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace LocalisationSystem
	{
		public class LocalisedStringTableAsset : ScriptableObject
		{
			#region Private Data
#if UNITY_EDITOR
			private const string AssetsRootFolder = "Assets/";
#endif
			#endregion

			#region Public Methods
#if UNITY_EDITOR
			public LocalisedStringSourceAsset[] FindStrings()
			{
				string rootFolder = AssetDatabase.GetAssetPath(this);

				int lastDash = rootFolder.LastIndexOf('/');

				if (lastDash != -1)
				{
					rootFolder = rootFolder.Substring(AssetsRootFolder.Length, lastDash - AssetsRootFolder.Length);
				}
				 
				List<LocalisedStringSourceAsset> strings = new List<LocalisedStringSourceAsset>();

				AddFromDirectory(strings, Application.dataPath + "/" + rootFolder);

				foreach (LocalisedStringSourceAsset item in strings)
				{
					item.CachedEditorData(this, rootFolder);
				}

				return strings.ToArray();
			}
#endif
			#endregion

			#region Private Functions
#if UNITY_EDITOR
			private static void AddFromDirectory(List<LocalisedStringSourceAsset> strings, string folderPath)
			{
				string[] fileEntries = Directory.GetFiles(folderPath);

				foreach (string fileName in fileEntries)
				{
					if (fileName.EndsWith(".asset"))
					{
						int assetPathIndex = fileName.IndexOf(AssetsRootFolder);
						string localPath = fileName.Substring(assetPathIndex);

						LocalisedStringSourceAsset item = AssetDatabase.LoadAssetAtPath<LocalisedStringSourceAsset>(localPath);

						if (item != null)
						{
							strings.Add(item);
						}
					}		
				}

				string[] subFolders = Directory.GetDirectories(folderPath);

				foreach (string subFolder in subFolders)
				{
					AddFromDirectory(strings, subFolder);
				}
			}
#endif
			#endregion
		}
	}
}