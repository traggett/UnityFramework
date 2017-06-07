using System.Collections.Generic;

namespace Engine
{
	namespace JSON
	{
		public class JSONWriter
		{
			public static string ToString(JSONElement jsonElement)
			{
				if (jsonElement is JSONBool)
				{
					return ToString((JSONBool)jsonElement);
				}
				else if (jsonElement is JSONNumber)
				{
					return ToString((JSONNumber)jsonElement);
				}
				else if (jsonElement is JSONArray)
				{
					return ToString((JSONArray)jsonElement);
				}
				else if (jsonElement is JSONObject)
				{
					return ToString((JSONObject)jsonElement);
				}

				return string.Empty;
			}

			public static string ToString(JSONBool jsonBool)
			{
				return jsonBool._value ? "true" : "false";
			}

			public static string ToString(JSONNumber jsonNumber)
			{
				return jsonNumber._value.ToString();
			}

			public static string ToString(JSONArray jsonArray)
			{
				string arrayString = "[";

				for (int i=0; i<jsonArray._elements.Count; i++)
				{
					arrayString += ToString(jsonArray._elements[i]);

					if (i < jsonArray._elements.Count-1)
						arrayString += ",";

					arrayString += "/n";
				}

				arrayString += "]";
				return arrayString;
			}

			public static string ToString(JSONObject jsonObject)
			{
				string objectString = "{";

				List<string> keys = new List<string>(jsonObject._fields.Keys);

				for (int i = 0; i < keys.Count; i++)
				{
					string fieldString = '"' + keys[i] + '"' + ": " + ToString(jsonObject._fields[keys[i]]);
					objectString += fieldString;

					if (i < keys.Count - 1)
						objectString += ",";

					objectString += "/n";
				}

				objectString += "}";
				return objectString;
			}
		}
	}
}