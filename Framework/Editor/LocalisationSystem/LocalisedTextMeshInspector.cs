using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		namespace Editor
		{
			public abstract class LocalisedTextMeshInspector : UnityEditor.Editor
			{
				private SerializedProperty _textProp;
				private SerializedProperty _listModeProp;
				private SerializedProperty _textListProp;
				private SerializedProperty _listStartStringProp;
				private SerializedProperty _listSeperatorStringProp;
				private SerializedProperty _listEndStringProp;

				private static readonly GUIContent _listModeLabel = new GUIContent("List Mode");
				private static readonly GUIContent _listSymbolsLabel = new GUIContent("List Symbols");
				private static readonly GUIContent _listPrefixLabel = new GUIContent("Prefix");
				private static readonly GUIContent _listSeperatorLabel = new GUIContent("Seperator");
				private static readonly GUIContent _listPostfixLabel = new GUIContent("Postfix");

				protected virtual void OnEnable()
				{
					_textProp = serializedObject.FindProperty("_text");
					
					_listModeProp = serializedObject.FindProperty("_textListMode");
					_textListProp = serializedObject.FindProperty("_textList");
					_listStartStringProp = serializedObject.FindProperty("_listStartString");
					_listSeperatorStringProp = serializedObject.FindProperty("_listSeperatorString");
					_listEndStringProp = serializedObject.FindProperty("_listEndString");
				}

				public override void OnInspectorGUI()
				{
					DrawLocalisedTextMeshProperties();
				}

				protected void DrawLocalisedTextMeshProperties()
				{
					EditorGUI.BeginChangeCheck();

					EditorGUILayout.PropertyField(_listModeProp, _listModeLabel);

					if (_listModeProp.boolValue)
					{
						EditorGUILayout.PropertyField(_textListProp);

						if (EditorGUILayout.BeginFoldoutHeaderGroup(true, _listSymbolsLabel))
						{
							EditorGUILayout.PropertyField(_listStartStringProp, _listPrefixLabel);
							EditorGUILayout.PropertyField(_listSeperatorStringProp, _listSeperatorLabel);
							EditorGUILayout.PropertyField(_listEndStringProp, _listPostfixLabel);
						}

						EditorGUILayout.EndFoldoutHeaderGroup();
					}
					else
					{
						EditorGUILayout.PropertyField(_textProp);
					}

					if (EditorGUI.EndChangeCheck())
					{
						serializedObject.ApplyModifiedProperties();

						LocalisedTextMesh localisedTextMesh = (LocalisedTextMesh)target;
						localisedTextMesh.RefreshText();
					}
				}
			}
		}
	}
}