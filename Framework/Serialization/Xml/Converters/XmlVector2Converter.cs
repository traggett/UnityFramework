using System.Xml;

using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(Vector2), "Vector2", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlVector2Converter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					//Add to nodes for x and y
					Vector2 vector = (Vector2)obj;
					XmlConverter.AppendFieldObject(node, vector.x, "x");
					XmlConverter.AppendFieldObject(node, vector.y, "y");
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					Vector2 vector = (Vector2)obj;

					vector.x = XmlConverter.FieldObjectFromXmlNode<float>(node, "x");
					vector.y = XmlConverter.FieldObjectFromXmlNode<float>(node, "y");

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
}