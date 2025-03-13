using UnityEngine;

namespace Framework
{
	namespace Animations
	{
		public sealed class AnimatableVector3 : AnimatableValue<Vector3>
		{
			#region AnimatableValue
			public static implicit operator AnimatableVector3(Vector3 value)
			{
				AnimatableVector3 interpolatableValue = new AnimatableVector3();
				interpolatableValue.Set(value);
				return interpolatableValue;
			}

			protected override Vector3 Lerp(Vector3 from, Vector3 to, float t)
			{
				return Vector3.Lerp(from, to, t);
			}
			#endregion
		}
	}
}