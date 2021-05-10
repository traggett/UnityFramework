namespace Framework
{
	namespace LocalisationSystem
	{
		public struct LocalisationGlobalVariable
		{
			public readonly string _key;
			public readonly int _version;

			public LocalisationGlobalVariable(string key, int version)
			{
				_key = key;
				_version = version;
			}
		}
	}
}