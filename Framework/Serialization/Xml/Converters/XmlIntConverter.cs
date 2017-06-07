using System;
using System.Xml;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(int), "Int", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlIntConverter
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

					return Convert.ToInt32(node.InnerText);
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return (int)obj != (int)defaultObj;
				}
				#endregion
			}
		}
	}
}