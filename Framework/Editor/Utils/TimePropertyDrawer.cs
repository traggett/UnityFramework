using UnityEngine;
using UnityEditor;
using System;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(TimeProperty))]
			public class TimePropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);
					
					SerializedProperty timeProperty = property.FindPropertyRelative("_time");
					
					TimeSpan timeSpan = TimeSpan.FromSeconds(timeProperty.doubleValue);

					string timeString = timeSpan.Minutes.ToString("00") + ":" + timeSpan.Seconds.ToString("00") + "." + timeSpan.Milliseconds.ToString("000");

					//Parse string

					EditorGUI.BeginChangeCheck();
					timeString = EditorGUI.DelayedTextField(position, label, timeString);

					if (EditorGUI.EndChangeCheck())
					{
						double newSeconds = 0d;

						//Milliseconds
						{
							int miliIndex = timeString.LastIndexOf(".");

							if (miliIndex != -1)
							{
								string millisecondsStr = timeString.Substring(miliIndex + 1);

								try
								{
									double milliseconds = Convert.ToDouble(millisecondsStr);
									newSeconds += milliseconds / 1000d;
								}
								catch
								{ }

								timeString = timeString.Substring(0, miliIndex);
							}
						}
						//Minutes
						{
							int minutesIndex = timeString.IndexOf(":");

							if (minutesIndex != -1)
							{
								string minutesStr = timeString.Substring(0, minutesIndex);

								try
								{
									double minutes = Convert.ToDouble(minutesStr);
									newSeconds += minutes * 60d;
								}
								catch
								{ }

								timeString = timeString.Substring(minutesIndex + 1);
							}
						}
						//Seconds
						if (!string.IsNullOrEmpty(timeString))
						{
							try
							{
								double seconds = Convert.ToDouble(timeString);
								newSeconds += seconds;
							}
							catch
							{ }
						}

						timeProperty.doubleValue = newSeconds;
					}

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}