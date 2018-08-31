using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public static class AssetUtils
		{
			public static string GetAppllicationPath()
			{
				return Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
			}

			public static string GetAssetPath(string filePath)
			{
				string assetPath = filePath;

				if (!string.IsNullOrEmpty(assetPath))
				{
					//Remove path before Resources folder
					string assets = "Assets/";
					int index = assetPath.IndexOf(assets);
					if (index != -1)
					{
						assetPath = assetPath.Substring(index, assetPath.Length - index);
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
					string streamingAssets = "StreamingAssets/";
					int index = runtimePath.IndexOf(streamingAssets);
					if (index != -1)
					{
						index += streamingAssets.Length;
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
					string resouces = "Resources/";
					int index = runtimePath.IndexOf(resouces);
					if (index != -1)
					{
						index += resouces.Length;
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
		}
	}
}