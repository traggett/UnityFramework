using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class GameObjectValueSource : ValueSource<GameObject>
		{
			public static implicit operator GameObjectValueSource(GameObject value)
			{
				GameObjectValueSource valueSource = new GameObjectValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}