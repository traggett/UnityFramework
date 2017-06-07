using System.Xml;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(string), "String", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlStringConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					node.InnerText = (string)obj;
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					return node.InnerText;
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return (string)obj != (string)defaultObj;
				}
				#endregion
			}
		}
	}
}