using System;

using System.Collections;
using System.Collections.Generic;

namespace Engine
{
	using Utils;
	
	namespace JSON
	{
		[JSONObjectConverter(typeof(Dictionary<,>), "Dictionary", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONDictionaryConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				IDictionary dictionary = (IDictionary)obj;
				Type[] dictionaryTypes = SystemUtils.GetGenericImplementationTypes(typeof(Dictionary<,>), obj.GetType());

				if (dictionaryTypes != null)
				{
					//Find keys
					string[] keys = new string[dictionary.Keys.Count];
					int i = 0;
					foreach (object key in dictionary.Keys)
					{
						keys[i++] = Convert.ToString(key);
					}

					//Append all child nodes
					i = 0;
					foreach (object value in dictionary.Values)
					{
						JSONElement arrayItemJSONElement = JSONConverter.ToJSONElement(value, node.OwnerDocument);
						JSONUtils.AddAttribute(node.OwnerDocument, arrayItemJSONElement, "key", keys[i++]);
						JSONUtils.SafeAppendChild(node, arrayItemJSONElement);
					}
				}			
			}
			
			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node != null && obj != null)
				{
					IDictionary dictionary = (IDictionary)obj;
					Type[] dictionaryTypes = SystemUtils.GetGenericImplementationTypes(typeof(Dictionary<,>), obj.GetType());
					
					foreach (JSONElement entryNode in node.ChildNodes)
					{
						JSONElement att = entryNode.Attributes.GetNamedItem("key");
						if (att != null)
						{
							object arrayNode = JSONConverter.FromJSONElement(dictionaryTypes[1], entryNode);					
							dictionary.Add(att.Value, arrayNode);
						}
					}

					return dictionary;
				}

				return obj;
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return true;
			}
			#endregion
		}
	}
}