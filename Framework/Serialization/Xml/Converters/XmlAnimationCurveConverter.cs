using System.Collections.Generic;
using System.Xml;

using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(AnimationCurve), "AnimationCurve", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlAnimationCurveConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					AnimationCurve curve = (AnimationCurve)obj;

					if (curve != null)
					{
						XmlNode keyframesXmlNode = XmlUtils.CreateXmlNode(node.OwnerDocument, XmlConverter.kXmlArrayTag);
						for (int i = 0; i < curve.length; i++)
						{
							Keyframe keyFrame = curve[i];
							XmlNode keyFrameXmlNode = XmlUtils.CreateXmlNode(node.OwnerDocument, "KeyFrame");
							XmlConverter.AppendFieldObject(keyFrameXmlNode, keyFrame.inTangent, "inTangent");
							XmlConverter.AppendFieldObject(keyFrameXmlNode, keyFrame.outTangent, "outTangent");
							XmlConverter.AppendFieldObject(keyFrameXmlNode, keyFrame.time, "time");
							XmlConverter.AppendFieldObject(keyFrameXmlNode, keyFrame.value, "value");
							XmlUtils.SafeAppendChild(keyframesXmlNode, keyFrameXmlNode);
						}
						XmlUtils.AddAttribute(node.OwnerDocument, keyframesXmlNode, XmlConverter.kXmlFieldIdAttributeTag, "keyFrames");
						XmlUtils.SafeAppendChild(node, keyframesXmlNode);
					}
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					AnimationCurve curve = new AnimationCurve();

					XmlNode keyframesXmlNode = XmlUtils.FindChildWithAttributeValue(node, XmlConverter.kXmlFieldIdAttributeTag, "keyFrames");
					if (keyframesXmlNode != null)
					{
						List<Keyframe> keyFrames = new List<Keyframe>();
						foreach (XmlNode child in keyframesXmlNode.ChildNodes)
						{
							Keyframe keyFrame = new Keyframe();
							keyFrame.inTangent = XmlConverter.FieldObjectFromXmlNode<float>(child, "inTangent");
							keyFrame.outTangent = XmlConverter.FieldObjectFromXmlNode<float>(child, "outTangent");
							keyFrame.time = XmlConverter.FieldObjectFromXmlNode<float>(child, "time");
							keyFrame.value = XmlConverter.FieldObjectFromXmlNode<float>(child, "value");
							keyFrames.Add(keyFrame);
						}
						curve.keys = keyFrames.ToArray();
					}

					return curve;
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