using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Framework
{
	using Utils;

	namespace Serialization
	{
		namespace Xml
		{
			[XmlObjectConverter(typeof(HashSet<>), "HashSet", "OnConvertToXmlNode", "OnConvertFromXmlNode", "ShouldWriteNodeMethod")]
			public static class XmlHashSetConverter
			{
				#region XmlObjectConverter
				public static void OnConvertToXmlNode(object obj, XmlNode node)
				{
					IEnumerable hashSet = (IDictionary)obj;
					Type hashSetType = SystemUtils.GetGenericImplementationType(typeof(HashSet<>), obj.GetType());

					if (hashSetType != null)
					{
						List<object> values = new List<object>();

						IEnumerator e = hashSet.GetEnumerator();

						while (e.MoveNext())
						{
							object value = e.Current;
							values.Add(value);
						}
						
						//Create and append child nodes
						foreach (object value in values)
						{
							XmlNode childNode = XmlConverter.ToXmlNode(value, node.OwnerDocument);
							XmlUtils.SafeAppendChild(node, childNode);
						}
					}
				}

				public static object OnConvertFromXmlNode(object obj, XmlNode node)
				{
					if (node != null && obj != null)
					{
						Type hashSetType = SystemUtils.GetGenericImplementationType(typeof(HashSet<>), obj.GetType());

						int numChildren = node.ChildNodes.Count;
						Array array = Array.CreateInstance(hashSetType, numChildren);

						for (int i = 0; i < array.Length; i++)
						{
							//Convert child node
							object arrayNode = XmlConverter.FromXmlNode(hashSetType, node.ChildNodes[i]);
							//Then set it on the array
							array.SetValue(arrayNode, i);
						}

						object hashSet = Activator.CreateInstance(obj.GetType(), array);
						return hashSet;
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
}