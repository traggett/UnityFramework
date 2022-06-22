using System.Xml;

namespace Framework
{
	using Maths;

	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(FloatRange), "FloatRange", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlFloatRangeConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					//Add to nodes for x and y
					FloatRange floatRange = (FloatRange)obj;
					XmlConverter.AppendFieldObject(node, floatRange.Min, "min");
					XmlConverter.AppendFieldObject(node, floatRange.Max, "max");
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node == null)
						return obj;

					FloatRange floatRange = (FloatRange)obj;

					floatRange.Min = XmlConverter.FieldObjectFromXmlNode<float>(node, "min");
					floatRange.Max = XmlConverter.FieldObjectFromXmlNode<float>(node, "max");

					return floatRange;
				}

				public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
				{
					return (FloatRange)obj != (FloatRange)defaultObj;
				}
				#endregion
			}
		}
	}
}