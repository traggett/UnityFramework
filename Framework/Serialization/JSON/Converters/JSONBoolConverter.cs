using System;


namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(bool), "Bool", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONBoolConverter
		{
			#region JSONObjectConverter
			public static JSONElement OnConvertToJSONElement(object obj)
			{
				JSONBool jsonObj = new JSONBool();
				jsonObj._value = (bool)obj;
				return jsonObj;
			}
			
			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null || !(node is JSONBool))
					return obj;

				return ((JSONBool)node)._value;
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return true;
			}
			#endregion
		}
	}
}