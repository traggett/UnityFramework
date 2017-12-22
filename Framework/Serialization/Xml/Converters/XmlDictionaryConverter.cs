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

						//Create all child nodes
						List<XmlNode> childNodes = new List<XmlNode>();
						i = 0;
						foreach (object value in dictionary.Values)
						{
							XmlNode arrayItemXmlNode = XmlConverter.ToXmlNode(value, node.OwnerDocument);
							XmlUtils.AddAttribute(node.OwnerDocument, arrayItemXmlNode, "key", keys[i++]);
							childNodes.Add(arrayItemXmlNode);
						}

						//Sort child nodes
						childNodes.Sort((x, y) => x.Attributes["key"].Value.CompareTo(y.Attributes["key"].Value));

						//Append child nodes in alphabetical order
						foreach (XmlNode childNode in childNodes)
						{
							XmlUtils.SafeAppendChild(node, childNode);
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

								if (dictionary.Contains(att.Value))
								{
									dictionary[att.Value] = arrayNode;
								}
								else
								{
									dictionary.Add(att.Value, arrayNode);
								}
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