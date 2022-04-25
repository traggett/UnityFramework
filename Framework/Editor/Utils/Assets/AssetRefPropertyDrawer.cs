using System;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(AssetRef<>), true)]
			public class AssetRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
					property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, new GUIContent(label.text + " ("+ GetAssetType().Name + " Reference)"), true);

					if (property.isExpanded)
					{
						EditorGUI.indentLevel++;

						SerializedProperty filePathProp = property.FindPropertyRelative("_filePath");
						SerializedProperty fileGUIDProp = property.FindPropertyRelative("_fileGUID");

						string resourcePath = AssetUtils.GetResourcePath(filePathProp.stringValue);
						UnityEngine.Object asset = Resources.Load(resourcePath);

						Rect sourceTypePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

						EditorGUI.BeginChangeCheck();

						asset = EditorGUI.ObjectField(sourceTypePosition, "File", asset, GetAssetType(), false);

						if (EditorGUI.EndChangeCheck())
						{
							filePathProp.stringValue = AssetDatabase.GetAssetPath(asset);
							fileGUIDProp.stringValue = AssetDatabase.AssetPathToGUID(filePathProp.stringValue);
						}

						EditorGUI.indentLevel--;
					}
					
					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return property.isExpanded ? EditorGUIUtility.singleLineHeight * 2f : EditorGUIUtility.singleLineHeight;
				}

				private Type GetAssetType()
				{
					return fieldInfo.FieldType.GenericTypeArguments[0];
				}
			}
		}
	}
}