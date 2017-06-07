

using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(Vector3), "Vector3", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONVector3Converter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				//Add to nodes for x and y
				Vector3 vector = (Vector3)obj;
				JSONConverter.AppendFieldObject(node, vector.x, "x");
				JSONConverter.AppendFieldObject(node, vector.y, "y");
				JSONConverter.AppendFieldObject(node, vector.z, "z");
			}

			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;

				Vector3 vector = (Vector3)obj;

				vector.x = JSONConverter.FieldObjectFromJSONElement<float>(node, "x");
				vector.y = JSONConverter.FieldObjectFromJSONElement<float>(node, "y");
				vector.z = JSONConverter.FieldObjectFromJSONElement<float>(node, "z");

				return vector;
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return (Vector3)obj != (Vector3)defaultObj;
			}
			#endregion
		}
	}
}