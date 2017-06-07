namespace Framework
{
	namespace ValueSourceSystem
	{
		public interface IValueSource<T>
		{
			T GetValue();
		}
	}
}