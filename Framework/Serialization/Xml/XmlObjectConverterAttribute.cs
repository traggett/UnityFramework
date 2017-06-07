using System;

namespace Framework
{
	namespace Serialization
	{
		namespace Xml
		{
			[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
			public sealed class XmlObjectConverterAttribute : Attribute
			{
				#region Public Data
				public readonly Type ObjectType;
				public readonly string XmlTag;
				public readonly string OnConvertToXmlNodeMethod;
				public readonly string OnConvertFromXmlNodeMethod;
				public readonly string ShouldWriteMethod;
				#endregion

				#region Public Interface
				public XmlObjectConverterAttribute(Type objectType, string xmlTag, string onConvertToXmlNodeMethod, string onConvertFromXmlNodeMethod, string shouldWriteMethod)
				{
					ObjectType = objectType;
					XmlTag = xmlTag;
					OnConvertToXmlNodeMethod = onConvertToXmlNodeMethod;
					OnConvertFromXmlNodeMethod = onConvertFromXmlNodeMethod;
					ShouldWriteMethod = shouldWriteMethod;
				}
				#endregion
			}
		}
	}
}