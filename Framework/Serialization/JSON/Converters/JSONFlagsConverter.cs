using System;


namespace Engine
{
	namespace JSON
	{
		[JSONObjectConverter(typeof(FlagsAttribute), "Flags", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONFlagsConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				string valueName = ConvertFlagsToString(obj);
				int valueIndex = Convert.ToInt32(obj);

				JSONConverter.AppendFieldObject(node, valueName, "valueName");
				JSONConverter.AppendFieldObject(node, valueIndex, "valueIndex");
			}

			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;
				
				string valueName = JSONConverter.FieldObjectFromJSONElement<string>(node, "valueName");

				//First try to get value from string, If not possible, use the int value of the enum
				int valueInt;

				if (!ConvertFlagsFromString(obj, valueName, out valueInt))
				{
					valueInt = JSONConverter.FieldObjectFromJSONElement<int>(node, "valueIndex");
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