using System;

namespace Framework
{
	using Utils;

	namespace SaveSystem
	{
		[Serializable]
		public class SaveDataFile
		{
			#region Public Data
			public SaveDataBlock[] _dataBlocks;
			#endregion

			#region Save Data Blocks
			public T Get<T>() where T : SaveDataBlock, new()
			{
				//Find existing blocks
				if (_dataBlocks != null)
				{
					foreach (SaveDataBlock data in _dataBlocks)
					{
						if (data.GetType() == typeof(T))
						{
							return data as T;
						}
					}
				}

				//None found in save file, add new block and return it
				T newData = new T();
				ArrayUtils.Add(ref _dataBlocks, newData);

				return newData;
			}

			public SaveDataBlock GetByType(Type type)
			{
				//Find existing blocks
				foreach (SaveDataBlock data in _dataBlocks)
				{
					if (data.GetType() == type)
					{
						return data;
					}
				}

				//None found in save file, add new block and return it
				SaveDataBlock newData = Activator.CreateInstance(type) as SaveDataBlock;
				ArrayUtils.Add(ref _dataBlocks, newData);

				return newData;
			}
			#endregion
		}
	}
}