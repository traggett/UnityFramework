using System.Reflection;
using TMPro;
using UnityEngine;

namespace Framework
{
	namespace UI
	{
		namespace TextMeshPro
		{
			public static class TextMeshProUtils
			{
				public static Vector2 GetTextMeshProSize(TMP_Text textMesh, bool onlyVisibleCharacters = true)
				{
					//Make sure Text Mesh has had Awake called
					ForceAwaken(textMesh);

					textMesh.ComputeMarginSize();
					textMesh.ForceMeshUpdate(true, false);

					return textMesh.GetRenderedValues(onlyVisibleCharacters);
				}

				public static float GetTextMeshProWidth(TMP_Text textMesh, bool onlyVisibleCharacters = true)
				{
					return GetTextMeshProSize(textMesh, onlyVisibleCharacters).x;
				}

				public static float GetTextMeshProHeight(TMP_Text textMesh, bool onlyVisibleCharacters = true)
				{
					return GetTextMeshProSize(textMesh, onlyVisibleCharacters).y;
				}

				public static bool IsAwake(TMP_Text textMesh)
				{
					FieldInfo awakeField = textMesh.GetType().GetField("m_isAwake", BindingFlags.NonPublic | BindingFlags.Instance);
					bool awake = (bool)awakeField.GetValue(textMesh);
					return awake;
				}

				public static void ForceAwaken(TMP_Text textMesh)
				{
					if (!IsAwake(textMesh))
					{
						//Manually call awake
						MethodInfo awakeMethod = textMesh.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
						awakeMethod.Invoke(textMesh, new object[0]);
					}

					//Need to also cache canvas - might be inactive
					if (textMesh is TextMeshProUGUI)
					{
						if (textMesh.canvas == null)
						{
							Canvas canvas = textMesh.GetComponentInParent<Canvas>(true);
							FieldInfo canvasField = typeof(UnityEngine.UI.Graphic).GetField("m_Canvas", BindingFlags.NonPublic | BindingFlags.Instance);
							canvasField.SetValue(textMesh, canvas);
						}
					}
				}

			}
		}
	}
}