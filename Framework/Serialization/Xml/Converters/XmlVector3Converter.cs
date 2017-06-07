using System.Xml;

using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(Vector3), "Vector3", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlVector3Converter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					//Add to nodes for x and y
					Vector3 vector = (Vector3)obj;
					XmlConverter.AppendFieldObject(node, vector.x, "x");
					XmlConverter.AppendFieldObject(node, vector.y, "y");
					XmlConverter.AppendFieldObject(node, vector.z, "z");
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					Vector3 vector = (Vector3)obj;

					vector.x = XmlConverter.FieldObjectFromXmlNode<float>(node, "x");
					vector.y = XmlConverter.FieldObjectFromXmlNode<float>(node, "y");
					vector.z = XmlConverter.FieldObjectFromXmlNode<float>(node, "z");

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
}