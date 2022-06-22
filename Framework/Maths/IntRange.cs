using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Framework
{
	namespace Maths
	{
		[Serializable]
		public struct IntRange
		{
			#region Private Data
			private int _min;
			private int _max;
			#endregion

			#region Public Properties
			public int Min
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

			public int Max
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

			public int Range
			{
				get
				{
					return _max - _min;
				}
			}
			#endregion

			#region Public Interface
			public IntRange(int min, int max)
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

			public static bool operator ==(IntRange a, IntRange b)
			{
				if (ReferenceEquals(a, b))
				{
					return true;
				}

				return a._min == b._min && a._max == b._max;
			}

			public static bool operator !=(IntRange a, IntRange b)
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

				return obj.GetType() == GetType() && this == ((IntRange)obj);
			}

			public override int GetHashCode()
			{
				return _min.GetHashCode() ^ _max.GetHashCode();
			}

			public int Lerp(float t)
			{
				return _min + Mathf.RoundToInt(t * (_max - _min));
			}

			public float Clamp(int value)
			{
				return Math.Clamp(value, _min, _max);
			}

			public bool Contains(int value)
			{
				return _min <= value && value <= _max;
			}

			public int GetRandomValue()
			{
				float t = Random.value;
				return Lerp(t);
			}

			public int GetRandomSignedValue()
			{
				int value = GetRandomValue();
				return Random.value > 0.5f ? value : -value;
			}

			public bool Overlaps(IntRange other)
			{
				return _max <= other._min && _min <= other._max;
			}

			public bool Intersect(IntRange other, out IntRange intersection)
			{
				if (Overlaps(other))
				{
					int min = Math.Max(_min, other._min);
					int max = Math.Min(_max, other._max);
					intersection = new IntRange(min, max);
					return true;
				}

				intersection = default;
				return false;
			}
			#endregion
		}
	}
}