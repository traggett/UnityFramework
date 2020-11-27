using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    namespace UI
    {
        public class ProcedualUIFade : MaskableGraphic
        {
			public enum Direction
			{ 
				Vertical,
				Horizontal
			}

			public Direction _direction;
			public Color _fromColor;
			public Color _toColor;

			public Sprite _sprite;

			public override Texture mainTexture
			{
				get
				{
					if (_sprite == null)
					{
						return null;
					}

					return _sprite.texture;
				}
			}

			protected override void OnRectTransformDimensionsChange()
            {
                base.OnRectTransformDimensionsChange();
                SetVerticesDirty();
            }

            protected override void OnPopulateMesh(VertexHelper vh)
            {
                vh.Clear();

				Vector2 bottomLeftCorner = new Vector2(0, 0) - rectTransform.pivot;
                bottomLeftCorner.x *= rectTransform.rect.width;
                bottomLeftCorner.y *= rectTransform.rect.height;

				Vector2 topRightCorner = bottomLeftCorner + (rectTransform.rect.width * Vector2.right) + (rectTransform.rect.height * Vector2.up);

				Color fromColor = _fromColor * this.color;
				Color toColor = _toColor * this.color;

				CreateQuad(vh,
                           bottomLeftCorner, topRightCorner,
						   fromColor, toColor, _direction);
            }

			private static void CreateQuad(VertexHelper vertexHelper, Vector2 bottomLeftCorner, Vector2 topRightCorner, Color from, Color to, Direction direction)
			{
				var i = vertexHelper.currentVertCount;

				UIVertex vert = new UIVertex();

				vert.position = bottomLeftCorner;
				vert.color = from;
				vert.uv0 = new Vector2(0f, 0f);
				vertexHelper.AddVert(vert);

				vert.position = new Vector2(topRightCorner.x, bottomLeftCorner.y);
				vert.color = direction == Direction.Vertical ? from : to;
				vert.uv0 = new Vector2(1f, 0f);
				vertexHelper.AddVert(vert);

				vert.position = topRightCorner;
				vert.color = to;
				vert.uv0 = new Vector2(1f, 1f);
				vertexHelper.AddVert(vert);

				vert.position = new Vector2(bottomLeftCorner.x, topRightCorner.y);
				vert.color = direction == Direction.Vertical ? to : from;
				vert.uv0 = new Vector2(0f, 1f);
				vertexHelper.AddVert(vert);

				vertexHelper.AddTriangle(i + 0, i + 2, i + 1);
				vertexHelper.AddTriangle(i + 3, i + 2, i + 0);
			}

		}
	}
}