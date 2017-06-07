using System.Xml;

using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(Quaternion), "Quaternion", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlQuaternionConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					Quaternion quaternion = (Quaternion)obj;
					Vector3 euler = quaternion.eulerAngles;
					XmlConverter.AppendFieldObject(node, euler.x, "x");
					XmlConverter.AppendFieldObject(node, euler.y, "y");
					XmlConverter.AppendFieldObject(node, euler.z, "z");
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;
					
					Vector3 euler;
					euler.x = XmlConverter.FieldObjectFromXmlNode<float>(node, "x");
					euler.y = XmlConverter.FieldObjectFromXmlNode<float>(node, "y");
					euler.z = XmlConverter.FieldObjectFromXmlNode<float>(node, "z");

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
}