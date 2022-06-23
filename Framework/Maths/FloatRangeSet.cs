using Framework.Utils;
using UnityEngine;

namespace Framework
{
	namespace Maths
	{
		public class FloatRangeSet 
		{
			#region Private Data
			private FloatRange[] _ranges = new FloatRange[0];
			#endregion

			#region Public Properties
			public float Range
			{
				get
				{
					float totalRange = 0f;

					for (int i = 0; i < _ranges.Length; i++)
					{
						totalRange += _ranges[i].Range;
					}

					return totalRange;
				}
			}
			#endregion

			#region Public Interface
			public void Add(FloatRange range)
			{
				for (int i=0; i<_ranges.Length; i++)
				{
					//If this ranges max value is less then array min, break
					if (range.Max < _ranges[i].Min)
					{
						break;
					}

					//If range is totally inside other range then dont need to change anything
					if (_ranges[i].Min <= range.Min && range.Max <= _ranges[i].Max)
					{
						return;
					}
					//If array range is totally inside new range then replace it with this (update array range with new min, clip range by its max)
					else if (range.Min <= _ranges[i].Min && _ranges[i].Max <= range.Max)
					{
						_ranges[i].Min = range.Min;
						range.Min = _ranges[i].Max;
					}
					//If range starts from within the array range, remove array range, set start of this range to start of array range
					else if (_ranges[i].Min <= range.Min && range.Min <= _ranges[i].Max)
					{
						range.Min = _ranges[i].Min;
						ArrayUtils.RemoveAt(ref _ranges, i);
						i--;
					}
					//If range ends from within the array range, remove array range, set start of this range to start of array range
					else if (_ranges[i].Min <= range.Max && range.Max <= _ranges[i].Max)
					{
						range.Max = _ranges[i].Max;
						ArrayUtils.RemoveAt(ref _ranges, i);
						i--;
					}
					//Otherwise subtract array range from new range
					else
					{
						if (range.Min > _ranges[i].Min && range.Min < _ranges[i].Max)
						{
							range.Min = _ranges[i].Max;
						}

						if (range.Max > _ranges[i].Min && range.Max < _ranges[i].Max)
						{
							range.Max = _ranges[i].Min;
						}
					}
				}

				//If range is still valid add to array (in sorted order by range min)
				if (range.Range > 0f)
				{
					int index;

					for (index = 0; index < _ranges.Length; index++)
					{
						if (range.Min < _ranges[index].Min)
							break;
					}

					ArrayUtils.Insert(ref _ranges, range, index);
				}
			}

			public void Subtract(FloatRange range)
			{
				for (int i = 0; i < _ranges.Length; i++)
				{
					//If this ranges max value is less then current min, break
					if (range.Max < _ranges[i].Min)
					{
						break;
					}

					//If range is totally inside array range 
					if (_ranges[i].Min <= range.Min && range.Max <= _ranges[i].Max)
					{
						//Need to split array range into two (subtract range from it)
						FloatRange rangeA = new FloatRange(_ranges[i].Min, range.Min);
						FloatRange rangeB = new FloatRange(range.Max, _ranges[i].Max);

						ArrayUtils.RemoveAt(ref _ranges, i);

						if (rangeB.Range > 0f)
							ArrayUtils.Insert(ref _ranges, rangeB, i);

						if (rangeA.Range > 0f)
							ArrayUtils.Insert(ref _ranges, rangeA, i);

						return;
					}
					//If array range is totally inside range then remove it
					else if (range.Min <= _ranges[i].Min && _ranges[i].Max <= range.Max)
					{
						ArrayUtils.RemoveAt(ref _ranges, i);
						i--;
					}
					//Otherwise subtract range from range in array
					else
					{
						if (_ranges[i].Min <= range.Min && range.Min <= _ranges[i].Max)
						{
							_ranges[i].Max = range.Min;
						}

						if (_ranges[i].Min <= range.Max && range.Max <= _ranges[i].Max)
						{
							_ranges[i].Min = range.Max;
						}
					}
				}
			}

			public float GetRandomValue()
			{
				float randomValue = Random.value * Range;
				float value = 0f;

				for (int i = 0; i < _ranges.Length; i++)
				{
					float rangeT = (randomValue - value) / _ranges[i].Range;

					if (rangeT <= 1f)
					{
						return _ranges[i].Lerp(rangeT);
					}

					value += _ranges[i].Range;
				}

				return 0f;
			}
			#endregion
		}
	}
}