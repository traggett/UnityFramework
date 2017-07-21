using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(AnimationCurve), "PropertyField")]
		public static class AnimationCurveEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				AnimationCurve curve = (AnimationCurve)obj;

				if (curve == null)
					curve = new AnimationCurve();

				EditorGUI.BeginChangeCheck();
				curve = EditorGUILayout.CurveField(label, curve);
				if (EditorGUI.EndChangeCheck())
					dataChanged = true;

				return curve;
			}
			#endregion
		}
	}
}