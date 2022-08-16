using System;
using System.Collections.Generic;
using System.Linq;
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
			private class ItemData
			{
				public T _item;
				public int _pickCount;
				public float _chance;
			}
			private Dictionary<T, ItemData> _items = new Dictionary<T, ItemData>();
			private float _bias = 0f;
			private bool _dontRepeat = false;
			#endregion

			#region Public Interface
			public RandomPicker(params T[] items)
			{
				_items = new Dictionary<T, ItemData>(items.Length);

				for (int i=0; i< items.Length; i++)
				{
					Add(items[i]);
				}
			}

			public void Add(T item)
			{
				if (!Contains(item))
				{
					_items[item] = new ItemData()
					{
						_item = item,
						_pickCount = 0,
					};
				}
			}

			public bool Contains(T item)
			{
				return _items.ContainsKey(item);
			}

			public void Remove(T item)
			{
				_items.Remove(item);
			}

			public void Reset()
			{
				foreach (var keyPair in _items)
				{
					keyPair.Value._pickCount = 0;
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

			public T PickRandomFrom(params T[] items)
			{
				if (items.Length == 0)
					throw new Exception();

				//Add all items
				for (int i = 0; i < items.Length; i++)
				{
					Add(items[i]);
				}

				if (items.Length == 1)
				{
					ItemData itemData = _items[items[0]];
					itemData._pickCount++;
					return items[0];
				}
				else
				{
					//Reset chance based on bias
					CalcualateChances(items);

					if (_dontRepeat)
					{
						DontAllowRepeats(items);
					}

					//Pick based on chance
					return PickFromChance(items);
				}			
			}

			public T PickRandom()
			{
				T[] allItems = _items.Keys.ToArray();

				return PickRandomFrom(allItems);
			}
			#endregion

			#region Private Functions
			private void CalcualateChances(T[] items)
			{
				for (int i = 0; i < items.Length; i++)
				{
					ItemData itemData = _items[items[i]];

					if (_bias > 0f && itemData._pickCount > 0)
					{
						itemData._chance = Mathf.Lerp(1f, 1f / itemData._pickCount, _bias);
					}
					else
					{
						itemData._chance = 1f;
					}
				}
			}

			private void DontAllowRepeats(T[] items)
			{
				//Find loweset pick count
				int lowestCount = GetLowestPickCount(items);

				//Set all items that have been picked more than this to zero
				for (int i = 0; i < items.Length; i++)
				{
					ItemData itemData = _items[items[i]];

					if (itemData._pickCount > lowestCount)
					{
						itemData._chance = 0f;
					}
				}
			}

			private int GetLowestPickCount(T[] items)
			{
				int count = int.MaxValue;

				for (int i = 0; i < items.Length; i++)
				{
					ItemData itemData = _items[items[i]];
					count = Math.Min(count, itemData._pickCount);
				}

				return count;
			}

			private T PickFromChance(T[] items)
			{
				float totalChance = 0f;

				for (int i = 0; i < items.Length; i++)
				{
					ItemData itemData = _items[items[i]];

					if (itemData._chance > 0f)
					{
						totalChance += itemData._chance;
					}
				}

				float randomValue = Random.value * totalChance;
				float cumulative = 0f;

				for (int i = 0; i < items.Length; i++)
				{
					ItemData itemData = _items[items[i]];

					if (itemData._chance > 0f)
					{
						cumulative += itemData._chance;

						if (randomValue < cumulative)
						{
							itemData._pickCount++;
							return items[i];
						}
					}
				}

				throw new Exception();
			}
			#endregion
		}
	}
}
