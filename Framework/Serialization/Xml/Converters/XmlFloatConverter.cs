using System;
using System.Xml;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(float), "Float", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlFloatConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					node.InnerText = Convert.ToString(obj);
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					return Convert.ToSingle(node.InnerText);
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return (float)obj != (float)defaultObj;
				}
				#endregion
			}
		}
	}
}