using System;
using System.Xml;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(bool), "Bool", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlBoolConverter
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

					return Convert.ToBoolean(node.InnerText);
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return true;
				}
				#endregion
			}
		}
	}
}