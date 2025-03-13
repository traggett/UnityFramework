using UnityEngine;

namespace Framework
{
	namespace Animations
	{
		public sealed class AnimatableColor : AnimatableValue<Color>
		{
			#region AnimatableValue
			public static implicit operator AnimatableColor(Color value)
			{
				AnimatableColor interpolatableValue = new AnimatableColor();
				interpolatableValue.Set(value);
				return interpolatableValue;
			}

			protected override Color Lerp(Color from, Color to, float t)
			{
				return Color.Lerp(from, to, t);
			}
			#endregion
		}
	}
}