using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml;
using System.IO;

using UnityEngine;

namespace Framework
{
	using Utils;

	namespace Serialization
	{
		namespace Xml
		{
			public static class XmlConverter
			{
				#region Public Data
				public static readonly string kXmlFieldIdAttributeTag = "id";
				public static readonly string kXmlArrayTag = "Array";
				public delegate void OnConvertToXmlDelegate(object obj, XmlNode node);
				public delegate object OnConvertFromXmlDelegate(object obj, XmlNode node);
				public delegate bool ShouldWriteDelegate(object obj, object defaultObj);
				#endregion

				#region Private Data
				private class ObjectConverter
				{
					public readonly OnConvertToXmlDelegate _onConvertToXmlNode;
					public readonly OnConvertFromXmlDelegate _onConvertFromXmlNode;
					public readonly ShouldWriteDelegate _shouldWrite;

					public ObjectConverter(OnConvertToXmlDelegate onConvertToXmlNode, OnConvertFromXmlDelegate onConvertFromXmlNode, ShouldWriteDelegate shouldWrite)
					{
						_onConvertToXmlNode = onConvertToXmlNode;
						_onConvertFromXmlNode = onConvertFromXmlNode;
						_shouldWrite = shouldWrite;
					}
				}
				private static Dictionary<string, Type> _tagToTypeMap = null;
				private static Dictionary<Type, string> _typeToTagMap = null;
				private static Dictionary<Type, ObjectConverter> _converterMap;
				private static readonly string kXmlNodeRuntimeTypeAttributeTag = "runtimeType";
				#endregion

				#region Public Interface
				public static object FromXmlNode(Type objType, XmlNode node, object defualtObject = null)
				{
					object obj = null;

					//If object is an array convert each element one by one
					if (objType.IsArray)
					{
						int numChildren = node != null ? node.ChildNodes.Count : 0;

						//Create a new array from this nodes children
						Array array = Array.CreateInstance(objType.GetElementType(), numChildren);

						for (int i = 0; i < array.Length; i++)
						{
							//Convert child node
							object elementObj = FromXmlNode(objType.GetElementType(), node.ChildNodes[i]);
							//Then set it on the array
							array.SetValue(elementObj, i);
						}

						//Then set value on member
						obj = array;
					}
					else
					{
						//First find the actual object type from the xmlNode (could be different due to inheritance)
						Type realObjType = GetRuntimeType(node);
						
						//If the xml node type and passed in type are both generic then read type from runtime type node
						if (NeedsRuntimeTypeInfo(objType, realObjType))
						{
							realObjType = ReadTypeFromRuntimeTypeInfo(node);

							//If its still can't be found then use passed in type
							if (realObjType == null)
								realObjType = objType;
						}
						//If we don't have an xmlNode or the object type is invalid then use passed in type
						else if (node == null || realObjType == null || realObjType.IsAbstract || realObjType.IsGenericType)
						{
							realObjType = objType;
						}

						//If no type was found just return the object.
						if (realObjType == null)
						{
							return obj;
						}

						//Convert objects fields
						if (defualtObject != null)
						{
							obj = defualtObject;
						}
						//Create an object instance if default not passed in
						else if (!realObjType.IsAbstract)
						{
							obj = CreateInstance(realObjType);
						}

						//If the object has an associated converter class, convert the object using it
						ObjectConverter converter = GetConverter(realObjType);
						if (converter != null)
						{
							obj = converter._onConvertFromXmlNode(obj, node);
						}
						//Otherwise convert fields
						else if (node != null && obj != null)
						{
							SerializedObjectMemberInfo[] serializedFields = SerializedObjectMemberInfo.GetSerializedFields(realObjType);
							foreach (SerializedObjectMemberInfo serializedField in serializedFields)
							{
								//First try and find xml node with an id attribute matching our attribute id
								XmlNode fieldNode = XmlUtils.FindChildWithAttributeValue(node, kXmlFieldIdAttributeTag, serializedField.GetID());

								object fieldObj = serializedField.GetValue(obj);
								Type fieldObjType = serializedField.GetFieldType();

								//Convert the object from xml node
								fieldObj = FromXmlNode(fieldObjType, fieldNode, fieldObj);

								//Then set value on parent object
								try
								{
									serializedField.SetValue(obj, fieldObj);
								}
								catch (Exception e)
								{
									throw e;
								}
							}
						}
					}

					//IXmlConversionCallbackReceiver callback
					if (obj is ISerializationCallbackReceiver)
						((ISerializationCallbackReceiver)obj).OnAfterDeserialize();

					return obj;
				}

				public static T FromXmlNode<T>(XmlNode node, T defualtObject = default(T))
				{
					return (T)FromXmlNode(typeof(T), node, defualtObject);
				}

				public static T FromXmlString<T>(string text)
				{
					if (!string.IsNullOrEmpty(text))
					{
						XmlDocument xmlDoc = new XmlDocument();

						try
						{
							xmlDoc.LoadXml(text);

						}
						catch (Exception e)
						{
							throw new CorruptFileException(e);
						}

						return FromXMLDoc<T>(xmlDoc);
					}

					return default(T);
				}

				public static object FromXmlString(Type type, string text)
				{
					if (!string.IsNullOrEmpty(text))
					{
						XmlDocument xmlDoc = new XmlDocument();

						try
						{
							xmlDoc.LoadXml(text);

						}
						catch (Exception e)
						{
							throw new CorruptFileException(e);
						}

						return FromXMLDoc(type, xmlDoc);
					}

					return null;
				}

				public static T FromFile<T>(string fileName)
				{
					if (!string.IsNullOrEmpty(fileName))
					{
						XmlDocument xmlDoc = new XmlDocument();

						try
						{
							xmlDoc.Load(fileName);

						}
						catch (Exception e)
						{
							throw new CorruptFileException(e);
						}

						return FromXMLDoc<T>(xmlDoc);
					}

					return default(T);
				}

				public static T FromTextAsset<T>(TextAsset asset)
				{
					if (asset != null)
					{
						return FromXmlString<T>(asset.text);
					}

					return default(T);
				}

				public static T FromXMLDoc<T>(XmlDocument xmlDoc)
				{
					if (xmlDoc != null)
					{
						string xmlTag = GetXmlTag(typeof(T));
						XmlNode node = xmlDoc.SelectSingleNode(xmlTag);

						if (node == null)
							throw new ObjectNotFoundException(typeof(T));

						return FromXmlNode<T>(node);
					}

					return default(T);
				}

				public static object FromXMLDoc(Type type, XmlDocument xmlDoc)
				{
					if (xmlDoc != null)
					{
						string xmlTag = GetXmlTag(type);
						XmlNode node = xmlDoc.SelectSingleNode(xmlTag);

						if (node == null)
							throw new Exception("No node of type " + type.Name + " found in XmlDocument");

						return FromXmlNode(type, node);
					}

					return null;
				}

				public static XmlNode ToXmlNode<T>(T obj, XmlDocument xmlDoc, object defualtObject = null)
				{
					XmlNode node = null;

					if (obj != null)
					{
						Type objType = obj.GetType();
						ObjectConverter converter = GetConverter(objType);

						if (ShouldWriteObject(objType, converter, obj, defualtObject))
						{
							//IXmlConversionCallbackReceiver callback
							if (obj is ISerializationCallbackReceiver)
								((ISerializationCallbackReceiver)obj).OnBeforeSerialize();

							//If the object is an array create a node for the array and convert each element individually
							if (objType.IsArray)
							{
								Array arrayField = obj as Array;

								if (arrayField != null && arrayField.Length > 0)
								{
									XmlNode arrayXmlNode = XmlUtils.CreateXmlNode(xmlDoc, kXmlArrayTag);

									//Append all child nodes
									for (int i=0; i< arrayField.Length; i++)
									{
										XmlNode arrayElementXmlNode = ToXmlNode(arrayField.GetValue(i), xmlDoc);
										XmlUtils.SafeAppendChild(arrayXmlNode, arrayElementXmlNode);
									}

									node = arrayXmlNode;
								}
							}
							else
							{
								string tag = GetXmlTag(objType);

								if (!string.IsNullOrEmpty(tag))
								{
									node = XmlUtils.CreateXmlNode(xmlDoc, tag);

									//If the object has an associated converter class, convert the object using it
									if (converter != null)
									{
										converter._onConvertToXmlNode(obj, node);
									}
									//Otherwise convert each field
									else
									{
										SerializedObjectMemberInfo[] xmlFields = SerializedObjectMemberInfo.GetSerializedFields(objType);

										//Create a default version of this to compare with?
										if (defualtObject == null)
										{
											defualtObject = CreateInstance(objType);
										}

										foreach (SerializedObjectMemberInfo xmlField in xmlFields)
										{
											object fieldObj = xmlField.GetValue(obj);
											object defualtFieldObj = xmlField.GetValue(defualtObject);

											XmlNode fieldXmlNode = ToXmlNode(fieldObj, xmlDoc, defualtFieldObj);

											if (fieldXmlNode != null)
											{
												AddRuntimeTypeInfoIfNecessary(xmlField.GetFieldType(), fieldObj.GetType(), fieldXmlNode, xmlDoc);
												XmlUtils.AddAttribute(xmlDoc, fieldXmlNode, kXmlFieldIdAttributeTag, xmlField.GetID());
												XmlUtils.SafeAppendChild(node, fieldXmlNode);
											}
										}
									}
								}
							}
						}
					}

					return node;
				}

				public static string ToXmlString<T>(T obj)
				{
					string xml = "";

					XmlDocument xmlDocument = ToXmlDoc(obj);

					using (var stringWriter = new StringWriter())
					{
						using (var xmlTextWriter = XmlWriter.Create(stringWriter))
						{
							xmlDocument.WriteTo(xmlTextWriter);
							xmlTextWriter.Flush();
							xml = stringWriter.GetStringBuilder().ToString();
						}
					}

					return xml;
				}


				public static bool ToFile<T>(T obj, string fileName)
				{
					XmlDocument xmlDoc = ToXmlDoc(obj);
					try
					{
						xmlDoc.Save(fileName);
#if UNITY_EDITOR
						//Refresh the saved asset
						AssetUtils.RefreshAsset(fileName);
#endif
						return true;
					}
					catch
					{
						return false;
					}
				}

				public static XmlDocument ToXmlDoc<T>(T obj)
				{
					XmlDocument xmlDoc = new XmlDocument();
					XmlNode xmlNode = ToXmlNode(obj, xmlDoc);
					xmlDoc.AppendChild(xmlNode);
					return xmlDoc;
				}

				public static T CreateCopy<T>(T obj)
				{
					if (obj != null)
					{
						//Convert to xml and then back again as a new node
						XmlDocument xmlDoc = new XmlDocument();
						XmlNode node = ToXmlNode(obj, xmlDoc);
						object defaultObj = CreateInstance(obj.GetType());
						return (T)FromXmlNode(obj.GetType(), node, defaultObj);
					}

					return default(T);
				}

				public static bool DoesAssetContainNode<T>(TextAsset asset)
				{
					if (asset != null)
					{
						XmlDocument xmlDoc = new XmlDocument();
						try
						{
							xmlDoc.LoadXml(asset.text);
							XmlNode node = xmlDoc.SelectSingleNode(GetXmlTag(typeof(T)));
							return node != null;
						}
						catch
						{
						}
					}

					return false;
				}

				public static XmlNode AppendFieldObject<T>(XmlNode parentNode, T obj, string id)
				{
					XmlNode childXmlNode = ToXmlNode(obj, parentNode.OwnerDocument);
					XmlUtils.AddAttribute(parentNode.OwnerDocument, childXmlNode, kXmlFieldIdAttributeTag, id);
					XmlUtils.SafeAppendChild(parentNode, childXmlNode);
					return childXmlNode;
				}

				public static T FieldObjectFromXmlNode<T>(XmlNode parentNode, string id, T defualtObject = default(T))
				{
					XmlNode childXmlNode = XmlUtils.FindChildWithAttributeValue(parentNode, kXmlFieldIdAttributeTag, id);
					return (T)FromXmlNode(typeof(T), childXmlNode, defualtObject);
				}
				#endregion

				#region Private Functions
				private static string GetXmlTag(Type type)
				{
					string xmlTag = null;

					BuildTypeMap();

					Type objType = GetObjectConversionType(type);

					if (!_typeToTagMap.TryGetValue(objType, out xmlTag))
					{
						throw new Exception("Type '" + objType.Name + "' is not mapped to an XML tag, check it is marked as [Serializable] or has a XmlObjectConverter class.");
					}

					return xmlTag;
				}

				private static void BuildTypeMap()
				{
					//Build list of types of classes / structs with the Serializable  attribute
					if (_tagToTypeMap == null || _typeToTagMap == null || _converterMap == null)
					{
						_tagToTypeMap = new Dictionary<string, Type>();
						_typeToTagMap = new Dictionary<Type, string>();
						_converterMap = new Dictionary<Type, ObjectConverter>();

						Assembly[] assemblies = SystemUtils.GetUnityAssemblies();

						foreach (Assembly assembly in assemblies)
						{
							Type[] types = assembly.GetTypes();

							//First build dictionary of types marked with [Serializable] attribute
							foreach (Type type in types)
							{
								if (Attribute.IsDefined(type, typeof(SerializableAttribute), false))
								{
									SerializableAttribute attribute = SystemUtils.GetAttribute<SerializableAttribute>(type);

									string xmlTag = type.Name;

									if (type.IsGenericType)
									{
										string name = type.Name;
										int index = name.IndexOf('`');
										xmlTag = index == -1 ? name : name.Substring(0, index);
									}

									if (!_tagToTypeMap.ContainsKey(xmlTag) && !_typeToTagMap.ContainsKey(type))
									{
										_tagToTypeMap.Add(xmlTag, type);
										_typeToTagMap.Add(type, xmlTag);
									}
#if DEBUG
									else
									{
										Debug.Log("Can't serialize type " + type.FullName + " as it shares a name with another class (" + _tagToTypeMap[xmlTag].FullName + ")");
									}
#endif
								}
							}

							//Then find all XmlObjectConverterAttribute for those types
							foreach (Type type in types)
							{
								XmlObjectConverterAttribute converterAttribute = SystemUtils.GetAttribute<XmlObjectConverterAttribute>(type);

								if (converterAttribute != null)
								{

									ObjectConverter converter = new ObjectConverter(SystemUtils.GetStaticMethodAsDelegate<OnConvertToXmlDelegate>(type, converterAttribute.OnConvertToXmlNodeMethod),
																					SystemUtils.GetStaticMethodAsDelegate<OnConvertFromXmlDelegate>(type, converterAttribute.OnConvertFromXmlNodeMethod),
																					SystemUtils.GetStaticMethodAsDelegate<ShouldWriteDelegate>(type, converterAttribute.ShouldWriteMethod));

									_converterMap[converterAttribute.ObjectType] = converter;
									_tagToTypeMap[converterAttribute.XmlTag] = converterAttribute.ObjectType;
									_typeToTagMap[converterAttribute.ObjectType] = converterAttribute.XmlTag;
								}
							}
						}
					}
				}

				private static Type GetRuntimeType(XmlNode node)
				{
					Type type = null;

					if (node != null)
					{
						string xmlTag = node.Name;

						BuildTypeMap();

						if (!_tagToTypeMap.TryGetValue(xmlTag, out type))
						{
							Debug.LogError("XML Node tag '" + xmlTag + "' is not mapped to a type, check a class has a XmlTagAttribute with the same tag");
						}
					}

					return type;
				}

				private static bool ShouldWriteObject(Type objType, ObjectConverter converter, object obj, object defualtObj)
				{
					//If its an array always write
					if (objType.IsArray)
					{
						return true;
					}
					//Never write the object if it's null (??what about if the default is non null?? write a null node??)
					else if (obj == null)
					{
						return false;
					}
					//If object is null by default OR is of a different then always write 
					else if (defualtObj == null || defualtObj.GetType() != obj.GetType())
					{
						return true;
					}
					//If the object has a converter ask it if should write 
					else if (converter != null)
					{
						return converter._shouldWrite(obj, defualtObj);
					}
					//otherwise check objects fields to see if they need to write
					else
					{
						SerializedObjectMemberInfo[] xmlFields = SerializedObjectMemberInfo.GetSerializedFields(obj.GetType());
						foreach (SerializedObjectMemberInfo xmlField in xmlFields)
						{
							ObjectConverter fieldConverter = GetConverter(xmlField.GetFieldType());
							object fieldObj = xmlField.GetValue(obj);
							object defualtFieldObj = xmlField.GetValue(defualtObj);

							if (ShouldWriteObject(xmlField.GetFieldType(), fieldConverter, fieldObj, defualtFieldObj))
								return true;
						}
					}

					return false;
				}

				private static Type ReadTypeFromRuntimeTypeInfo(XmlNode node)
				{
					Type type = null;

					if (node != null)
					{
						XmlNode childXmlNode = XmlUtils.FindChildWithAttributeValue(node, kXmlNodeRuntimeTypeAttributeTag, "");
						if (childXmlNode != null)
						{
							type = (Type)FromXmlNode(typeof(Type), childXmlNode);
						}
					}

					return type;
				}

				private static void AddRuntimeTypeInfoIfNecessary(Type fieldType, Type objType, XmlNode parentNode, XmlDocument xmlDoc)
				{
					//If field is abstract or generic AND node is abstract or generic, insert type node so we can convert it
					if (NeedsRuntimeTypeInfo(fieldType, objType))
					{
						XmlNode typeNode = ToXmlNode(objType, xmlDoc);
						XmlUtils.AddAttribute(xmlDoc, typeNode, kXmlNodeRuntimeTypeAttributeTag, "");
						XmlUtils.SafeAppendChild(parentNode, typeNode);
					}
				}

				private static bool NeedsRuntimeTypeInfo(Type fieldType, Type objType)
				{
					if (fieldType != null && objType != null)
					{
						Type conversionType = GetObjectConversionType(objType);
						return fieldType != typeof(Type) && (fieldType.IsAbstract || fieldType == typeof(object)) && (conversionType.IsAbstract || conversionType.IsGenericType);
					}

					return false;
				}

				private static ObjectConverter GetConverter(Type type)
				{
					ObjectConverter converter;

					BuildTypeMap();

					Type objectType = GetObjectConversionType(type);

					if (objectType.IsEnum)
					{
						if (objectType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
						{
							objectType = typeof(FlagsAttribute);
						}
						else
						{
							objectType = typeof(Enum);
						}
					}
					else if (objectType.IsGenericType)
					{
						objectType = objectType.GetGenericTypeDefinition();
					}

					if (_converterMap.TryGetValue(objectType, out converter))
					{
						return converter;
					}

					return null;
				}

				private static object CreateInstance(Type type)
				{
					object obj;

					if (type == null || type.IsAbstract)
					{
						throw new Exception("Can't create object of abstract type " + type.Name);
					}

					try
					{
						obj = Activator.CreateInstance(type, true);
					}
					catch
					{
						if (type == typeof(string) || !type.IsClass)
						{
							obj = default(Type);
						}
						else
						{
							throw new Exception("Can't create object of type " + type.Name + " check it has a parameterless constructor defined.");
						}
					}

					return obj;
				}

				private static Type GetObjectConversionType(Type objType)
				{
					if (objType.IsEnum)
					{
						if (objType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
						{
							objType = typeof(FlagsAttribute);
						}
						else
						{
							objType = typeof(Enum);
						}
					}
					else if (objType.IsGenericType)
					{
						objType = objType.GetGenericTypeDefinition();
					}
					else if (objType == typeof(Type).GetType())
					{
						objType = typeof(Type);
					}

					return objType;
				}
#endregion
			}
		}
	}
}