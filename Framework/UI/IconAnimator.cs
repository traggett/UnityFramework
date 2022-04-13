using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	using Maths;
	using Graphics;
	using Animations;

	namespace UI
	{
		public class IconAnimator : HidableComponent
		{
			#region Public Data
			public RectTransform _rectTransform;
			public Image _icon;
			public float _size = 128f;
			public InterpolationType _showInterpolation;
			public InterpolationType _hideInterpolation;
			public AnimationCurve _alphaCurve;
			#endregion

			#region HidableComponent
			protected override void OnUpdateAnimations(float showLerp, bool showing)
			{
				_icon.color = ColorUtils.SetAlpha(_icon.color, _alphaCurve.Evaluate(showLerp));

				float size = Interpolation.Interpolate(showing ? _showInterpolation : _hideInterpolation, 0f, _size, showLerp);
				_rectTransform.sizeDelta = new Vector2(size, size);
			}
			#endregion
		}
	}
}