using UnityEngine;
using System;

namespace Framework
{
	namespace Maths
	{
		public static class Interpolation
		{
			#region Utility Functions
			public static float Interpolate(eInterpolation type, float start, float end, float t)
			{
				switch (type)
				{
					case eInterpolation.InQuad:
						return InterpolateInQuad(start, end, t);
					case eInterpolation.OutQuad:
						return InterpolateOutQuad(start, end, t);
					case eInterpolation.InOutQuad:
						return InterpolateInOutQuad(start, end, t);
					case eInterpolation.InOutCubic:
						return InterpolateInOutCubic(start, end, t);
					case eInterpolation.InCubic:
						return InterpolateInCubic(start, end, t);
					case eInterpolation.OutCubic:
						return InterpolateOutCubic(start, end, t);
					case eInterpolation.InSine:
						return InterpolateInSine(start, end, t);
					case eInterpolation.OutSine:
						return InterpolateOutSine(start, end, t);
					case eInterpolation.InOutSine:
						return InterpolateInOutSine(start, end, t);
					case eInterpolation.InOutSineInv:
						return InterpolateInOutSineInv(start, end, t);
					case eInterpolation.InElastic:
						return InterpolateInElastic(start, end, t);
					case eInterpolation.OutElastic:
						return InterpolateOutElastic(start, end, t);
					case eInterpolation.InOutElastic:
						return InterpolateInOutElastic(start, end, t);
					case eInterpolation.InBounce:
						return InterpolateInBounce(start, end, t);
					case eInterpolation.OutBounce:
						return InterpolateOutBounce(start, end, t);
					case eInterpolation.InOutBounce:
						return InterpolateInOutBounce(start, end, t);
					default:
					case eInterpolation.Linear:
						return InterpolateLinear(start, end, t);
				}
			}

			public static double Interpolate(eInterpolation type, double start, double end, double t)
			{
				switch (type)
				{
					case eInterpolation.InQuad:
						return InterpolateInQuad(start, end, t);
					case eInterpolation.OutQuad:
						return InterpolateOutQuad(start, end, t);
					case eInterpolation.InOutQuad:
						return InterpolateInOutQuad(start, end, t);
					case eInterpolation.InOutCubic:
						return InterpolateInOutCubic(start, end, t);
					case eInterpolation.InCubic:
						return InterpolateInCubic(start, end, t);
					case eInterpolation.OutCubic:
						return InterpolateOutCubic(start, end, t);
					case eInterpolation.InSine:
						return InterpolateInSine(start, end, t);
					case eInterpolation.OutSine:
						return InterpolateOutSine(start, end, t);
					case eInterpolation.InOutSine:
						return InterpolateInOutSine(start, end, t);
					case eInterpolation.InOutSineInv:
						return InterpolateInOutSineInv(start, end, t);
					case eInterpolation.InElastic:
						return InterpolateInElastic(start, end, t);
					case eInterpolation.OutElastic:
						return InterpolateOutElastic(start, end, t);
					case eInterpolation.InOutElastic:
						return InterpolateInOutElastic(start, end, t);
					case eInterpolation.InBounce:
						return InterpolateInBounce(start, end, t);
					case eInterpolation.OutBounce:
						return InterpolateOutBounce(start, end, t);
					case eInterpolation.InOutBounce:
						return InterpolateInOutBounce(start, end, t);
					default:
					case eInterpolation.Linear:
						return InterpolateLinear(start, end, t);
				}
			}
			#endregion

			#region Linear
			public static float InterpolateLinear(float start, float end, float t)
			{
				end -= start;
				return end * t + start;
			}

			public static double InterpolateLinear(double start, double end, double t)
			{
				end -= start;
				return end * t + start;
			}
			#endregion

			#region Quad
			public static float InterpolateInQuad(float start, float end, float t)
			{
				end -= start;
				return end * t * t + start;
			}

			public static double InterpolateInQuad(double start, double end, double t)
			{
				end -= start;
				return end * t * t + start;
			}

			public static float InterpolateOutQuad(float start, float end, float t)
			{
				end -= start;
				return -end * t * (t - 2.0f) + start;
			}

			public static double InterpolateOutQuad(double start, double end, double t)
			{
				end -= start;
				return -end * t * (t - 2.0d) + start;
			}

			public static float InterpolateInOutQuad(float start, float end, float t)
			{
				t /= 0.5f;
				end -= start;
				if (t < 1.0f) return end * 0.5f * t * t + start;
				t--;
				return -end * 0.5f * (t * (t - 2.0f) - 1.0f) + start;
			}

			public static double InterpolateInOutQuad(double start, double end, double t)
			{
				t /= 0.5d;
				end -= start;

				if (t < 1.0d)
					return end * 0.5d * t * t + start;

				t--;
				return -end * 0.5d * (t * (t - 2.0d) - 1.0d) + start;
			}
			#endregion

			#region Cubic
			public static float InterpolateInCubic(float start, float end, float t)
			{
				t--;
				end -= start;
				return end * (t * t * t + 1.0f) + start;
			}

			public static double InterpolateInCubic(double start, double end, double t)
			{
				t--;
				end -= start;
				return end * (t * t * t + 1.0d) + start;
			}

			public static float InterpolateOutCubic(float start, float end, float t)
			{
				t /= 0.5f;
				end -= start;
				if (t < 1.0f)
					return end * 0.5f * t * t * t + start;
				else
				{
					t -= 2.0f;
					return end * 0.5f * (t * t * t + 2.0f) + start;
				}
			}


			public static double InterpolateOutCubic(double start, double end, double t)
			{
				t /= 0.5d;
				end -= start;
				if (t < 1.0d)
					return end * 0.5f * t * t * t + start;
				else
				{
					t -= 2.0d;
					return end * 0.5d * (t * t * t + 2.0d) + start;
				}
			}

			public static float InterpolateInOutCubic(float start, float end, float t)
			{
				t /= 0.5f;
				end -= start;
				if (t < 1.0f)
					return end * 0.5f * t * t * t + start;
				else
				{
					t -= 2.0f;
					return end * 0.5f * (t * t * t + 2.0f) + start;
				}
			}

			public static double InterpolateInOutCubic(double start, double end, double t)
			{
				t /= 0.5d;
				end -= start;
				if (t < 1.0d)
					return end * 0.5d * t * t * t + start;
				else
				{
					t -= 2.0d;
					return end * 0.5d * (t * t * t + 2.0d) + start;
				}
			}
			#endregion

			#region Sine
			public static float InterpolateInSine(float start, float end, float t)
			{
				end -= start;
				return -end * Mathf.Cos(t * (Mathf.PI * 0.5f)) + end + start;
			}

			public static double InterpolateInSine(double start, double end, double t)
			{
				end -= start;
				return -end * Math.Cos(t * (Math.PI * 0.5d)) + end + start;
			}

			public static float InterpolateOutSine(float start, float end, float t)
			{
				end -= start;
				return end * Mathf.Sin(t * (Mathf.PI * 0.5f)) + start;
			}

			public static double InterpolateOutSine(double start, double end, double t)
			{
				end -= start;
				return end * Math.Sin(t * (Math.PI * 0.5d)) + start;
			}

			public static float InterpolateInOutSine(float start, float end, float t)
			{
				end -= start;
				return -end * 0.5f * (Mathf.Cos(Mathf.PI * t) - 1.0f) + start;
			}

			public static double InterpolateInOutSine(double start, double end, double t)
			{
				end -= start;
				return -end * 0.5d * (Math.Cos(Math.PI * t) - 1.0d) + start;
			}

			public static float InterpolateInOutSineInv(float start, float end, float t)
			{
				end -= start;
				return end * (Mathf.Acos(-2.0f * (t - 0.5f)) / Mathf.PI) + start;
			}

			public static double InterpolateInOutSineInv(double start, double end, double t)
			{
				end -= start;
				return end * (Math.Acos(-2.0d * (t - 0.5d)) / Math.PI) + start;
			}
			#endregion

			#region Elastic
			public static float InterpolateInElastic(float start, float end, float t)
			{
				end -= start;

				float d = 1f;
				float p = d * .3f;
				float s = 0;
				float a = 0;

				if (t == 0) return start;

				if ((t /= d) == 1) return start + end;

				if (a == 0f || a < Mathf.Abs(end))
				{
					a = end;
					s = p / 4;
				}
				else {
					s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
				}

				return -(a * Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + start;
			}

			public static double InterpolateInElastic(double start, double end, double t)
			{
				end -= start;

				double d = 1d;
				double p = d * .3d;
				double s = 0;
				double a = 0;

				if (t == 0) return start;

				if ((t /= d) == 1) return start + end;

				if (a == 0f || a < Math.Abs(end))
				{
					a = end;
					s = p / 4;
				}
				else {
					s = p / (2 * Math.PI) * Math.Asin(end / a);
				}

				return -(a * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + start;
			}

			public static float InterpolateOutElastic(float start, float end, float t)
			{
				end -= start;

				float d = 1f;
				float p = d * .3f;
				float s = 0;
				float a = 0;

				if (t == 0) return start;

				if ((t /= d) == 1) return start + end;

				if (a == 0f || a < Mathf.Abs(end))
				{
					a = end;
					s = p * 0.25f;
				}
				else {
					s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
				}

				return (a * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + end + start);
			}

			public static double InterpolateOutElastic(double start, double end, double t)
			{
				end -= start;

				double d = 1d;
				double p = d * .3d;
				double s = 0;
				double a = 0;

				if (t == 0) return start;

				if ((t /= d) == 1) return start + end;

				if (a == 0f || a < Math.Abs(end))
				{
					a = end;
					s = p * 0.25d;
				}
				else {
					s = p / (2 * Math.PI) * Math.Asin(end / a);
				}

				return (a * Math.Pow(2, -10 * t) * Math.Sin((t * d - s) * (2 * Math.PI) / p) + end + start);
			}

			public static float InterpolateInOutElastic(float start, float end, float t)
			{
				end -= start;

				float d = 1f;
				float p = d * .3f;
				float s = 0;
				float a = 0;

				if (t == 0) return start;

				if ((t /= d * 0.5f) == 2) return start + end;

				if (a == 0f || a < Mathf.Abs(end))
				{
					a = end;
					s = p / 4;
				}
				else {
					s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
				}

				if (t < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + start;
				return a * Mathf.Pow(2, -10 * (t -= 1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
			}

			public static double InterpolateInOutElastic(double start, double end, double t)
			{
				end -= start;

				double d = 1f;
				double p = d * .3f;
				double s = 0;
				double a = 0;

				if (t == 0) return start;

				if ((t /= d * 0.5f) == 2) return start + end;

				if (a == 0f || a < Math.Abs(end))
				{
					a = end;
					s = p / 4;
				}
				else {
					s = p / (2 * Math.PI) * Math.Asin(end / a);
				}

				if (t < 1) return -0.5f * (a * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + start;
				return a * Math.Pow(2, -10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
			}
			#endregion

			#region Bounce
			public static float InterpolateInBounce(float start, float end, float t)
			{
				end -= start;
				float d = 1f;
				return end - InterpolateOutBounce(0, end, d - t) + start;
			}

			public static double InterpolateInBounce(double start, double end, double t)
			{
				end -= start;
				double d = 1d;
				return end - InterpolateOutBounce(0, end, d - t) + start;
			}

			public static float InterpolateOutBounce(float start, float end, float t)
			{
				t /= 1f;
				end -= start;
				if (t < (1 / 2.75f))
				{
					return end * (7.5625f * t * t) + start;
				}
				else if (t < (2 / 2.75f))
				{
					t -= (1.5f / 2.75f);
					return end * (7.5625f * (t) * t + .75f) + start;
				}
				else if (t < (2.5 / 2.75))
				{
					t -= (2.25f / 2.75f);
					return end * (7.5625f * (t) * t + .9375f) + start;
				}
				else {
					t -= (2.625f / 2.75f);
					return end * (7.5625f * (t) * t + .984375f) + start;
				}
			}

			public static double InterpolateOutBounce(double start, double end, double t)
			{
				t /= 1d;
				end -= start;
				if (t < (1 / 2.75d))
				{
					return end * (7.5625d * t * t) + start;
				}
				else if (t < (2 / 2.75d))
				{
					t -= (1.5d / 2.75d);
					return end * (7.5625d * (t) * t + .75d) + start;
				}
				else if (t < (2.5 / 2.75))
				{
					t -= (2.25d / 2.75d);
					return end * (7.5625d * (t) * t + .9375d) + start;
				}
				else {
					t -= (2.625d / 2.75d);
					return end * (7.5625d * (t) * t + .984375d) + start;
				}
			}

			public static float InterpolateInOutBounce(float start, float end, float t)
			{
				end -= start;
				float d = 1f;
				if (t < d * 0.5f) return InterpolateOutBounce(0, end, t * 2) * 0.5f + start;
				else return InterpolateOutBounce(0, end, t * 2 - d) * 0.5f + end * 0.5f + start;
			}

			public static double InterpolateInOutBounce(double start, double end, double t)
			{
				end -= start;
				double d = 1d;
				if (t < d * 0.5d) return InterpolateOutBounce(0, end, t * 2) * 0.5d + start;
				else return InterpolateOutBounce(0, end, t * 2 - d) * 0.5d + end * 0.5d + start;
			}
			#endregion
		}
	}
}