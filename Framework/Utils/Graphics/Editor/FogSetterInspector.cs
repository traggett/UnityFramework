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
				FogSetter fogSetter = (FogSetter)target;

				EditorGUI.BeginChangeCheck();

				fogSetter._enableFog = EditorGUILayout.Toggle("Enable Fog", fogSetter._enableFog);
				fogSetter._fogColor = EditorGUILayout.ColorField("Fog Color", fogSetter._fogColor);
				fogSetter._fogMode = (FogMode)EditorGUILayout.EnumPopup("Fog Mode", fogSetter._fogMode);

				switch (fogSetter._fogMode)
				{
					case FogMode.Linear:
						{
							fogSetter._fogStartDistance = EditorGUILayout.FloatField("Fog Start Distance", fogSetter._fogStartDistance);
							fogSetter._fogEndDistance = EditorGUILayout.FloatField("Fog End Distance", fogSetter._fogEndDistance);
						}
						break;
					case FogMode.Exponential:
					case FogMode.ExponentialSquared:
						{
							fogSetter._fogDensity = EditorGUILayout.FloatField("Fog Density", fogSetter._fogDensity);
						}
						break;
				}

				if (EditorGUI.EndChangeCheck())
				{
					fogSetter.SetFog();
				}
			}
		}
	}
}