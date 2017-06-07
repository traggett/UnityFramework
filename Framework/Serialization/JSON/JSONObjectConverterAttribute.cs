using System;

namespace Engine
{
	namespace JSON
	{
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		public sealed class JSONObjectConverterAttribute : Attribute
		{
			#region Public Data
			public readonly Type ObjectType;
			public readonly string JSONTag;
			public readonly string OnConvertToJSONElementMethod;
			public readonly string OnConvertFromJSONElementMethod;
			public readonly string ShouldWriteMethod;
			#endregion

			#region Public Interface
			public JSONObjectConverterAttribute(Type objectType, string xmlTag, string onConvertToXmlNodeMethod, string onConvertFromXmlNodeMethod, string shouldWriteMethod)
			{
				ObjectType = objectType;
				JSONTag = xmlTag;
				OnConvertToJSONElementMethod = onConvertToXmlNodeMethod;
				OnConvertFromJSONElementMethod = onConvertFromXmlNodeMethod;
				ShouldWriteMethod = shouldWriteMethod;
			}
			#endregion
		}
	}
}