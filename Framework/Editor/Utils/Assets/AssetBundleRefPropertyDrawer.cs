using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(AssetBundleRef))]
			public class AssetBundleRefPropertyDrawer : PropertyDrawer
			{
				#region Public Interface
				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return EditorGUIUtility.singleLineHeight;
				}

				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					SerializedProperty assetBundleProperty = property.FindPropertyRelative("_fileName");

					string[] names = AssetDatabase.GetAllAssetBundleNames();
					GUIContent[] options = new GUIContent[names.Length + 1];

					int index = 0;

					options[0] = new GUIContent("(None)");

					for (int i = 0; i < names.Length; i++)
					{
						options[i + 1] = new GUIContent(names[i]);

						if (names[i] == assetBundleProperty.stringValue)
						{
							index = i + 1;
						}
					}

					EditorGUI.BeginChangeCheck();

					index = EditorGUI.Popup(position, label, index, options);

					if (EditorGUI.EndChangeCheck())
					{
						if (index == 0)
						{
							assetBundleProperty.stringValue = null;
						}
						else
						{
							assetBundleProperty.stringValue = names[index - 1];
						}
					}

					EditorGUI.EndProperty();
				}
				#endregion
			}
		}
	}
}