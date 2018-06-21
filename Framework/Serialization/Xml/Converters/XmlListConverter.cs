using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{
	using Utils;

	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(List<>), "List", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlListConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					IList list = (IList)obj;
					Type listType = SystemUtils.GetGenericImplementationType(typeof(List<>), obj.GetType());

					if (listType != null)
					{
						for (int i=0; i<list.Count; i++)
						{
							XmlNode arrayItemXmlNode = XmlConverter.ToXmlNode(list[i], node.OwnerDocument);
							XmlUtils.SafeAppendChild(node, arrayItemXmlNode);
						}
					}
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node != null && obj != null)
					{
						IList list = (IList)obj;
						Type listType = SystemUtils.GetGenericImplementationType(typeof(List<>), obj.GetType());

						foreach (XmlNode entryNode in node.ChildNodes)
						{
							object listItemNode = XmlConverter.FromXmlNode(listType, entryNode);

							list.Add(listItemNode);
						}

						return list;
					}

					return obj;
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