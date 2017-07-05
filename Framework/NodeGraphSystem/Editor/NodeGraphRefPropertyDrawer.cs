using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace NodeGraphSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(NodeGraphRefProperty))]
			public sealed class NodeGraphRefPropertyDrawer : PropertyDrawer
			{
				private float _height = EditorGUIUtility.singleLineHeight * 3;
				
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

					property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, property.displayName);
					_height = EditorGUIUtility.singleLineHeight;

					if (property.isExpanded)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						Rect filePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

						SerializedProperty fileProp = property.FindPropertyRelative("_file");
						fileProp.objectReferenceValue = EditorGUI.ObjectField(filePosition, "File", fileProp.objectReferenceValue, typeof(TextAsset), true);

						_height += EditorGUIUtility.singleLineHeight;

						if (fileProp.objectReferenceValue != null)
						{
							Rect editButtonPosition = new Rect(position.x + EditorGUIUtility.labelWidth, position.y + EditorGUIUtility.singleLineHeight * 2, (position.width - EditorGUIUtility.labelWidth), EditorGUIUtility.singleLineHeight);

							if (GUI.Button(editButtonPosition, "Edit"))
							{
								NodeGraphEditorWindow.Load(fileProp.objectReferenceValue as TextAsset);
							}
							_height += EditorGUIUtility.singleLineHeight;
						}					
						 
						EditorGUI.indentLevel = origIndent;
					}

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return _height;
				}
			}
		}
	}
}