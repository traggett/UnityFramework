using System.Xml;

using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(Vector4), "Vector4", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlVector4Converter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					//Add to nodes for x and y
					Vector4 vector = (Vector4)obj;
					XmlConverter.AppendFieldObject(node, vector.x, "x");
					XmlConverter.AppendFieldObject(node, vector.y, "y");
					XmlConverter.AppendFieldObject(node, vector.z, "z");
					XmlConverter.AppendFieldObject(node, vector.w, "w");
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					Vector4 vector = (Vector4)obj;

					vector.x = XmlConverter.FieldObjectFromXmlNode<float>(node, "x");
					vector.y = XmlConverter.FieldObjectFromXmlNode<float>(node, "y");
					vector.z = XmlConverter.FieldObjectFromXmlNode<float>(node, "z");
					vector.w = XmlConverter.FieldObjectFromXmlNode<float>(node, "w");

					return vector;
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return (Vector4)obj != (Vector4)defaultObj;
				}
				#endregion
			}
		}
	}
}