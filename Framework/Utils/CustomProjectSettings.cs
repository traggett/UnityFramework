using System;
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
					//If the settings don't exist, create new one for default values
					_settings = Resources.Load<T>("ProjectSettings/" + GetUniqueName());

					if (_settings == null)
					{
						_settings = CreateInstance<T>();

#if UNITY_EDITOR
						//If in editor also save this asset
						if (!Application.isPlaying && !File.Exists(Application.dataPath + "/Resources/ProjectSettings/" + GetUniqueName() + ".asset"))
						{
							if (!Directory.Exists(Application.dataPath + "/Resources/ProjectSettings"))
							{
								Directory.CreateDirectory(Application.dataPath + "/Resources/ProjectSettings");
							}

							string assetPathAndName = "Assets/Resources/ProjectSettings/" + GetUniqueName() + ".asset";
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
