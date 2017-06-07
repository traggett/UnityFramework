using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Engine
{
	using Utils;

	namespace JSON
	{
		[JSONObjectConverter(typeof(Type), "SystemType", "OnConvertToJSONElement", "OnConvertFromJSONElement", "ShouldWriteNodeMethod")]
		public static class JSONSystemTypeConverter
		{
			#region JSONObjectConverter
			public static void OnConvertToJSONElement(object obj, JSONElement node)
			{
				Type currentType = (Type)obj;
				if (currentType != null)
					node.InnerText = currentType.FullName;
			}

			public static object OnConvertFromJSONElement(object obj, JSONElement node)
			{
				if (node == null)
					return obj;

				if (!string.IsNullOrEmpty(node.InnerText))
				{
					return SystemUtils.GetType(node.InnerText);
				}

				return null;
			}

			public static bool ShouldWriteNodeMethod(object obj, object defaultObj)
			{
				return (Type)obj != (Type)defaultObj;
			}
			#endregion
		}
	}
}