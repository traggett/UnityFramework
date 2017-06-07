using System;

using UnityEditor;
using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		[JSONEditor(typeof(FlagsAttribute), "PropertyField")]
		public static class JSONFlagsEditor
		{
			#region JSONObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
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