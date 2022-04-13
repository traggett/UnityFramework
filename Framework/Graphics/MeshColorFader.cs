using UnityEngine;

namespace Framework
{
    namespace Graphics
	{
		public class MeshColorFader : ColorFader
		{
			public Renderer _renderer;

			protected override Color GetColor()
			{
				return _renderer.material.color;
			}

			protected override void OnSetColor(Color color)
			{
				_renderer.material.color = color;
			}

			protected override void OnSetAlpha(float alpha)
			{
				_renderer.material.color = ColorUtils.SetAlpha(_renderer.material.color, alpha);
			}
		}
	}
}