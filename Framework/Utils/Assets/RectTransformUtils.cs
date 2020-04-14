using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public static class RectTransformUtils
		{
			public static void SetLeft(RectTransform transform, float left)
			{
				transform.offsetMin = new Vector2(left, transform.offsetMin.y);
			}

			public static void SetRight(RectTransform transform, float right)
			{
				transform.offsetMax = new Vector2(-right, transform.offsetMax.y);
			}

			public static void SetTop(RectTransform transform, float top)
			{
				transform.offsetMax = new Vector2(transform.offsetMax.x, -top);
			}

			public static float GetTop(RectTransform transform)
			{
				return -transform.offsetMax.y;
			}

			public static void SetBottom(RectTransform transform, float bottom)
			{
				transform.offsetMin = new Vector2(transform.offsetMin.x, bottom);
			}

			public static void SetWidth(RectTransform transform, float width)
			{
				transform.sizeDelta = new Vector2(width, transform.sizeDelta.y);
			}

			public static void SetHeight(RectTransform transform, float height)
			{
				transform.sizeDelta = new Vector2(transform.sizeDelta.x, height);
			}
		}
	}
}