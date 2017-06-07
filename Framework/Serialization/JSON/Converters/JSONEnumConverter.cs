using System;


namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(Enum), "Enum", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONEnumConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				string valueName = Enum.GetName(obj.GetType(), obj);
				int valueIndex = Convert.ToInt32(obj);

				JSONConverter.AppendFieldObject(node, valueName, "valueName");
				JSONConverter.AppendFieldObject(node, valueIndex, "valueIndex");
			}
			
			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;
				
				string valueName = JSONConverter.FieldObjectFromJSONElement<string>(node, "valueName");
				int valueIndex = JSONConverter.FieldObjectFromJSONElement<int>(node, "valueIndex");

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