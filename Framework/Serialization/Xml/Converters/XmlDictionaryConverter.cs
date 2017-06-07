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
			[XmlObjectConverter(typeof(Dictionary<,>), "Dictionary", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlDictionaryConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					IDictionary dictionary = (IDictionary)obj;
					Type[] dictionaryTypes = SystemUtils.GetGenericImplementationTypes(typeof(Dictionary<,>), obj.GetType());

					if (dictionaryTypes != null)
					{
						//Find keys
						string[] keys = new string[dictionary.Keys.Count];
						int i = 0;
						foreach (object key in dictionary.Keys)
						{
							keys[i++] = Convert.ToString(key);
						}

						//Append all child nodes
						i = 0;
						foreach (object value in dictionary.Values)
						{
							XmlNode arrayItemXmlNode = XmlConverter.ToXmlNode(value, node.OwnerDocument);
							XmlUtils.AddAttribute(node.OwnerDocument, arrayItemXmlNode, "key", keys[i++]);
							XmlUtils.SafeAppendChild(node, arrayItemXmlNode);
						}
					}
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node != null && obj != null)
					{
						IDictionary dictionary = (IDictionary)obj;
						Type[] dictionaryTypes = SystemUtils.GetGenericImplementationTypes(typeof(Dictionary<,>), obj.GetType());

						foreach (XmlNode entryNode in node.ChildNodes)
						{
							XmlNode att = entryNode.Attributes.GetNamedItem("key");
							if (att != null)
							{
								object arrayNode = XmlConverter.FromXmlNode(dictionaryTypes[1], entryNode);
								dictionary.Add(att.Value, arrayNode);
							}
						}

						return dictionary;
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