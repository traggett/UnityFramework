using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace Framework
{
	namespace Maths
	{
		[Serializable]
		public struct FloatRange
		{
			public float _min;
			public float _max;

			public FloatRange(float min, float max)
			{
				_min = min;
				_max = max;
			}

			public static bool operator ==(FloatRange a, FloatRange b)
			{
				// If both are null, or both are same instance, return true.
				if (ReferenceEquals(a, b))
				{
					return true;
				}

				// If one is null, but not both, return false.
				if (((object)a == null) || ((object)b == null))
				{
					return false;
				}

				return a._min == b._min && a._max == b._max;
			}

			public static bool operator !=(FloatRange a, FloatRange b)
			{
				// If both are null, or both are same instance, return false.
				if (ReferenceEquals(a, b))
				{
					return false;
				}

				// If one is null, but not both, return true.
				if (((object)a == null) || ((object)b == null))
				{
					return true;
				}

				return a._min != b._min || a._max != b._max;
			}
			
			public override string ToString()
			{
				return "(" + _min + ", " + _max + ")";
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				return obj.GetType() == GetType() && this == ((FloatRange)obj);
			}

			public override int GetHashCode()
			{
				return _min.GetHashCode() ^ _max.GetHashCode();
			}

			public float Get(float t)
			{
				return Mathf.Lerp(_min, _max, t);
			}

			public float GetT(float value)
			{
				return (value - _min) / (_max - _min);
			}

			public float Clamp(float value)
			{
				return Mathf.Clamp(value, _min, _max);
			}

			public float GetClampedT(float value)
			{
				return Mathf.Clamp01(GetT(value));
			}

			public bool Contains(float value)
			{
				return _min <= value && value <= _max;
			}

			public bool Overlaps(FloatRange other)
			{
				return (_min <= other._min && other._min <= _max) || (_min <= other._max && other._max <= _max);
			}

			public float GetRandomValue()
			{
				float t = Random.value;
				return Get(t);
			}

			public float GetRandomSignedValue()
			{
				float value = GetRandomValue();
				return Random.value > 0.5f ? value : -value;
			}
		}
	}
}