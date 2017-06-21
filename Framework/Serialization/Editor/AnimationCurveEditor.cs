using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils.Editor;

	namespace Serialization
	{
		[SerializedObjectEditor(typeof(AnimationCurve), "PropertyField")]
		public static class AnimationCurveEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				AnimationCurve curve = (AnimationCurve)obj;

				if (curve == null)
					curve = new AnimationCurve();

				EditorGUI.BeginChangeCheck();
				curve = EditorGUILayout.CurveField(label, curve);
				dataChanged = EditorGUI.EndChangeCheck();
				return curve;
			}
			#endregion
		}
	}
}