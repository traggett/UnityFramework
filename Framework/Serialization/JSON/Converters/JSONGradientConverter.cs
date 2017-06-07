using System.Collections.Generic;


using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(Gradient), "Gradient", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONGradientConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				Gradient gradient = (Gradient)obj;

				if (gradient != null)
				{
					JSONElement colorKeysJSONElement = JSONUtils.CreateJSONElement(node.OwnerDocument, JSONConverter.kJSONArrayTag);
					for (int i = 0; i < gradient.colorKeys.Length; i++)
					{
						JSONElement colorKeyJSONElement = JSONUtils.CreateJSONElement(node.OwnerDocument, "ColorKey");
						JSONConverter.AppendFieldObject(colorKeyJSONElement, gradient.colorKeys[i].color, "color");
						JSONConverter.AppendFieldObject(colorKeyJSONElement, gradient.colorKeys[i].time, "time");
						JSONUtils.SafeAppendChild(colorKeysJSONElement, colorKeyJSONElement);
					}
					JSONUtils.AddAttribute(node.OwnerDocument, colorKeysJSONElement, JSONConverter.kJSONFieldIdAttributeTag, "colorKeys");
					JSONUtils.SafeAppendChild(node, colorKeysJSONElement);


					JSONElement alphasKeyJSONElement = JSONUtils.CreateJSONElement(node.OwnerDocument, JSONConverter.kJSONArrayTag);
					for (int i = 0; i < gradient.alphaKeys.Length; i++)
					{
						JSONElement alphaKeyJSONElement = JSONUtils.CreateJSONElement(node.OwnerDocument, "AlphaKey");
						JSONConverter.AppendFieldObject(alphaKeyJSONElement, gradient.alphaKeys[i].alpha, "alpha");
						JSONConverter.AppendFieldObject(alphaKeyJSONElement, gradient.alphaKeys[i].time, "time");
						JSONUtils.SafeAppendChild(alphasKeyJSONElement, alphaKeyJSONElement);
					}
					JSONUtils.AddAttribute(node.OwnerDocument, alphasKeyJSONElement, JSONConverter.kJSONFieldIdAttributeTag, "alphaKeys");
					JSONUtils.SafeAppendChild(node, alphasKeyJSONElement);
				}	
			}

			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;

				Gradient gradient = new Gradient();

				JSONElement colorKeysJSONElement = JSONUtils.FindChildWithAttributeValue(node, JSONConverter.kJSONFieldIdAttributeTag, "colorKeys");
				if (colorKeysJSONElement != null)
				{
					List<GradientColorKey> colorKeys = new List<GradientColorKey>();
					foreach (JSONElement child in colorKeysJSONElement.ChildNodes)
					{
						GradientColorKey colorKey = new GradientColorKey();
						colorKey.color = JSONConverter.FieldObjectFromJSONElement<Color>(child, "color");
						colorKey.time = JSONConverter.FieldObjectFromJSONElement<float>(child, "time");
						colorKeys.Add(colorKey);
					}
					gradient.colorKeys = colorKeys.ToArray();
				}			

				JSONElement alphaKeysJSONElement = JSONUtils.FindChildWithAttributeValue(node, JSONConverter.kJSONFieldIdAttributeTag, "alphaKeys");
				if (alphaKeysJSONElement != null)
				{
					List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>();
					foreach (JSONElement child in alphaKeysJSONElement.ChildNodes)
					{
						GradientAlphaKey alphaKey = new GradientAlphaKey();
						alphaKey.alpha = JSONConverter.FieldObjectFromJSONElement<float>(child, "alpha");
						alphaKey.time = JSONConverter.FieldObjectFromJSONElement<float>(child, "time");
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