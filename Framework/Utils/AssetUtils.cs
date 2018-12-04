using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace Utils
	{
		public static class AssetUtils
		{
			public static readonly string kStreamingAssets = "StreamingAssets/";
			public static readonly string kResouces = "Resources/";
			public static readonly string kAssets = "Assets/";

			public static string GetAppllicationPath()
			{
				return Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
			}

			public static string GetAssetPath(string filePath)
			{
				string assetPath = filePath;

				if (!string.IsNullOrEmpty(assetPath))
				{
					//Remove path before Assets folder
					int index = assetPath.IndexOf(kAssets);
					if (index != -1)
					{
						assetPath = assetPath.Substring(index, kAssets.Length - index);
					}
				}

				return assetPath;
			}

			public static string GetStreamingAssetPath(string filePath)
			{
				string runtimePath = filePath;

				if (!string.IsNullOrEmpty(runtimePath))
				{
					//Remove path before StreamingAssets folder
					int index = runtimePath.IndexOf(kStreamingAssets);
					if (index != -1)
					{
						index += kStreamingAssets.Length;
						runtimePath = runtimePath.Substring(index, runtimePath.Length - index);
					}
				}

				return runtimePath;
			}

			public static string GetResourcePath(string filePath)
			{
				string runtimePath = filePath;

				if (!string.IsNullOrEmpty(runtimePath))
				{
					//Remove path before Resources folder
					int index = runtimePath.IndexOf(kResouces);
					if (index != -1)
					{
						index += kResouces.Length;
						runtimePath = runtimePath.Substring(index, runtimePath.Length - index);

						//Remove file extension
						index = runtimePath.LastIndexOf(".");
						if (index != -1)
						{
							runtimePath = runtimePath.Substring(0, index);
						}
					}
					else
					{
						return null;
					}

				}

				return runtimePath;
			}

			public static bool IsStreamingAsset(Object asset)
			{
				if (asset != null)
				{
					string path = AssetDatabase.GetAssetPath(asset);
					int index = path.IndexOf(kStreamingAssets);
					return index != -1;
				}

				return true;
			}

			public static bool IsAssetInResources(Object asset)
			{
				if (asset != null)
				{
					string path = AssetDatabase.GetAssetPath(asset);
					int index = path.IndexOf(kResouces);
					return index != -1;
				}

				return true;
			}

#if UNITY_EDITOR
			public static void RefreshAsset(string filePath)
			{
				AssetDatabase.ImportAsset(GetAssetPath(filePath));
			}

			public static void CreateScriptableObjectAsset<T>() where T : ScriptableObject
			{
				T asset = ScriptableObject.CreateInstance<T>();

				string path = AssetDatabase.GetAssetPath(Selection.activeObject);
				if (path == "")
				{
					path = "Assets";
				}
				else if (Path.GetExtension(path) != "")
				{
					path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
				}

				string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

				AssetDatabase.CreateAsset(asset, assetPathAndName);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				EditorUtility.FocusProjectWindow();
				Selection.activeObject = asset;
			}
#endif
		}

	}
}