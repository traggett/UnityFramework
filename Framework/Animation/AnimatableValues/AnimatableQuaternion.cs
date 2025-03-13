using UnityEngine;

namespace Framework
{
	namespace Animations
	{
		public sealed class AnimatableQuaternion : AnimatableValue<Quaternion>
		{
			#region AnimatableValue
			public static implicit operator AnimatableQuaternion(Quaternion value)
			{
				AnimatableQuaternion interpolatableValue = new AnimatableQuaternion();
				interpolatableValue.Set(value);
				return interpolatableValue;
			}

			protected override Quaternion Lerp(Quaternion from, Quaternion to, float t)
			{
				return Quaternion.Slerp(from, to, t);
			}
			#endregion
		}
	}
}