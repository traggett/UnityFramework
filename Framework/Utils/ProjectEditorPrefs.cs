#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			public static class ProjectEditorPrefs
			{
				private static string kJoin = ".";

				public static void DeleteKey(string key)
				{
					EditorPrefs.DeleteKey(GetProjectKey(key));
				}

				public static string GetString(string key, string defaultValue)
				{
					return EditorPrefs.GetString(GetProjectKey(key), defaultValue);
				}

				public static bool GetBool(string key, bool defaultValue)
				{
					return EditorPrefs.GetBool(GetProjectKey(key), defaultValue);
				}

				public static int GetInt(string key, int defaultValue)
				{
					return EditorPrefs.GetInt(GetProjectKey(key), defaultValue);
				}

				public static float GetFloat(string key, float defaultValue)
				{
					return EditorPrefs.GetFloat(GetProjectKey(key), defaultValue);
				}

				public static void SetString(string key, string value)
				{
					EditorPrefs.SetString(GetProjectKey(key), value);
				}

				public static void SetBool(string key, bool value)
				{
					EditorPrefs.SetBool(GetProjectKey(key), value);
				}

				public static void SetInt(string key, int value)
				{
					EditorPrefs.SetInt(GetProjectKey(key), value);
				}

				public static void SetFloat(string key, float value)
				{
					EditorPrefs.SetFloat(GetProjectKey(key), value);
				}

				private static string GetProjectId()
				{
					return Application.productName;
				}

				private static string GetProjectKey(string key)
				{
					return GetProjectId() + kJoin + key;
				}
			}
		}
	}
}

#endif