namespace Framework
{
	namespace ValueSourceSystem
	{
		public interface IDynamicValueSourceContainer
		{	
			object GetValueSource(int index);

#if UNITY_EDITOR
			int GetNumberOfValueSources();
			string GetValueSourceName(int index);
#endif
		}
	}
}