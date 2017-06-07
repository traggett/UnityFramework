namespace Framework
{
	namespace Maths
	{
		public enum eDirection
		{
			None,
			Left,
			Right,
			Up,
			Down
		}

		public enum eAlignment2D
		{
			Horiztonal,
			Vertical
		}

		public enum eDirection2D
		{
			Forwards = 1,
			Backwards = -1,
		}

		public enum eInterpolation
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