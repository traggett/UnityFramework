

namespace Engine
{
	using Maths;

	namespace JSON
	{
		[JSONObjectConverter(typeof(FloatRange), "FloatRange", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONFloatRangeConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				//Add to nodes for x and y
				FloatRange floatRange = (FloatRange)obj;
				JSONConverter.AppendFieldObject(node, floatRange._min, "min");
				JSONConverter.AppendFieldObject(node, floatRange._max, "max");
			}
			
			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;

				FloatRange floatRange = (FloatRange)obj;

				floatRange._min = JSONConverter.FieldObjectFromJSONElement<float>(node, "min");
				floatRange._max = JSONConverter.FieldObjectFromJSONElement<float>(node, "max");

				return floatRange;
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return (FloatRange)obj != (FloatRange)defaultObj;
			}
			#endregion
		}
	}
}