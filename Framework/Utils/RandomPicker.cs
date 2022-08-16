using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Framework
{
	namespace Utils
	{
		public class RandomPicker<T>
		{
			#region Public Data
			/// <summary>
			/// If set you won't get the same item twice until all items have been picked.
			/// </summary>
			public bool DontRepeat
			{
				get
				{
					return _dontRepeat;
				}
				set
				{
					_dontRepeat = value;
				}
			}

			/// <summary>
			/// The bias is a weighting to pick things that haven't been picked before.
			/// E.g. there are two options, Option A has been picked four times already, Option B, none.
			/// A bias of 0 means they will be as likely as each other, a bias of 1 means 
			/// Option A will be four times less likely to be picked.
			/// </summary>
			public float Bias
			{
				get
				{
					return _bias;
				}
				set
				{
					_bias = value;
				}
			}
			#endregion

			#region Private Data		
			private struct ItemData
			{
				public T _item;
				public int _pickCount;
				public float _chance;
			}
			private ItemData[] _array;
			private float _bias = 0f;
			private bool _dontRepeat = false;
			#endregion

			#region Public Interface
			public RandomPicker(T[] array)
			{
				_array = new ItemData[array.Length];

				for (int i=0; i<_array.Length; i++)
				{
					_array[i]._item = array[i];
					_array[i]._pickCount = 0;
				}
			}

			public void Add(T item)
			{
				ItemData itemData = new ItemData()
				{
					_item = item,
					_pickCount = 0,
				};

				ArrayUtils.Add(ref _array, itemData);
			}

			public bool Contains(T item)
			{
				for (int i = 0; i < _array.Length; i++)
				{
					if (_array[i]._item.Equals(item))
					{
						return true;
					}
				}

				return false;
			}

			public void Remove(T item)
			{
				for (int i = 0; i < _array.Length; i++)
				{
					if (_array[i]._item.Equals(item))
					{
						ArrayUtils.RemoveAt(ref _array, i);
						break;
					}
				}
			}

			public void Reset()
			{
				for (int i = 0; i < _array.Length; i++)
				{
					_array[i]._pickCount = 0;
				}
			}

			public static RandomPicker<int> CreateIndexPicker(T[] array)
			{
				int[] indexes = new int[array.Length];

				for (int i = 0; i < array.Length; i++)
				{
					indexes[i] = i;
				}

				return new RandomPicker<int>(indexes);
			}

			public T PickRandom()
			{
				//Reset chance based on bias
				CalcualateChances();

				if (_dontRepeat)
				{
					DontAllowRepeats();
				}

				//Pick based on chance
				int index = PickFromChance();

				if (index != -1)
				{
					_array[index]._pickCount++;
					return _array[index]._item;
				}

				throw new Exception();
			}
			#endregion

			#region Private Functions
			private void CalcualateChances()
			{
				for (int i = 0; i < _array.Length; i++)
				{
					if (_bias > 0f)
					{
						_array[i]._chance = Mathf.Lerp(1f, 1f / _array[i]._pickCount, _bias);
					}
					else
					{
						_array[i]._chance = 1f;
					}
				}
			}

			private void DontAllowRepeats()
			{
				//Find loweset pick count
				int lowestCount = GetLowestPickCount();

				//Set all items that have been picked more than this to zero
				for (int i = 0; i < _array.Length; i++)
				{
					if (_array[i]._pickCount > lowestCount)
					{
						_array[i]._chance = 0f;
					}
				}
			}

			private int GetLowestPickCount()
			{
				int count = int.MaxValue;

				for (int i = 0; i < _array.Length; i++)
				{
					count = Math.Min(count, _array[i]._pickCount);
				}

				return count;
			}

			private int PickFromChance()
			{
				float totalChance = 0f;

				for (int i = 0; i < _array.Length; i++)
				{
					if (_array[i]._chance > 0f)
					{
						totalChance += _array[i]._chance;
					}
				}

				float randomValue = Random.value * totalChance;
				float cumulative = 0f;

				for (int i = 0; i < _array.Length; i++)
				{
					if (_array[i]._chance > 0f)
					{
						cumulative += _array[i]._chance;

						if (randomValue < cumulative)
						{
							return i;
						}
					}
				}

				return _array.Length - 1;
			}
			#endregion
		}
	}
}
