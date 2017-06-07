

using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(Vector2), "Vector2", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONVector2Converter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				//Add to nodes for x and y
				Vector2 vector = (Vector2)obj;
				JSONConverter.AppendFieldObject(node, vector.x, "x");
				JSONConverter.AppendFieldObject(node, vector.y, "y");
			}

			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;

				Vector2 vector = (Vector2)obj;
				
				vector.x = JSONConverter.FieldObjectFromJSONElement<float>(node, "x");
				vector.y = JSONConverter.FieldObjectFromJSONElement<float>(node, "y");

				return vector;
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return (Vector2)obj != (Vector2)defaultObj;
			}
			#endregion
		}
	}
}