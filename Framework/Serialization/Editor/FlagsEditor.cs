using System;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(FlagsAttribute), "PropertyField")]
		public static class FlagsEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
			{
				string[] flagOptions = Enum.GetNames(obj.GetType());
				EditorGUI.BeginChangeCheck();
				int flags = EditorGUILayout.MaskField(label, Convert.ToInt32(obj), flagOptions);
				if (EditorGUI.EndChangeCheck())
				{
					//-1 when 'Everything' is selected
					if (flags == -1)
					{
						flags = 0;

						foreach (var enumValue in Enum.GetValues(obj.GetType()))
						{
							int flag = Convert.ToInt32(enumValue);
							flags |= flag;
						}
					}

					dataChanged = true;
					return Enum.ToObject(obj.GetType(), flags);
				}

				dataChanged =  false;
				return obj;
			}
			#endregion
		}
	}
}