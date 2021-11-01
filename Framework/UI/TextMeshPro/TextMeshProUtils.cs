﻿using System.Reflection;
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
				public static Vector2 GetTextMeshProSize(TMP_Text textMesh)
				{
					//Make sure Text Mesh has had Awake called
					ForceAwaken(textMesh);

					textMesh.ComputeMarginSize();
					textMesh.ForceMeshUpdate(true, false);

					return textMesh.GetRenderedValues(true);
				}

				public static float GetTextMeshProWidth(TMP_Text textMesh)
				{
					return GetTextMeshProSize(textMesh).x;
				}

				public static float GetTextMeshProHeight(TMP_Text textMesh)
				{
					return GetTextMeshProSize(textMesh).y;
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
						MethodInfo awakeMethod = textMesh.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
						awakeMethod.Invoke(textMesh, new object[0]);
					}
				}

			}
		}
	}
}