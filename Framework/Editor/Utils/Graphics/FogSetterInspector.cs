using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		[CustomEditor(typeof(FogSetter), true)]
		public class FogSetterInspector : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				SerializedProperty enableFog = serializedObject.FindProperty("_enableFog");
				SerializedProperty fogColor = serializedObject.FindProperty("_fogColor");
				SerializedProperty fogMode = serializedObject.FindProperty("_fogMode");
				SerializedProperty fogStartDistance = serializedObject.FindProperty("_fogStartDistance");
				SerializedProperty fogEndDistance = serializedObject.FindProperty("_fogEndDistance");
				SerializedProperty fogDensity = serializedObject.FindProperty("_fogDensity");

				EditorGUI.BeginChangeCheck();

				EditorGUILayout.PropertyField(enableFog);
				EditorGUILayout.PropertyField(fogColor);
				EditorGUILayout.PropertyField(fogMode);
			
				switch ((FogMode)fogMode.intValue)
				{
					case FogMode.Linear:
						{
							EditorGUILayout.PropertyField(fogStartDistance);
							EditorGUILayout.PropertyField(fogEndDistance);
						}
						break;
					case FogMode.Exponential:
					case FogMode.ExponentialSquared:
						{
							EditorGUILayout.PropertyField(fogDensity);
						}
						break;
				}

				if (serializedObject.ApplyModifiedProperties())
				{
					FogSetter fogSetter = (FogSetter)target;
					fogSetter.SetFog();
				}
			}
		}
	}
}