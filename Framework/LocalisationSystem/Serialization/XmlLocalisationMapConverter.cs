using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;


namespace Framework
{
	using Serialization.Xml;

	namespace LocalisationSystem
	{
		namespace Serialization
		{
			namespace Xml
			{
				[XmlObjectConverter(typeof(LocalisationMap), "LocalisationMap", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
				public static class XmlLocalisationMapConverter
				{
					#region XmlObjectConverter
					public static void OnConvertToXmlNode(object obj, XmlNode node)
					{
						LocalisationMap localisationMap = (LocalisationMap)obj;

						//Find GUIDs
						var guids = localisationMap.GetStringGUIDs();
						
						//Create all child nodes
						List<XmlNode> childNodes = new List<XmlNode>();
						
						foreach (var guid in guids)
						{
							string text = localisationMap.Get(guid, true);
							string key = localisationMap.KeyFromGUID(guid);

							XmlNode arrayItemXmlNode = XmlConverter.ToXmlNode(text, node.OwnerDocument);
							XmlUtils.AddAttribute(node.OwnerDocument, arrayItemXmlNode, "key", key);
							XmlUtils.AddAttribute(node.OwnerDocument, arrayItemXmlNode, "guid", guid);
							childNodes.Add(arrayItemXmlNode);
						}

						//Append child nodes
						foreach (XmlNode childNode in childNodes)
						{
							XmlUtils.SafeAppendChild(node, childNode);
						}
					}

					public static object OnConvertFromXmlNode(object obj, XmlNode node)
					{
						if (node != null && obj != null)
						{
							LocalisationMap localisationMap = (LocalisationMap)obj;

							foreach (XmlNode entryNode in node.ChildNodes)
							{
								XmlNode keyAtt = entryNode.Attributes.GetNamedItem("key");
								XmlNode guidAtt = entryNode.Attributes.GetNamedItem("guid");

								if (keyAtt != null && guidAtt != null)
								{
									string text = XmlConverter.FromXmlNode<string>(entryNode);

									localisationMap.SetString(guidAtt.Value, keyAtt.Value, text);
								}

							}

							return localisationMap;
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
}