using Framework.Maths;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public static class RectTransformUtils
		{
			public static void SetLeft(RectTransform rt, float left)
			{
				rt.offsetMin = new Vector2(left, rt.offsetMin.y);
			}

			public static void SetRight(RectTransform rt, float right)
			{
				rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
			}

			public static void SetTop(RectTransform rt, float top)
			{
				rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
			}

			public static void SetBottom(RectTransform rt, float bottom)
			{
				rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
			}
		}
	}
}