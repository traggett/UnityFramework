using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
	namespace Graphics
	{
		[CustomEditor(typeof(AmbientLightSetter), true)]
		public class AmbientLightSetterInspector : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{			
				SerializedProperty ambientMode = serializedObject.FindProperty("_ambientMode");
				SerializedProperty ambientLight = serializedObject.FindProperty("_ambientLight");
				SerializedProperty ambientEquatorColor = serializedObject.FindProperty("_ambientEquatorColor");
				SerializedProperty ambientGroundColor = serializedObject.FindProperty("_ambientGroundColor");
				SerializedProperty skyBox = serializedObject.FindProperty("_skyBox");
				
				EditorGUILayout.PropertyField(ambientMode);

				switch ((AmbientMode)ambientMode.intValue)
				{
					case AmbientMode.Flat:
						{
							EditorGUILayout.PropertyField(ambientLight, new GUIContent("Ambient Light Color"));
						}
						break;
					case AmbientMode.Trilight:
						{
							EditorGUILayout.PropertyField(ambientLight, new GUIContent("Ambient Sky Color"));
							EditorGUILayout.PropertyField(ambientEquatorColor, new GUIContent("Ambient Equator Color"));
							EditorGUILayout.PropertyField(ambientGroundColor, new GUIContent("Ambient Ground Color"));
						}
						break;
				}

				EditorGUILayout.PropertyField(skyBox);

				if (serializedObject.ApplyModifiedProperties())
				{
					AmbientLightSetter lightSetter = (AmbientLightSetter)target;
					lightSetter.SetAmbientLight();
				}
			}
		}
	}
}