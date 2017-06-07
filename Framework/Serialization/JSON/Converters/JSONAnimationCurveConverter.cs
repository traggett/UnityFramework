using System.Collections.Generic;

using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(AnimationCurve), "AnimationCurve", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONAnimationCurveConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				AnimationCurve curve = (AnimationCurve)obj;

				if (curve != null)
				{
					JSONElement keyframesJSONElement = JSONUtils.CreateJSONElement(node.OwnerDocument, JSONConverter.kJSONArrayTag);
					for (int i = 0; i < curve.length; i++)
					{
						Keyframe keyFrame = curve[i];
						JSONElement keyFrameJSONElement = JSONUtils.CreateJSONElement(node.OwnerDocument, "KeyFrame");
						JSONConverter.AppendFieldObject(keyFrameJSONElement, keyFrame.inTangent, "inTangent");
						JSONConverter.AppendFieldObject(keyFrameJSONElement, keyFrame.outTangent, "outTangent");
						JSONConverter.AppendFieldObject(keyFrameJSONElement, keyFrame.tangentMode, "tangentMode");
						JSONConverter.AppendFieldObject(keyFrameJSONElement, keyFrame.time, "time");
						JSONConverter.AppendFieldObject(keyFrameJSONElement, keyFrame.value, "value");
						JSONUtils.SafeAppendChild(keyframesJSONElement, keyFrameJSONElement);
					}
					JSONUtils.AddAttribute(node.OwnerDocument, keyframesJSONElement, JSONConverter.kJSONFieldIdAttributeTag, "keyFrames");
					JSONUtils.SafeAppendChild(node, keyframesJSONElement);
				}	
			}

			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;

				AnimationCurve curve = new AnimationCurve();

				JSONElement keyframesJSONElement = JSONUtils.FindChildWithAttributeValue(node, JSONConverter.kJSONFieldIdAttributeTag, "keyFrames");
				if (keyframesJSONElement != null)
				{
					List<Keyframe> keyFrames = new List<Keyframe>();
					foreach (JSONElement child in keyframesJSONElement.ChildNodes)
					{
						Keyframe keyFrame = new Keyframe();
						keyFrame.inTangent = JSONConverter.FieldObjectFromJSONElement<float>(child, "inTangent");
						keyFrame.outTangent = JSONConverter.FieldObjectFromJSONElement<float>(child, "outTangent");
						keyFrame.tangentMode = JSONConverter.FieldObjectFromJSONElement<int>(child, "tangentMode");
						keyFrame.time = JSONConverter.FieldObjectFromJSONElement<float>(child, "time");
						keyFrame.value = JSONConverter.FieldObjectFromJSONElement<float>(child, "value");
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