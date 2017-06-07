using System.Collections.Generic;

namespace Engine
{
	namespace JSON
	{
		public class JSONObject : JSONElement
		{
			public Dictionary<string, JSONElement> _fields;
		}
	}
}