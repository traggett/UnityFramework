using System;

namespace Framework
{
	namespace Maths
	{
		[Flags]
		public enum TransformFlags
		{
			None = 0x0,
			Translate = 0x1,
			Rotate = 0x2,
			Scale = 0x3
		}

		public enum InterpolationType
		{
			Linear,
			InQuad,
			OutQuad,
			InOutQuad,
			InCubic,
			OutCubic,
			InOutCubic,
			InSine,
			OutSine,
			InOutSine,
			InOutSineInv,
			InElastic,
			OutElastic,
			InOutElastic,
			InBounce,
			OutBounce,
			InOutBounce,
		}
	}
}