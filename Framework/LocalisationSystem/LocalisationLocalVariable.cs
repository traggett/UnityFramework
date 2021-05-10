namespace Framework
{
	namespace LocalisationSystem
	{
		public struct LocalisationLocalVariable
		{
			public readonly string _key;
			public readonly string _value;
			public readonly bool _localised;

			public LocalisationLocalVariable(string key, string value, bool localised = false)
			{
				_key = key;
				_value = value;
				_localised = localised;
			}
			
			public override string ToString()
			{
				return _key + ":" + _value;
			}
		}
	}
}