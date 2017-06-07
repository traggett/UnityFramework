using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
	namespace Utils
	{
		[CustomEditor(typeof(AmbientLightSetter), true)]
		public class AmbientLightSetterInspector : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				AmbientLightSetter lightSetter = (AmbientLightSetter)target;

				EditorGUI.BeginChangeCheck();

				ColorPickerHDRConfig hdrConfig = new ColorPickerHDRConfig(0.0f, 1.0f, 0.0f, 1.0f);

				lightSetter._ambientMode = (AmbientMode)EditorGUILayout.EnumPopup("Ambient Light Mode", lightSetter._ambientMode);
				
				switch (lightSetter._ambientMode)
				{
					case AmbientMode.Flat:
						{
							lightSetter._ambientLight = EditorGUILayout.ColorField(new GUIContent("Ambient Light Color"), lightSetter._ambientLight, true, false, true, hdrConfig);
							lightSetter._ambientLight.a = 0.0f;
						}
						break;
					case AmbientMode.Trilight:
						{
							lightSetter._ambientLight = EditorGUILayout.ColorField(new GUIContent("Ambient Sky Color"), lightSetter._ambientLight, true, false, true, hdrConfig);
							lightSetter._ambientLight.a = 1.0f;
							lightSetter._ambientEquatorColor = EditorGUILayout.ColorField(new GUIContent("Ambient Equator Color"), lightSetter._ambientEquatorColor, true, false, true, hdrConfig);
							lightSetter._ambientGroundColor = EditorGUILayout.ColorField(new GUIContent("Ambient Ground Color"), lightSetter._ambientGroundColor, true, false, true, hdrConfig);
						}
						break;
				}

				lightSetter._skyBox = (Material)EditorGUILayout.ObjectField(new GUIContent("Sky Box"), lightSetter._skyBox, typeof(Material), false);

				if (EditorGUI.EndChangeCheck())
				{
					lightSetter.SetAmbientLight();
				}
			}
		}
	}
}