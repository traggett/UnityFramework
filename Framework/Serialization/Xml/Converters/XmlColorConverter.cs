using System.Xml;

using UnityEngine;

namespace Framework
{
	using Utils;

	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(Color), "Color", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlColorConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					Color color = (Color)obj;
					node.InnerText = StringUtils.ColorToHex(color);
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					return StringUtils.HexToColor(node.InnerText);
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return (Color)obj != (Color)defaultObj;
				}
				#endregion
			}
		}
	}
}