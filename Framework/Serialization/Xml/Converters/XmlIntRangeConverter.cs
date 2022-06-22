using System.Xml;

namespace Framework
{
	using Maths;

	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(IntRange), "IntRange", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlIntRangeConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					//Add to nodes for x and y
					IntRange intRange = (IntRange)obj;
					XmlConverter.AppendFieldObject(node, intRange.Min, "min");
					XmlConverter.AppendFieldObject(node, intRange.Max, "max");
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					IntRange intRange = (IntRange)obj;

					intRange.Min = XmlConverter.FieldObjectFromXmlNode<int>(node, "min");
					intRange.Max = XmlConverter.FieldObjectFromXmlNode<int>(node, "max");

					return intRange;
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return (IntRange)obj != (IntRange)defaultObj;
				}
				#endregion
			}
		}
	}
}