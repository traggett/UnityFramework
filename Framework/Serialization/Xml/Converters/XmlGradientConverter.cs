using System.Collections.Generic;
using System.Xml;

using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(Gradient), "Gradient", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlGradientConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					Gradient gradient = (Gradient)obj;

					if (gradient != null)
					{
						XmlNode colorKeysXmlNode = XmlUtils.CreateXmlNode(node.OwnerDocument, XmlConverter.kXmlArrayTag);
						for (int i = 0; i < gradient.colorKeys.Length; i++)
						{
							XmlNode colorKeyXmlNode = XmlUtils.CreateXmlNode(node.OwnerDocument, "ColorKey");
							XmlConverter.AppendFieldObject(colorKeyXmlNode, gradient.colorKeys[i].color, "color");
							XmlConverter.AppendFieldObject(colorKeyXmlNode, gradient.colorKeys[i].time, "time");
							XmlUtils.SafeAppendChild(colorKeysXmlNode, colorKeyXmlNode);
						}
						XmlUtils.AddAttribute(node.OwnerDocument, colorKeysXmlNode, XmlConverter.kXmlFieldIdAttributeTag, "colorKeys");
						XmlUtils.SafeAppendChild(node, colorKeysXmlNode);


						XmlNode alphasKeyXmlNode = XmlUtils.CreateXmlNode(node.OwnerDocument, XmlConverter.kXmlArrayTag);
						for (int i = 0; i < gradient.alphaKeys.Length; i++)
						{
							XmlNode alphaKeyXmlNode = XmlUtils.CreateXmlNode(node.OwnerDocument, "AlphaKey");
							XmlConverter.AppendFieldObject(alphaKeyXmlNode, gradient.alphaKeys[i].alpha, "alpha");
							XmlConverter.AppendFieldObject(alphaKeyXmlNode, gradient.alphaKeys[i].time, "time");
							XmlUtils.SafeAppendChild(alphasKeyXmlNode, alphaKeyXmlNode);
						}
						XmlUtils.AddAttribute(node.OwnerDocument, alphasKeyXmlNode, XmlConverter.kXmlFieldIdAttributeTag, "alphaKeys");
						XmlUtils.SafeAppendChild(node, alphasKeyXmlNode);
					}
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					Gradient gradient = new Gradient();

					XmlNode colorKeysXmlNode = XmlUtils.FindChildWithAttributeValue(node, XmlConverter.kXmlFieldIdAttributeTag, "colorKeys");
					if (colorKeysXmlNode != null)
					{
						List<GradientColorKey> colorKeys = new List<GradientColorKey>();
						foreach (XmlNode child in colorKeysXmlNode.ChildNodes)
						{
							GradientColorKey colorKey = new GradientColorKey();
							colorKey.color = XmlConverter.FieldObjectFromXmlNode<Color>(child, "color");
							colorKey.time = XmlConverter.FieldObjectFromXmlNode<float>(child, "time");
							colorKeys.Add(colorKey);
						}
						gradient.colorKeys = colorKeys.ToArray();
					}

					XmlNode alphaKeysXmlNode = XmlUtils.FindChildWithAttributeValue(node, XmlConverter.kXmlFieldIdAttributeTag, "alphaKeys");
					if (alphaKeysXmlNode != null)
					{
						List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>();
						foreach (XmlNode child in alphaKeysXmlNode.ChildNodes)
						{
							GradientAlphaKey alphaKey = new GradientAlphaKey();
							alphaKey.alpha = XmlConverter.FieldObjectFromXmlNode<float>(child, "alpha");
							alphaKey.time = XmlConverter.FieldObjectFromXmlNode<float>(child, "time");
							alphaKeys.Add(alphaKey);
						}
						gradient.alphaKeys = alphaKeys.ToArray();
					}

					return gradient;
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