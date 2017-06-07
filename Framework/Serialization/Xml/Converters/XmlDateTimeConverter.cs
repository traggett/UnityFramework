using System;
using System.Xml;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(DateTime), "DateTime", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlDateTimeConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					DateTime dateTime = (DateTime)obj;
					node.InnerText = dateTime.ToString();
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					return DateTime.Parse(node.InnerText);
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return (DateTime)obj != (DateTime)defaultObj;
				}
				#endregion
			}
		}
	}
}