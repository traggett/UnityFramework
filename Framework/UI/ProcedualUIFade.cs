using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    namespace UI
    {
        public class ProcedualUIFade : Image
        {
			public enum FadeDirection
			{ 
				Vertical,
				Horizontal
			}
			
			public FadeDirection Direction 
			{ 
				get 
				{ 
					return _direction; 
				} 
				set 
				{ 
					_direction = value; 
					SetVerticesDirty();
				} 
			}

			public virtual Color FromColor 
			{ 
				get 
				{ 
					return color; 
				} 
				set 
				{ 
					color = value; 
				} 
			}

			public virtual Color ToColor 
			{ 
				get 
				{ 
					return _toColor; 
				} 
				set 
				{ 
					_toColor = value; 
					SetVerticesDirty();
				} 
			}

			[SerializeField]
			private Color _toColor;

			[SerializeField]
			private FadeDirection _direction;

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
				Vector2[] uvs;

				if (sprite != null && sprite.uv.Length == 4)
				{
					uvs = sprite.uv;
				}
				else
				{
					uvs = new Vector2[]
					{
						new Vector2(0f, 0f),
						new Vector2(1f, 0f),
						new Vector2(1f, 1f),
						new Vector2(0f, 1f),
					};
				}

				vh.AddVert(bottomLeftCorner, FromColor, uvs[0]);
				vh.AddVert(new Vector2(topRightCorner.x, bottomLeftCorner.y), _direction == FadeDirection.Vertical ? FromColor : ToColor, uvs[1]);
				vh.AddVert(topRightCorner, ToColor, uvs[2]);
				vh.AddVert(new Vector2(bottomLeftCorner.x, topRightCorner.y), _direction == FadeDirection.Vertical ? ToColor : FromColor, uvs[3]);

				vh.AddTriangle(0, 2, 1);
				vh.AddTriangle(3, 2, 0);
            }
		}
	}
}