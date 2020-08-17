using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	using Maths;
	using Utils;

	namespace UI
	{
		public class IconAnimator : UIAnimator
		{
			#region Public Data
			public RectTransform _rectTransform;
			public Image _backer;
			public float _size = 128f;
			public InterpolationType _showInterpolation;
			public InterpolationType _hideInterpolation;
			public AnimationCurve _alphaCurve;
			#endregion

			#region UIAnimator
			protected override void OnUpdateAnimations(float showLerp, bool showing)
			{
				_backer.color = ColorUtils.SetAlpha(_backer.color, _alphaCurve.Evaluate(showLerp));

				float size = Interpolation.Interpolate(showing ? _showInterpolation : _hideInterpolation, 0f, _size, showLerp);
				_rectTransform.sizeDelta = new Vector2(size, size);
			}
			#endregion
		}
	}
}