using System;
using UnityEngine;
namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public struct TimeProperty
		{
			#region Serialized Data
			[SerializeField]
			private double _time;	
			#endregion

			public TimeProperty(double time = 0d)
			{
				_time = time;
			}

			public static implicit operator double(TimeProperty property)
			{
				return property._time;
			}
			
			public static implicit operator TimeProperty(int time)
			{
				return new TimeProperty(time);
			}
		}
	}
}