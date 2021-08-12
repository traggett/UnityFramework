using System;
using System.Collections.Generic;

namespace Framework
{
	namespace Utils
	{
		public static class ArrayUtils
		{
			public static void Add<T>(ref T[] array, T item)
			{
				if (array == null)
				{
					array = new T[1];
					array[0] = item;
				}
				else
				{
					T[] newArray = new T[array.Length + 1];

					if (array.Length > 0)
						Array.Copy(array, newArray, array.Length);

					newArray[array.Length] = item;
					array = newArray;
				}
			}

			public static void Concat<T>(ref T[] array, T[] items)
			{
				if (array == null)
				{
					array = new T[items.Length];
					Array.Copy(items, array, items.Length);
				}
				else
				{
					T[] newArray = new T[array.Length + items.Length];

					if (array.Length > 0)
						Array.Copy(array, newArray, array.Length);

					if (items.Length > 0)
						Array.Copy(items, 0, newArray, array.Length, items.Length);

					array = newArray;
				}
			}
			
			public static void Insert<T>(ref T[] array, T item, int index)
			{
				T[] newArray = new T[array.Length + 1];

				if (index > 0)
					Array.Copy(array, newArray, index);

				newArray[index] = item;

				if (index != array.Length)
					Array.Copy(array, index, newArray, index + 1, array.Length - index);

				array = newArray;
			}

			public static void Remove<T>(ref T[] array, T item)
			{
				int index = Array.IndexOf<T>(array, item);
				if (index != -1)
					RemoveAt<T>(ref array, index);
			}

			public static void RemoveAt<T>(ref T[] array, int index)
			{
				T[] newArray = new T[array.Length - 1];

				if (index > 0)
				{
					Array.Copy(array, newArray, index);
				}
				if (index + 1 < array.Length)
				{
					Array.Copy(array, index + 1, newArray, index, array.Length - 1 - index);
				}

				array = newArray;
			}

			public static void Randomise<T>(T[] array)
			{
				int n = array.Length;
				while (n > 1)
				{
					n--;
					int k = UnityEngine.Random.Range(0, n + 1);
					T value = array[k];
					array[k] = array[n];
					array[n] = value;
				}
			}

			public static int GetCount<T>(IEnumerable<T> enumerable)
			{
				int count = 0;

				foreach (T t in enumerable)
				{
					count++;
				}

				return count;
			}
		}
	}
}