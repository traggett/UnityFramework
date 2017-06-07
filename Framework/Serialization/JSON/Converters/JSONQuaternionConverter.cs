

using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(Quaternion), "Quaternion", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONQuaternionConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				Quaternion quaternion = (Quaternion)obj;
				Vector3 euler = quaternion.eulerAngles;
				JSONConverter.AppendFieldObject(node, euler.x, "x");
				JSONConverter.AppendFieldObject(node, euler.y, "y");
				JSONConverter.AppendFieldObject(node, euler.z, "z");
			}

			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;

				Quaternion quaternion = (Quaternion)obj;

				Vector3 euler;
				euler.x = JSONConverter.FieldObjectFromJSONElement<float>(node, "x");
				euler.y = JSONConverter.FieldObjectFromJSONElement<float>(node, "y");
				euler.z = JSONConverter.FieldObjectFromJSONElement<float>(node, "z");

				return Quaternion.Euler(euler);
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return (Quaternion)obj != (Quaternion)defaultObj;
			}
			#endregion
		}
	}
}