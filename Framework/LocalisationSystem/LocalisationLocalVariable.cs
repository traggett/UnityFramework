namespace Framework
{
	namespace LocalisationSystem
	{
		public struct LocalisationLocalVariable
		{
			public string _key;
			public string _value;

			public LocalisationLocalVariable(string key, string value)
			{
				_key = key;
				_value = value;
			}
			
			public override string ToString()
			{
				return _key + ":" + _value;
			}
		}
	}
}