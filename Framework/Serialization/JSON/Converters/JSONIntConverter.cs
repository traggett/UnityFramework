namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(int), "Int", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONIntConverter
		{
			#region JSONObjectConverter
			public static JSONElement OnConvertToJSONElement(object obj)
			{
				JSONNumber jsonObj = new JSONNumber();
				jsonObj._value = (int)obj;
				return jsonObj;
			}

			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null || !(node is JSONNumber))
					return obj;

				return (int)((JSONNumber)node)._value;
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return true;
			}
			#endregion
		}
	}
}