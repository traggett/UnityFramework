using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    namespace UI
    {
        public class ProcedualUIFade : MaskableGraphic
        {
			public Color _topColor;
			public Color _bottomColor;
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

                var bottomLeftCorner = new Vector2(0, 0) - rectTransform.pivot;
                bottomLeftCorner.x *= rectTransform.rect.width;
                bottomLeftCorner.y *= rectTransform.rect.height;

                CreateQuad(vh,
                           bottomLeftCorner,
                           bottomLeftCorner + (rectTransform.rect.width * Vector2.right) + (rectTransform.rect.height * Vector2.up));
            }

			private void CreateQuad(VertexHelper vertexHelper, Vector2 bottomLeftCorner, Vector2 topRightCorner)
			{
				var i = vertexHelper.currentVertCount;

				UIVertex vert = new UIVertex();

				vert.position = bottomLeftCorner;
				vert.color = _bottomColor;
				vert.uv0 = new Vector2(0f, 0f);
				vertexHelper.AddVert(vert);

				vert.position = new Vector2(topRightCorner.x, bottomLeftCorner.y);
				vert.color = _bottomColor;
				vert.uv0 = new Vector2(1f, 0f);
				vertexHelper.AddVert(vert);

				vert.position = topRightCorner;
				vert.color = _topColor;
				vert.uv0 = new Vector2(1f, 1f);
				vertexHelper.AddVert(vert);

				vert.position = new Vector2(bottomLeftCorner.x, topRightCorner.y);
				vert.color = _topColor;
				vert.uv0 = new Vector2(0f, 1f);
				vertexHelper.AddVert(vert);

				vertexHelper.AddTriangle(i + 0, i + 2, i + 1);
				vertexHelper.AddTriangle(i + 3, i + 2, i + 0);
			}

		}
	}
}