using System;
using System.Xml;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			public static class XmlUtils
			{
				public static XmlNode CreateXmlNode(XmlDocument xmlDoc, string name)
				{
					return xmlDoc.CreateNode(XmlNodeType.Element, name, string.Empty);
				}

				public static void SafeAppendChild(XmlNode parent, XmlNode node)
				{
					if (parent != null && node != null)
					{
						parent.AppendChild(node);
					}
				}

				public static XmlNode FindChildWithAttributeValue(XmlNode node, string attributeName, string attributeValue)
				{
					//Find xml node with matching id
					foreach (XmlNode childNode in node.ChildNodes)
					{
						if (childNode.Attributes != null)
						{
							XmlNode att = childNode.Attributes.GetNamedItem(attributeName);
							if (att != null && att.Value == attributeValue)
							{
								return childNode;
							}
						}
					}

					return null;
				}
				public static float GetXMLNodeAttribute(XmlNode node, string tag, float defautValue)
				{
					XmlNode att = node.Attributes.GetNamedItem(tag);
					if (att != null)
					{
						return Convert.ToSingle(att.Value);
					}

					return defautValue;
				}

				public static int GetXMLNodeAttribute(XmlNode node, string tag, int defautValue)
				{
					XmlNode att = node.Attributes.GetNamedItem(tag);
					if (att != null)
					{
						return Convert.ToInt32(att.Value);
					}

					return defautValue;
				}

				public static bool GetXMLNodeAttribute(XmlNode node, string tag, bool defautValue)
				{
					XmlNode att = node.Attributes.GetNamedItem(tag);
					if (att != null)
					{
						return Convert.ToBoolean(att.Value);
					}

					return defautValue;
				}

				public static string GetXMLNodeAttribute(XmlNode node, string tag, string defautValue)
				{
					XmlNode att = node.Attributes.GetNamedItem(tag);
					if (att != null)
					{
						return att.Value;
					}

					return defautValue;
				}

				public static void AddAttribute(XmlDocument xmldoc, XmlNode node, string tag, float value)
				{
					XmlAttribute att = xmldoc.CreateAttribute(tag);
					att.Value = Convert.ToString(value);
					node.Attributes.Append(att);
				}

				public static void AddAttribute(XmlDocument xmldoc, XmlNode node, string tag, int value)
				{
					XmlAttribute att = xmldoc.CreateAttribute(tag);
					att.Value = Convert.ToString(value);
					node.Attributes.Append(att);
				}

				public static void AddAttribute(XmlDocument xmldoc, XmlNode node, string tag, bool value)
				{
					XmlAttribute att = xmldoc.CreateAttribute(tag);
					att.Value = Convert.ToString(value);
					node.Attributes.Append(att);
				}

				public static void AddAttribute(XmlDocument xmldoc, XmlNode node, string tag, string value)
				{
					XmlAttribute att = xmldoc.CreateAttribute(tag);
					att.Value = value;
					node.Attributes.Append(att);
				}
			}
		}
	}
}