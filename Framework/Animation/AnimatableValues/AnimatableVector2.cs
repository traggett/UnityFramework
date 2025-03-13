using UnityEngine;

namespace Framework
{
	namespace Animations
	{
		public sealed class AnimatableVector2 : AnimatableValue<Vector2>
		{
			#region AnimatableValue
			public static implicit operator AnimatableVector2(Vector2 value)
			{
				AnimatableVector2 interpolatableValue = new AnimatableVector2();
				interpolatableValue.Set(value);
				return interpolatableValue;
			}

			protected override Vector2 Lerp(Vector2 from, Vector2 to, float t)
			{
				return Vector2.Lerp(from, to, t);
			}
			#endregion
		}
	}
}