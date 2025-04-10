using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public class RandomPicker<T> : ISerializationCallbackReceiver
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

			#region Serialised Data
			[SerializeField][Range(0f, 1)] private float _bias;
			[SerializeField] private bool _dontRepeat;
			[SerializeField] private List<T> _items = new List<T>();
			#endregion

			#region Private Data		
			private class ItemData
			{
				public T _item;
				public int _pickCount;
				public float _weight;
				public float _chance;
			}
			private Dictionary<T, ItemData> _itemData = new Dictionary<T, ItemData>();
			#endregion

			#region Public Interface
			public RandomPicker(params T[] items)
			{
				_itemData = new Dictionary<T, ItemData>(items.Length);

				for (int i=0; i< items.Length; i++)
				{
					Add(items[i]);
				}
			}
			
			public RandomPicker(IEnumerable<T> items)
			{
				_itemData = new Dictionary<T, ItemData>();

				foreach (T item in items)
				{
					Add(item);
				}
			}

			public void Add(T item)
			{
				if (!Contains(item))
				{
					_itemData[item] = new ItemData()
					{
						_item = item,
						_pickCount = 0,
						_weight = 1f,
					};
				}
			}

			public bool Contains(T item)
			{
				return _itemData.ContainsKey(item);
			}

			public void Remove(T item)
			{
				_itemData.Remove(item);
			}

			public void Reset()
			{
				foreach (var keyPair in _itemData)
				{
					keyPair.Value._pickCount = 0;
				}
			}

			public T PickRandomFrom(IEnumerable<T> items)
			{
				int count = 0;

				foreach (T item in items)
				{
					Add(item);
					count++;
				}

				if (count <= 1)
				{
					IEnumerator<T> enumerator = items.GetEnumerator();

					if (enumerator.MoveNext())
					{
						T item = enumerator.Current;
						ItemData itemData = _itemData[item];
						itemData._pickCount++;
						return item;
					}

					return default;
				}
				else
				{
					//Reset chance based on bias
					CalcualateChances(items);

					//Pick based on chance
					return PickFromChance(items);
				}
			}

			public T PickRandom()
			{
				return PickRandomFrom(_itemData.Keys);
			}

			public bool SetWeight(T item, float weight)
			{
				if (_itemData.TryGetValue(item, out ItemData itemData))
				{
					itemData._weight = weight;
					return true;
				}

				return false;
			}
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{


			}

			public void OnAfterDeserialize()
			{
				_itemData = new Dictionary<T, ItemData>(_items.Count);

				foreach (T item in _items)
				{
					Add(item);
				}
			}
			#endregion

			#region Private Functions
			private void CalcualateChances(IEnumerable<T> items)
			{
				GetItemsData(items, out float totalPicks, out int lowestPickCount, out float totalWeight);

				bool applyBias = _bias > 0f && totalPicks > 0 && totalWeight > 0f;

				foreach (T item in items)
				{
					ItemData itemData = _itemData[item];

					if (_dontRepeat && itemData._pickCount > lowestPickCount)
					{
						itemData._chance = 0f;
					}
					else
					{
						if (applyBias && totalPicks > 0 && itemData._pickCount > 0)
						{
							float actualFractionPicked = itemData._pickCount / totalPicks;
							float expectedFractionPicked = itemData._weight / totalWeight;
							float biasedChance = expectedFractionPicked / actualFractionPicked;

							itemData._chance = Mathf.Lerp(itemData._weight, itemData._weight * biasedChance, _bias);
						}
						else
						{
							itemData._chance = itemData._weight;
						}
					}
				}
			}

			private void GetItemsData(IEnumerable<T> items, out float totalPicks, out int lowestPicks, out float totalWeight)
			{
				totalPicks = 0f;
				totalWeight = 0f;
				lowestPicks = int.MaxValue;

				foreach (T item in items)
				{
					ItemData itemData = _itemData[item];

					totalPicks += itemData._pickCount;
					totalWeight += itemData._weight;

					lowestPicks = Math.Min(lowestPicks, itemData._pickCount);
				}
			}

			private T PickFromChance(IEnumerable<T> items)
			{
				float totalChance = 0f;

				foreach (T item in items)
				{
					ItemData itemData = _itemData[item];

					if (itemData._chance > 0f)
					{
						totalChance += itemData._chance;
					}
				}

				if (totalChance > 0f)
				{
					float randomValue = Random.value * totalChance;
					float cumulative = 0f;

					foreach (T item in items)
					{
						ItemData itemData = _itemData[item];

						if (itemData._chance > 0f)
						{
							cumulative += itemData._chance;

							if (randomValue <= cumulative)
							{
								itemData._pickCount++;
								return item;
							}
						}
					}
				}

				return default;
			}
			#endregion
		}
	}
}
