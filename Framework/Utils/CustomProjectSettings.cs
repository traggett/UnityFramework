using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public abstract class CustomProjectSettings<T> : ScriptableObject where T : CustomProjectSettings<T>
		{
			private static T _settings;

			public static string GetUniqueName()
			{
				return typeof(T).Name.Replace("ProjectSettings", "");
			}

			public static T Get()
			{
				if (_settings == null)
				{
					string assetName = GetUniqueName();

					//If the settings don't exist, create new one for default values
					_settings = Resources.Load<T>("ProjectSettings/" + assetName);

					if (_settings == null)
					{
						_settings = CreateInstance<T>();

#if UNITY_EDITOR
						string fileFolder = Application.dataPath + "/Resources/ProjectSettings";

						//If in editor also save this asset
						if (!Application.isPlaying && !File.Exists(fileFolder + "/ " + assetName + ".asset"))
						{
							if (!Directory.Exists(fileFolder))
							{
								Directory.CreateDirectory(fileFolder);
							}

							string assetPathAndName = "Assets/Resources/ProjectSettings/" + assetName + ".asset";
							AssetDatabase.CreateAsset(_settings, assetPathAndName);

							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
						}
#endif
					}
				}

				return _settings;
			}
		}
	}
}
