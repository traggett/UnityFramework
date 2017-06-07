using System;
using System.Xml;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(FlagsAttribute), "Flags", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlFlagsConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					string valueName = ConvertFlagsToString(obj);
					int valueIndex = Convert.ToInt32(obj);

					XmlConverter.AppendFieldObject(node, valueName, "valueName");
					XmlConverter.AppendFieldObject(node, valueIndex, "valueIndex");
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					string valueName = XmlConverter.FieldObjectFromXmlNode<string>(node, "valueName");

					//First try to get value from string, If not possible, use the int value of the enum
					int valueInt;

					if (!ConvertFlagsFromString(obj, valueName, out valueInt))
					{
						valueInt = XmlConverter.FieldObjectFromXmlNode<int>(node, "valueIndex");
					}

					return Enum.ToObject(obj.GetType(), valueInt);
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return (int)obj != (int)defaultObj;
				}
				#endregion

				#region Private Functions
				private static string ConvertFlagsToString(object obj)
				{
					string flagsString = "";

					int flags = (int)obj;

					foreach (var enumValue in Enum.GetValues(obj.GetType()))
					{
						int flag = Convert.ToInt32(enumValue);
						if ((flags & flag) != 0)
						{
							if (!string.IsNullOrEmpty(flagsString))
								flagsString += '|';

							flagsString += Enum.GetName(obj.GetType(), flag);
						}
					}

					return flagsString;
				}

				private static bool ConvertFlagsFromString(object obj, string flagsString, out int flagsValue)
				{
					flagsValue = 0;

					if (string.IsNullOrEmpty(flagsString))
						return false;

					string[] flags = flagsString.Split('|');
					foreach (string flag in flags)
					{
						if (Enum.IsDefined(obj.GetType(), flag))
						{
							int flagValue = (int)Enum.Parse(obj.GetType(), flag);

							flagsValue |= flagValue;
						}
						else
						{
							return false;
						}
					}

					return true;
				}
				#endregion
			}
		}
	}
}