

namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(string), "String", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONStringConverter
		{
			#region JSONObjectConverter
			public static JSONElement OnConvertToJSONElement(object obj)
			{
				JSONString jsonObj = new JSONString();
				jsonObj._value = (string)obj;
				return jsonObj;
			}

			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null || !(node is JSONString))
					return obj;

				return ((JSONString)node)._value;
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return true;
			}
			#endregion
		}
	}
}