namespace Framework
{
	namespace DynamicValueSystem
	{
		public interface IValueSourceContainer
		{
			//Returns an IValueSource<>
			object GetValueSource(int index);

#if UNITY_EDITOR
			//Returns the number of IValueSource<> currently available on this object
			int GetNumberOfValueSource();
			//Returns the number of IValueSource<> currently available on this object
			string GetValueSourceName(int index);
#endif
		}
	}
}