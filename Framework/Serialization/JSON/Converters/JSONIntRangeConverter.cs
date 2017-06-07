

namespace Engine
{
	using Maths;

	namespace JSON
	{
		[JSONObjectConverter(typeof(IntRange), "IntRange", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONIntRangeConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				//Add to nodes for x and y
				IntRange intRange = (IntRange)obj;
				JSONConverter.AppendFieldObject(node, intRange._min, "min");
				JSONConverter.AppendFieldObject(node, intRange._max, "max");
			}
			
			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;

				IntRange intRange = (IntRange)obj;

				intRange._min = JSONConverter.FieldObjectFromJSONElement<int>(node, "min");
				intRange._max = JSONConverter.FieldObjectFromJSONElement<int>(node, "max");

				return intRange;
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return (IntRange)obj != (IntRange)defaultObj;
			}
			#endregion
		}
	}
}