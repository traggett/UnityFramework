using UnityEngine;
using UnityEngine.UI;

namespace Framework
{    
    namespace Utils
	{
		public class UIColorFader : ColorFader
		{
			public Graphic _graphic;

			protected override Color GetColor()
			{
				return _graphic.color;
			}

			protected override void OnSetColor(Color color)
			{
				_graphic.color = color;
			}

			protected override void OnSetAlpha(float alpha)
			{
				_graphic.color = ColorUtils.SetAlpha(_graphic.color, alpha);
			}
		}
	}
}