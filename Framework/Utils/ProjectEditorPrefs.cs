using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			public static class ProjectEditorPrefs
			{ 
				public static void DeleteAll()
				{
					EditorPrefs.DeleteAll();
				}

				public static void DeleteKey(string key)
				{

				}

				public static bool GetBool(string key, bool defaultValue)
				{
					return false;
				}
				
			}
		}
	}
}
