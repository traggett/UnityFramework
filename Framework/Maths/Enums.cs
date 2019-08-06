using System;

namespace Framework
{
	namespace Maths
	{
		[Flags]
		public enum TransformFlags
		{
			Translate = (1 << 0),
			Rotate = (1 << 1),
			Scale = (1 << 2)
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

		public enum Direction1D
		{
			Forwards,
			Backwards,
		}

		public enum Direction2D
		{
			Up,
			Down,
			Left,
			Right
		}
	}
}