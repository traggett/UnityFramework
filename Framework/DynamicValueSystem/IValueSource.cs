namespace Framework
{
	namespace DynamicValueSystem
	{
		public interface IValueSource<T>
		{
			T GetValue();
		}
	}
}