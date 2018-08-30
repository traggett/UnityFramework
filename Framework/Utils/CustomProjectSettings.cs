using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public abstract class CustomProjectSettings : ScriptableObject
		{
			public static string GetUniqueName<T>() where T : CustomProjectSettings
			{
				return typeof(T).Name.Replace("ProjectSettings", "");
			}

			public static T Get<T>() where T : CustomProjectSettings
			{
				//If the settings don't exist, create new one for default values
				T asset = Resources.Load<T>("ProjectSettings/" + GetUniqueName<T>());

				if (asset == null)
				{
					asset = ScriptableObject.CreateInstance<T>();

#if UNITY_EDITOR
					//If in editor also save this asset
					if (!Application.isPlaying)
					{
						if (!Directory.Exists(Application.dataPath + "/Resources/ProjectSettings"))
						{
							Directory.CreateDirectory(Application.dataPath + "/Resources/ProjectSettings");
						}

						string assetPathAndName = "Assets/Resources/ProjectSettings/" + GetUniqueName<T>() + ".asset";
						AssetDatabase.CreateAsset(asset, assetPathAndName);

						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
					}
#endif
				}

				return asset;
			}
		}
	}
}
