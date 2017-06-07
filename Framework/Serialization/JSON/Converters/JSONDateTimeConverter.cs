using System;


namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(DateTime), "DateTime", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONDateTimeConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				DateTime dateTime = (DateTime)obj;
				node.InnerText = dateTime.ToString();
			}
			
			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;

				return DateTime.Parse(node.InnerText);
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return (DateTime)obj != (DateTime)defaultObj;
			}
			#endregion
		}
	}
}