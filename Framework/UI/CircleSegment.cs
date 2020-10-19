using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    namespace UI
    {
        public class CircleSegment : MaskableGraphic
        {
			public int _segments = 12;

			[Range(-360f, 360f)]
			public float _fromAngle = 90f;

			[Range(-360f, 360f)]
			public float _toAngle = 180f;

			protected override void OnRectTransformDimensionsChange()
            {
                base.OnRectTransformDimensionsChange();
                SetVerticesDirty();
            }

            protected override void OnPopulateMesh(VertexHelper vh)
            {
                vh.Clear();

				if (_fromAngle != _toAngle && _segments > 0)
				{
					float fromAngle = _fromAngle * Mathf.Deg2Rad;
					float toAngle = _toAngle * Mathf.Deg2Rad;
					
					float segmentAngle = (Mathf.PI * 2f) / _segments;

					float visibleRange = toAngle - fromAngle;
					if (visibleRange < 0f)
						segmentAngle = -segmentAngle;

					int shownSegments = Mathf.CeilToInt(visibleRange / segmentAngle);

					Rect rect = rectTransform.rect;
					Vector2 center = rect.center;
					float radius = rect.width * 0.5f;
					float yScale = rect.height / rect.width;

					for (int i = 0; i < shownSegments; i++)
					{
						float nextAngle;

						if (i == shownSegments - 1)
							nextAngle = toAngle;
						else
							nextAngle = fromAngle + segmentAngle;

						CreateSegment(vh, center, fromAngle, nextAngle, radius, yScale);
						fromAngle = nextAngle;
					}
				}
            }

			private void CreateSegment(VertexHelper vertexHelper, Vector2 center, float fromAngle, float toAngle, float radius, float yScale)
			{
				var i = vertexHelper.currentVertCount;

				UIVertex vert = new UIVertex();

				vert.position = center;
				vert.color = color;
				vertexHelper.AddVert(vert);

				vert.position = center + new Vector2(Mathf.Cos(fromAngle) * radius, Mathf.Sin(fromAngle) * radius);
				vert.position.y *= yScale;
				vert.color = color;
				vertexHelper.AddVert(vert);

				vert.position = center + new Vector2(Mathf.Cos(toAngle) * radius, Mathf.Sin(toAngle) * radius);
				vert.position.y *= yScale;
				vert.color = color;
				vertexHelper.AddVert(vert);

				if (fromAngle < toAngle)
					vertexHelper.AddTriangle(i + 0, i + 2, i + 1);
				else
					vertexHelper.AddTriangle(i + 0, i + 1, i + 2);
			}
		}
	}
}