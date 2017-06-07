

using UnityEngine;

namespace Engine
{
	using Utils;

	namespace JSON
	{
		[JSONObjectConverter(typeof(Color), "Color", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONColorConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				Color color = (Color)obj;
				node.InnerText = StringUtils.ColorToHex(color);
			}
			
			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;
				
				return StringUtils.HexToColor(node.InnerText);
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return (Color)obj != (Color)defaultObj;
			}
			#endregion
		}
	}
}