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
					XmlConverter.AppendFieldObject(node, intRange._min, "min");
					XmlConverter.AppendFieldObject(node, intRange._max, "max");
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					IntRange intRange = (IntRange)obj;

					intRange._min = XmlConverter.FieldObjectFromXmlNode<int>(node, "min");
					intRange._max = XmlConverter.FieldObjectFromXmlNode<int>(node, "max");

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