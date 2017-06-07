using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class ComponentValueSource : ValueSource<Component>
		{
			public static implicit operator ComponentValueSource(Component value)
			{
				ComponentValueSource valueSource = new ComponentValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}