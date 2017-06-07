using System;
using System.Xml;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(Enum), "Enum", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlEnumConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					string valueName = Enum.GetName(obj.GetType(), obj);
					int valueIndex = Convert.ToInt32(obj);

					XmlConverter.AppendFieldObject(node, valueName, "valueName");
					XmlConverter.AppendFieldObject(node, valueIndex, "valueIndex");
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					string valueName = XmlConverter.FieldObjectFromXmlNode<string>(node, "valueName");
					int valueIndex = XmlConverter.FieldObjectFromXmlNode<int>(node, "valueIndex");

					//First try to get value from string
					if (!string.IsNullOrEmpty(valueName) && Enum.IsDefined(obj.GetType(), valueName))
					{
						return Enum.Parse(obj.GetType(), valueName);
					}
					//If not possible, use the int value of the enum
					else
					{
						return Enum.ToObject(obj.GetType(), valueIndex);
					}
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return (int)obj != (int)defaultObj;
				}
				#endregion
			}
		}
	}
}