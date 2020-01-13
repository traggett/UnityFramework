using System;
using UnityEngine;

namespace Framework
{
	namespace Maths
	{
		public static class MathUtils
		{
			#region Easing
			public static float GetLerp(float from, float to, float value)
			{
				if (value <= from)
					return 0.0f;
				if (value >= to)
					return 1.0f;

				return (value - from) / (to - from);
			}

			public static float GetUnclampedLerp(float from, float to, float value)
			{
				return (value - from) / (to - from);
			}

			public static Rect Lerp(Rect from, Rect to, float t)
			{
				Rect lerp = from;
				lerp.x = Mathf.Lerp(from.x, to.x, t);
				lerp.y = Mathf.Lerp(from.y, to.y, t);
				lerp.width = Mathf.Lerp(from.width, to.width, t);
				lerp.height = Mathf.Lerp(from.height, to.height, t);
				return lerp;
			}

			public static FloatRange Lerp(FloatRange from, FloatRange to, float t)
			{
				FloatRange lerp = from;
				lerp._min = Mathf.Lerp(from._min, to._min, t);
				lerp._max = Mathf.Lerp(from._max, to._max, t);
				return lerp;
			}

			public static float Interpolate(InterpolationType type, float start, float end, float t)
			{
				return Interpolation.Interpolate(type, start, end, t);
			}

			public static double Interpolate(InterpolationType type, double start, double end, double t)
			{
				return Interpolation.Interpolate(type, start, end, t);
			}

			public static Vector2 Interpolate(InterpolationType type, Vector2 from, Vector2 to, float t)
			{
				float lerp = Interpolation.Interpolate(type, 0.0f, 1.0f, t);
				return Vector2.Lerp(from, to, lerp);
			}

			public static Vector3 Interpolate(InterpolationType type, Vector3 from, Vector3 to, float t)
			{
				float lerp = Interpolation.Interpolate(type, 0.0f, 1.0f, t);
				return Vector3.Lerp(from, to, lerp);
			}

			public static Quaternion Interpolate(InterpolationType type, Quaternion from, Quaternion to, float t)
			{
				float lerp = Interpolation.Interpolate(type, 0.0f, 1.0f, t);
				return Quaternion.Slerp(from, to, lerp);
			}

			public static Color Interpolate(InterpolationType type, Color from, Color to, float t)
			{
				float lerp = Interpolation.Interpolate(type, 0.0f, 1.0f, t);
				return Color.Lerp(from, to, lerp);
			}

			public static Rect Interpolate(InterpolationType type, Rect from, Rect to, float t)
			{
				float lerp = Interpolation.Interpolate(type, 0.0f, 1.0f, t);
				return Lerp(from, to, lerp);
			}

			public static FloatRange Interpolate(InterpolationType type, FloatRange from, FloatRange to, float t)
			{
				float lerp = Interpolation.Interpolate(type, 0.0f, 1.0f, t);
				return Lerp(from, to, lerp);
			}

			public static float Damp(float a, float b, float lambda, float deltaTime, InterpolationType type = InterpolationType.Linear)
			{
				return Interpolation.Interpolate(type, a, b, 1.0f - Mathf.Exp(-lambda * deltaTime));
			}

			public static double Damp(double a, double b, double lambda, double deltaTime, InterpolationType type = InterpolationType.Linear)
			{
				return Interpolation.Interpolate(type, a, b, 1.0d - Math.Exp(-lambda * deltaTime));
			}

			public static Vector2 Damp(Vector2 a, Vector2 b, float lambda, float deltaTime, InterpolationType type = InterpolationType.Linear)
			{
				float lerp = Damp(0.0f, 1.0f, lambda, deltaTime, type);
				return Vector2.Lerp(a, b, lerp);
			}

			public static Vector3 Damp(Vector3 a, Vector3 b, float lambda, float deltaTime, InterpolationType type = InterpolationType.Linear)
			{
				float lerp = Damp(0.0f, 1.0f, lambda, deltaTime, type);
				return Vector3.Lerp(a, b, lerp);
			}

			public static Quaternion Damp(Quaternion a, Quaternion b, float lambda, float deltaTime, InterpolationType type = InterpolationType.Linear)
			{
				float lerp = Damp(0.0f, 1.0f, lambda, deltaTime, type);
				return Quaternion.Slerp(a, b, lerp);
			}
			#endregion

			public static int RandomSign()
			{
				return UnityEngine.Random.Range(0, 1) == 0 ? 1 : -1;
			}

			public static bool AlmostZero(Vector3 vec)
			{
				return Mathf.Approximately(vec.x, 0.0f) && Mathf.Approximately(vec.y, 0.0f) && Mathf.Approximately(vec.z, 0.0f);
			}

			public static float Sqrd(float num)
			{
				return num * num;
			}

			public static float Pow(float num, int exp)
			{
				float result = 1.0f;
				while (exp > 0)
				{
					if (exp % 2 == 1)
						result *= num;
					exp >>= 1;
					num *= num;
				}

				return result;
			}

			public static bool FloatRangeIntersects(float range1start, float range1end, float range2start, float range2end)
			{
				if (range1start <= range2end)
				{
					return (range1end >= range2start);
				}
				else if (range1end >= range2start)
				{
					return (range1start <= range2end);
				}

				return false;
			}

			public static float AngleBetween(Vector2 a, Vector2 b)
			{
				float ang = Vector2.Angle(a, b);
				Vector3 cross = Vector3.Cross(a, b);

				if (cross.z > 0)
					ang = 360 - ang;

				return DegreesTo180Range(ang);
			}

			public static Vector2 Cross(Vector2 dir)
			{
				return new Vector2(dir.y, -dir.x);
			}

			public static Vector2 Rotate(Vector2 v, float degrees)
			{
				float radians = degrees * Mathf.Deg2Rad;
				float sin = Mathf.Sin(radians);
				float cos = Mathf.Cos(radians);

				float tx = v.x;
				float ty = v.y;

				return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
			}

			public static float DegreesTo180Range(float angle)
			{
				angle %= 360.0f;

				if (angle > 180.0f)
					angle -= 360.0f;
				else if (angle < -180.0f)
					angle += 360.0f;

				return angle;
			}

			public static float AngleDiff(float a, float b)
			{
				float angleDiff = a - b;
				float invAngle = 360.0f - angleDiff;

				if (invAngle < angleDiff)
				{
					angleDiff = invAngle;
				}

				return angleDiff;
			}

			public static Vector3 GetClosestPointToLine(Vector3 start, Vector3 end, Vector3 point)
			{
				Vector3 line = (end - start);
				float len = line.magnitude;
				line.Normalize();

				Vector3 v = point - start;
				float d = Vector3.Dot(v, line);
				d = Mathf.Clamp(d, 0f, len);

				return start + line * d;
			}

			public static float SignedAngleBetween(Vector2 a, Vector2 b)
			{
				float perpDot = (a.x * b.y) - (a.y * b.x);
				return Mathf.Atan2(perpDot, Vector2.Dot(a, b));
			}

			public static Vector2 To2DVector(Vector3 vector)
			{
				return new Vector2(vector.x, vector.z);
			}

			public static Vector2 ClampToUnit(Vector2 vector)
			{
				float lengthSqr = vector.sqrMagnitude;

				if (lengthSqr > 1.0f)
					return (vector / Mathf.Sqrt(lengthSqr));

				return vector;
			}
			
			public static Vector3 GetTranslationFromMatrix(ref Matrix4x4 matrix)
			{
				Vector3 translate;
				translate.x = matrix.m03;
				translate.y = matrix.m13;
				translate.z = matrix.m23;
				return translate;
			}
			
			public static Quaternion GetRotationFromMatrix(ref Matrix4x4 matrix)
			{
				Vector3 forward;
				forward.x = matrix.m02;
				forward.y = matrix.m12;
				forward.z = matrix.m22;

				Vector3 upwards;
				upwards.x = matrix.m01;
				upwards.y = matrix.m11;
				upwards.z = matrix.m21;

				return Quaternion.LookRotation(forward, upwards);
			}
			
			public static Vector3 GetScaleFromMatrix(ref Matrix4x4 matrix)
			{
				Vector3 scale;
				scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
				scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
				scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
				return scale;
			}
			
			public static bool ClosestPointsOnTwoLines(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, out float closestPointLine1T, out float closestPointLine2T)
			{
				closestPointLine1T = 0.0f;
				closestPointLine2T = 0.0f;

				float a = Vector3.Dot(lineVec1, lineVec1);
				float b = Vector3.Dot(lineVec1, lineVec2);
				float e = Vector3.Dot(lineVec2, lineVec2);

				float d = a * e - b * b;

				//lines are not parallel
				if (d != 0.0f)
				{
					Vector3 r = linePoint1 - linePoint2;
					float c = Vector3.Dot(lineVec1, r);
					float f = Vector3.Dot(lineVec2, r);

					float s = (b * f - c * e) / d;
					float t = (a * f - c * b) / d;

					closestPointLine1T = s;
					closestPointLine2T = t;

					return true;
				}

				else
				{
					return false;
				}
			}

			public static bool IntercetionOfTwoNormalisedLines(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, out Vector3 intercetion)
			{
				float line1DirDotLine2Dir = Vector3.Dot(lineVec1, lineVec2);

				float d = 1.0f - line1DirDotLine2Dir * line1DirDotLine2Dir;

				//lines are not parallel
				if (d != 0.0f)
				{
					Vector3 r = linePoint1 - linePoint2;
					float c = Vector3.Dot(lineVec1, r);
					float f = Vector3.Dot(lineVec2, r);
					float s = (line1DirDotLine2Dir * f - c) / d;

					intercetion = linePoint1 + lineVec1 * s;

					return true;
				}

				else
				{
					intercetion = Vector3.zero;

					return false;
				}
			}

			public static bool Approximately(float a, float b, float epsilon)
			{
				float absA = Mathf.Abs(a);
				float absB = Mathf.Abs(b);
				float diff = Mathf.Abs(a - b);
				
				if (a == b)
				{ // shortcut, handles infinities
					return true;
				}
				else if (a == 0 || b == 0 || diff < float.Epsilon)
				{
					// a or b is zero or both are extremely close to it
					// relative error is less meaningful here
					return diff < (epsilon * float.Epsilon);
				}
				else
				{ // use relative error
					return diff / Mathf.Min((absA + absB), float.Epsilon) < epsilon;
				}
			}

			public static bool Is2DPointInsideConvexPoly(Vector2[] polyPoints, Vector2 point)
			{
				if (polyPoints.Length < 3)
					return false;

				float lastLineSign = 0.0f;

				for (int i = 0; i < polyPoints.Length; i++)
				{
					//Work out which side of the line the point is, right (positive) or left (negative)
					// (y - y0) (x1 - x0) - (x - x0) (y1 - y0)
					int j = (i + 1) % polyPoints.Length;

					float lineSide = ((point.y - polyPoints[i].y) * (polyPoints[j].x - polyPoints[i].x)) - ((point.x - polyPoints[i].x) * (polyPoints[j].y - polyPoints[i].y));
					
					//if on a different side to previous lines, not in poly
					if (i > 0 && ((lastLineSign <= 0.0f && lineSide > 0.0f) || (lastLineSign >= 0.0f && lineSide < 0.0f)))
					{
						return false;
					}

					lastLineSign = lineSide;
				}

				return true;
			}

			public static bool IsSphereInFrustrum(ref Plane[] frustrumPlanes, ref Vector3 center, float radius, float frustumPadding = 0f)
			{
				for (int i = 0; i < frustrumPlanes.Length; i++)
				{
					Vector3 normal = frustrumPlanes[i].normal;
					float distance = frustrumPlanes[i].distance;

					float dist = normal.x * center.x + normal.y * center.y + normal.z * center.z + distance;

					if (dist < -radius - frustumPadding)
					{
						return false;
					}
				}

				return true;
			}

			public static Vector3 GetPosition(ref Matrix4x4 matrix)
			{
				return new Vector3(matrix.m03, matrix.m13, matrix.m23);
			}

			public static void SetPosition(ref Matrix4x4 matrix, Vector3 position)
			{
				matrix.m03 = position.x;
				matrix.m13 = position.y;
				matrix.m23 = position.z;
			}

			public static Vector3 GetForward(ref Matrix4x4 matrix)
			{
				return matrix.GetColumn(2);
			}

			public static Vector3 GetUp(ref Matrix4x4 matrix)
			{
				return matrix.GetColumn(1);
			}
		}
	}
}