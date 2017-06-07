namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(float), "Float", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONFloatConverter
		{
			#region JSONObjectConverter
			public static JSONElement OnConvertToJSONElement(object obj)
			{
				JSONNumber jsonObj = new JSONNumber();
				jsonObj._value = (float)obj;
				return jsonObj;
			}

			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null || !(node is JSONNumber))
					return obj;

				return ((JSONNumber)node)._value;
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return true;
			}
			#endregion
		}
	}
}