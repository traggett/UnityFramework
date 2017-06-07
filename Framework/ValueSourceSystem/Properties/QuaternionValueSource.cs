using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class QuaternionValueSource : ValueSource<Quaternion>
		{
			public static implicit operator QuaternionValueSource(Quaternion value)
			{
				QuaternionValueSource valueSource = new QuaternionValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}