using UnityEngine;
using System;
using System.IO;

namespace Framework
{
	namespace Serialization
	{
		using Xml;

		public static class SerializeConverter
		{
			//TO DO if want to support different serialization types then choose them based on file name etc in this class


			#region Public Interface
			public static T FromFile<T>(string fileName)
			{
				//To do! Work out what converter to use based on file extension?
				if (!string.IsNullOrEmpty(fileName))
				{
					return XmlConverter.FromFile<T>(fileName);
				}

				return default(T);
			}

			public static T FromTextAsset<T>(TextAsset asset)
			{
				//To do! Work out what converter to use based on file contents?
				if (asset != null)
				{
					return XmlConverter.FromXmlString<T>(asset.text);
				}

				return default(T);
			}

			public static T FromString<T>(string text)
			{
				//To do! Work out what converter to use based on contents?
				return XmlConverter.FromXmlString<T>(text);
			}

			public static object FromString(Type type, string text)
			{
				//To do! Work out what converter to use based on contents?
				return XmlConverter.FromXmlString(type, text);
			}

			public static string ToString<T>(T obj)
			{
				return XmlConverter.ToXmlString(obj);
			}

#if UNITY_EDITOR
			public static bool ToFile<T>(T obj, string fileName)
			{
				if (Path.GetExtension(fileName).ToLower() == ".xml")
				{
					return XmlConverter.ToFile<T>(obj, fileName);
				}

				return false;
			}
#endif

			public static T CreateCopy<T>(T obj)
			{
				if (obj != null)
				{
					return XmlConverter.CreateCopy(obj);
				}

				return default(T);
			}

			public static bool DoesAssetContainObject<T>(TextAsset asset)
			{
				if (asset != null)
				{
					return XmlConverter.DoesAssetContainNode<T>(asset);
				}

				return false;
			}
			#endregion
		}
	}
}