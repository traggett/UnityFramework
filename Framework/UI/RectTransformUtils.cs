using UnityEngine;

namespace Framework
{
	namespace UI
	{
		public static class RectTransformUtils
		{
			private static readonly Vector3[] _worldCornersCache = new Vector3[4];

			public static void SetLeft(RectTransform transform, float left)
			{
				transform.offsetMin = new Vector2(left, transform.offsetMin.y);
			}

			public static float GetLeft(RectTransform transform)
			{
				return transform.offsetMin.x;
			}

			public static void SetRight(RectTransform transform, float right)
			{
				transform.offsetMax = new Vector2(-right, transform.offsetMax.y);
			}

			public static float GetRight(RectTransform transform)
			{
				return -transform.offsetMax.x;
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

			public static float GetBottom(RectTransform transform)
			{
				return transform.offsetMin.x;
			}

			public static void SetWidth(RectTransform transform, float width)
			{
				transform.sizeDelta = new Vector2(width, transform.sizeDelta.y);
			}

			public static float GetWidth(RectTransform transform)
			{
				return transform.sizeDelta.x;
			}

			public static void SetHeight(RectTransform transform, float height)
			{
				transform.sizeDelta = new Vector2(transform.sizeDelta.x, height);
			}

			public static float GetHeight(RectTransform transform)
			{
				return transform.sizeDelta.y;
			}

			public static void SetX(RectTransform transform, float x)
			{
				transform.anchoredPosition = new Vector2(x, transform.anchoredPosition.y);
			}

			public static float GetX(RectTransform transform)
			{
				return transform.anchoredPosition.x;
			}

			public static void SetY(RectTransform transform, float y)
			{
				transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, y);
			}

			public static float GetY(RectTransform transform)
			{
				return transform.anchoredPosition.y;
			}

			public static Vector3 GetWorldPos(RectTransform rectTransform)
			{
				rectTransform.GetWorldCorners(_worldCornersCache);
				return _worldCornersCache[0] + ((_worldCornersCache[2] - _worldCornersCache[0]) * 0.5f);
			}

			public static void SetPivotKeepPosition(RectTransform rectTransform, Vector2 pivot)
			{
				Vector3 deltaPosition = rectTransform.pivot - pivot;
				deltaPosition.Scale(rectTransform.rect.size);
				deltaPosition.Scale(rectTransform.localScale);
				deltaPosition = rectTransform.rotation * deltaPosition;

				rectTransform.pivot = pivot;
				rectTransform.localPosition -= deltaPosition; 
			}
		}
	}
}