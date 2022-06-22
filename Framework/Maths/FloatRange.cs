using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Framework
{
	namespace Maths
	{
		[Serializable]
		public struct FloatRange
		{
			#region Private Data
			private float _min;
			private float _max;
			#endregion

			#region Public Properties
			public float Min
			{
				get
				{
					return _min;
				}
				set
				{
					if (value > _max)
						_max = value;

					_min = value;
				}
			}

			public float Max
			{
				get
				{
					return _max;
				}
				set
				{
					if (value < _min)
						_min = value;

					_max = value;
				}
			}

			public float Range
			{
				get
				{
					return _max - _min;
				}
			}
			#endregion

			#region Public Interface
			public FloatRange(float min, float max)
			{
				if (min > max)
				{
					_min = max;
					_max = min;
				}
				else
				{
					_min = min;
					_max = max;
				}
			}
			public static bool operator ==(FloatRange a, FloatRange b)
			{
				if (ReferenceEquals(a, b))
				{
					return true;
				}

				return a._min == b._min && a._max == b._max;
			}

			public static bool operator !=(FloatRange a, FloatRange b)
			{
				if (ReferenceEquals(a, b))
				{
					return false;
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

			public float Lerp(float t)
			{
				return Mathf.Lerp(_min, _max, t);
			}

			public float Clamp(float value)
			{
				return Mathf.Clamp(value, _min, _max);
			}

			public bool Contains(float value)
			{
				return _min <= value && value <= _max;
			}

			public float GetRandomValue()
			{
				float t = Random.value;
				return Lerp(t);
			}

			public float GetRandomSignedValue()
			{
				float value = GetRandomValue();
				return Random.value > 0.5f ? value : -value;
			}

			public bool Overlaps(FloatRange other)
			{
				return _max <= other._min && _min <= other._max;
			}

			public bool Intersect(FloatRange other, out FloatRange intersection)
			{
				if (Overlaps(other))
				{
					float min = Mathf.Max(_min, other._min);
					float max = Mathf.Min(_max, other._max);
					intersection = new FloatRange(min, max);
					return true;
				}

				intersection = default;
				return false;
			}
			#endregion
		}
	}
}