using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Engine
{
	using Utils;

	namespace JSON
	{
		public static class JSONConverter
		{
			#region Public Data
			public static readonly string kJSONFieldIdAttributeTag = "id";
			public static readonly string kJSONArrayTag = "Array";
			public delegate JSONElement OnConvertToJSONDelegate(object obj);
			public delegate object OnConvertFromJSONDelegate(object obj, JSONElement node);
			public delegate bool ShouldWriteDelegate(object obj, object defaultObj);
			#endregion

			#region Private Data
			private class ObjectConverter
			{
				public readonly OnConvertToJSONDelegate _onConvertToJSONElement;
				public readonly OnConvertFromJSONDelegate _onConvertFromJSONElement;
				public readonly ShouldWriteDelegate _shouldWrite;

				public ObjectConverter(OnConvertToJSONDelegate onConvertToJSONElement, OnConvertFromJSONDelegate onConvertFromJSONElement, ShouldWriteDelegate shouldWrite)
				{
					_onConvertToJSONElement = onConvertToJSONElement;
					_onConvertFromJSONElement = onConvertFromJSONElement;
					_shouldWrite = shouldWrite;
				}
			}
			private static Dictionary<string, Type> _tagToTypeMap = null;
			private static Dictionary<Type, string> _typeToTagMap = null;
			private static Dictionary<Type, ObjectConverter> _converterMap;
			private static readonly string kJSONElementRuntimeTypeAttributeTag = "runtimeType";
			#endregion

			#region Public Interface
			public static object FromJSONElement(Type objType, JSONElement node, object defualtObject = null)
			{
				object obj = null;

				//If object is an array convert each element one by one
				if (objType.IsArray)
				{
					JSONArray jsonArray = (JSONArray)node;

					int numChildren = node != null ? jsonArray._elements.Count : 0;

					//Create a new array from this nodes children
					Array array = Array.CreateInstance(objType.GetElementType(), numChildren);

					for (int i = 0; i < array.Length; i++)
					{
						//Convert child node
						object elementObj = FromJSONElement(objType.GetElementType(), jsonArray._elements[i]);
						//Then set it on the array
						array.SetValue(elementObj, i);
					}

					//Then set value on member
					obj = array;
				}
				else
				{
					//First find the actual object type from the JSONElement (could be different due to inheritance)
					Type realObjType = GetRuntimeType(node);

					//If the JSON node type and passed in type are both generic then read type from runtime type node
					if (NeedsRuntimeTypeInfo(objType, realObjType))
					{
						realObjType = ReadTypeFromRuntimeTypeInfo(node);

						//If its still can't be found then use passed in type
						if (realObjType == null)
							realObjType = objType;
					}
					//If we don't have an JSONElement or the object type is invalid then use passed in type
					else if (node == null || realObjType == null || realObjType.IsAbstract || realObjType.IsGenericType)
					{
						realObjType = objType;
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
						obj = converter._onConvertFromJSONElement(obj, node);
					}
					//Otherwise convert fields
					else if (node != null && obj != null)
					{
						JSONField[] JSONFields = GetJSONFields(realObjType);
						foreach (JSONField JSONField in JSONFields)
						{
							//First try and find JSON node with an id attribute matching our attribute id
							JSONElement fieldNode = JSONUtils.FindChildWithAttributeValue(node, kJSONFieldIdAttributeTag, JSONField.GetID());

							object fieldObj = JSONField.GetValue(obj);
							Type fieldObjType = JSONField.GetFieldType();

							//Convert the object from JSON node
							fieldObj = FromJSONElement(fieldObjType, fieldNode, fieldObj);

							//Then set value on parent object
							try
							{
								JSONField.SetValue(obj, fieldObj);
							}
							catch (Exception e)
							{
								throw e;
							}
						}
					}
				}

				//IJSONConversionCallbackReceiver callback
				if (obj is IJSONConversionCallbackReceiver)
					((IJSONConversionCallbackReceiver)obj).OnConvertFromJSONElement(node);

				return obj;
			}

			public static T FromJSONElement<T>(JSONElement node, T defualtObject = default(T))
			{
				return (T)FromJSONElement(typeof(T), node, defualtObject);
			}

			public static T FromJSONString<T>(string text)
			{
				if (!string.IsNullOrEmpty(text))
				{
					JsonParser parser = new JsonParser();
					JSONElement json = parser.Decode(text);
					return FromJSONElement<T>(json);
				}

				return default(T);
			}

			public static object FromJSONString(Type type, string text)
			{
				if (!string.IsNullOrEmpty(text))
				{
					if (!string.IsNullOrEmpty(text))
					{
						JsonParser parser = new JsonParser();
						JSONElement json = parser.Decode(text);
						return FromJSONElement(type, json);
					}
				}

				return null;
			}

			public static T FromFile<T>(string fileName)
			{
				JSONElement json = FromFileName(fileName);
				return FromJSONElement<T>(json);
			}

			public static JSONElement FromFileName(string fileName)
			{
				if (!string.IsNullOrEmpty(fileName))
				{
#if WINDOWS_PHONE
					Stream stream = Microsoft.Xna.Framework.TitleContainer.OpenStream(path);
					using (StreamReader reader = new StreamReader(stream)) {
#else
					using (StreamReader reader = new StreamReader(fileName))
					{
						JsonParser parser = new JsonParser();
						return parser.Decode(reader.ReadToEnd());
#endif // WINDOWS_PHONE
					}
				}

				return null;
			}


			public static T FromTextAsset<T>(TextAsset asset)
			{
				if (asset != null)
				{
					return FromJSONString<T>(asset.text);
				}

				return default(T);
			}
			
			public static JSONElement ToJSONElement<T>(T obj, object defualtObject = null)
			{
				JSONElement node = null;

				if (obj != null)
				{
					Type objType = obj.GetType();
					ObjectConverter converter = GetConverter(objType);
				
					if (ShouldWriteObject(objType, converter, obj, defualtObject))
					{
						//IJSONConversionCallbackReceiver callback
						if (obj is IJSONConversionCallbackReceiver)
							((IJSONConversionCallbackReceiver)obj).OnConvertToJSONElement(node);	

						//If the object is an array create a node for the array and convert each element individually
						if (objType.IsArray)
						{
							object[] arrayField = obj as object[];

							if (arrayField != null && arrayField.Length > 0)
							{
								JSONArray arrayJSONElement = new JSONArray();
								arrayJSONElement._elements = new List<JSONElement>();

								//Append all child nodes
								foreach (object arrayItem in arrayField)
								{
									JSONElement arrayElementJSONElement = ToJSONElement(arrayItem);
									arrayJSONElement._elements.Add(arrayElementJSONElement);
								}

								node = arrayJSONElement;
							}
						}
						else
						{
							string tag = GetJSONTag(objType);

							if (!string.IsNullOrEmpty(tag))
							{
								//If the object has an associated converter class, convert the object using it
								if (converter != null)
								{
									node = converter._onConvertToJSONElement(obj);
								}
								//Otherwise convert each field to an object
								else
								{
									JSONObject jsonObject = new JSONObject();
									jsonObject._fields = new Dictionary<string, JSONElement>();

									JSONField[] jsonFields = GetJSONFields(objType);

									//Create a default version of this to compare with?
									if (defualtObject == null)
									{
										defualtObject = CreateInstance(objType);
									}

									foreach (JSONField jsonField in jsonFields)
									{
										object fieldObj = jsonField.GetValue(obj);
										object defualtFieldObj = jsonField.GetValue(defualtObject);

										JSONElement fieldJSONElement = ToJSONElement(fieldObj, defualtFieldObj);

										if (fieldJSONElement != null)
										{
											AddRuntimeTypeInfoIfNecessary(jsonField.GetFieldType(), fieldObj.GetType(), fieldJSONElement);
											jsonObject._fields[jsonField.GetID()] = fieldJSONElement;
										}
									}

									node = jsonObject;
								}
							}
						}				
					}
				}

				return node;
			}

			public static string ToJSONString<T>(T obj)
			{
				JSONElement jsonElement = ToJSONElement(obj);
				return JSONWriter.ToString(jsonElement);
			}

#if UNITY_EDITOR
			public static bool ToFile<T>(T obj, string fileName)
			{
				try
				{
					string jsonString = ToJSONString(obj);
					File.WriteAllText(fileName, jsonString);
					return true;
				}
				catch
				{
					return false;
				}
			}
#endif

			public static T CreateCopy<T>(T obj)
			{
				if (obj != null)
				{
					//Convert to JSON and then back again as a new node
					JSONElement node = ToJSONElement(obj);
					object defaultObj = CreateInstance(obj.GetType());
					return (T)FromJSONElement(obj.GetType(), node, defaultObj);
				}

				return default(T);
			}

			public static bool DoesAssetContainNode<T>(TextAsset asset)
			{
				if (asset != null)
				{
					//Hmm this is harder as not all objects have a type?
					//Maybe only return true if find one?
					JsonParser parser = new JsonParser();
					JSONElement json = parser.Decode(asset.text);

				
				}

				return false;
			}
			
			public static JSONField[] GetJSONFields(Type objType)
			{
				//Find all fields in type that have been marked with the JSONFieldAttribute
				List<JSONField> JSONFields = new List<JSONField>();

				BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

				//First find fields
				FieldInfo[] fields = objType.GetFields(bindingAttr);
				foreach (FieldInfo field in fields)
				{
					JSONFieldAttribute fieldAttribute = SystemUtils.GetAttribute<JSONFieldAttribute>(field);

					if (fieldAttribute != null)
					{
						JSONField JSONField = new JSONField(fieldAttribute, field);
						JSONFields.Add(JSONField);
					}
				}

				//Then find all properties
				PropertyInfo[] properties = objType.GetProperties(bindingAttr);
				foreach (PropertyInfo property in properties)
				{
					JSONFieldAttribute fieldAttribute = SystemUtils.GetAttribute<JSONFieldAttribute>(property);

					if (fieldAttribute != null)
					{
						JSONField JSONField = new JSONField(fieldAttribute, property);
						JSONFields.Add(JSONField);
					}
				}

				return JSONFields.ToArray();
			}

			public static bool FindJSONField(Type objType, string id, out JSONField field)
			{
				JSONField[] JSONFields = GetJSONFields(objType);
				foreach (JSONField JSONField in JSONFields)
				{
					if (JSONField.GetID() == id)
					{
						field = JSONField;
						return true;
					}
				}

				field = new JSONField();
				return false;
			}

			public static object[] GetJSONFieldInstances(object obj)
			{
				JSONField[] JSONFields = GetJSONFields(obj.GetType());
				List<object> fieldInstances = new List<object>();

				foreach (JSONField JSONField in JSONFields)
				{
					if (JSONField.GetFieldType().IsArray)
					{
						object[] array = (object[])JSONField.GetValue(obj);
						if (array != null)
							fieldInstances.AddRange(array);
					}
					else
					{
						object newObj = JSONField.GetValue(obj);
						if (newObj != null)
							fieldInstances.Add(newObj);
					}
				}

				return fieldInstances.ToArray();
			}

			public static object GetJSONFieldInstance(object obj, string id)
			{
				JSONField fieldInfo;

				if (FindJSONField(obj.GetType(), id, out fieldInfo))
				{
					return fieldInfo.GetValue(obj);
				}

				return null;
			}

			public static JSONElement AppendFieldObject<T>(JSONObject parentNode, T obj, string id)
			{
				JSONElement childJSONElement = ToJSONElement(obj);
				parentNode._fields[id] = childJSONElement;
				return childJSONElement;
			}

			public static T FieldObjectFromJSONElement<T>(JSONElement parentNode, string id, T defualtObject = default(T))
			{
				JSONElement childJSONElement = JSONUtils.FindChildWithAttributeValue(parentNode, kJSONFieldIdAttributeTag, id);
				return (T)FromJSONElement(typeof(T), childJSONElement, defualtObject);
			}
			#endregion

			#region Private Functions
			private static string GetJSONTag(Type type)
			{
				string JSONTag = null;

				BuildTypeMap();

				Type objType = GetObjectConversionType(type);

				if (!_typeToTagMap.TryGetValue(objType, out JSONTag))
				{
					throw new Exception("Type '" + objType + "' is not mapped to an JSON tag, check it has the JSONTagAttribute or has a JSONObjectConverter class.");
				}

				return JSONTag;
			}

			private static void BuildTypeMap()
			{
				if (_tagToTypeMap == null || _typeToTagMap == null || _converterMap == null)
				{
					_tagToTypeMap = new Dictionary<string, Type>();
					_typeToTagMap = new Dictionary<Type, string>();
					_converterMap = new Dictionary<Type, ObjectConverter>();

					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

					foreach (Assembly assembly in assemblies)
					{
						Type[] types = assembly.GetTypes();

						foreach (Type type in types)
						{
							//Types can either be marked with a JSONTagAttribute or be a converter class marked with JSONObjectConverterAttribute
							JSONObjectAttribute attribute = SystemUtils.GetAttribute<JSONObjectAttribute>(type);
							if (attribute != null)
							{
								//Either use tag from attribute or the type if none was defined
								string JSONTag = attribute.JSONTag;
								if (string.IsNullOrEmpty(JSONTag))
									JSONTag = type.Name;

								if (_tagToTypeMap.ContainsKey(JSONTag))
								{
									throw new Exception("Can't initialize JSONTagAttribute for " + type.FullName + " as it's JSON tag '" + JSONTag + "' is already used by another class");
								}

								_tagToTypeMap.Add(JSONTag, type);
								_typeToTagMap.Add(type, JSONTag);
							}
							else
							{
								JSONObjectConverterAttribute converterAttribute = SystemUtils.GetAttribute<JSONObjectConverterAttribute>(type);

								if (converterAttribute != null)
								{
									if (_converterMap.ContainsKey(converterAttribute.ObjectType))
									{
										throw new Exception("Can't initialize JSONObjectConverterAttribute for " + type.FullName + " as already have a converter for type " + converterAttribute.ObjectType);
									}
									if (_tagToTypeMap.ContainsKey(converterAttribute.JSONTag))
									{
										throw new Exception("Can't initialize JSONObjectConverterAttribute for " + type.FullName + " as it's JSON tag '" + converterAttribute.JSONTag + "' is already used by another class");
									}

									ObjectConverter converter = new ObjectConverter(SystemUtils.GetStaticMethodAsDelegate<OnConvertToJSONDelegate>(type, converterAttribute.OnConvertToJSONElementMethod), 
																					SystemUtils.GetStaticMethodAsDelegate<OnConvertFromJSONDelegate>(type, converterAttribute.OnConvertFromJSONElementMethod),
																					SystemUtils.GetStaticMethodAsDelegate<ShouldWriteDelegate>(type, converterAttribute.ShouldWriteMethod));
									_converterMap.Add(converterAttribute.ObjectType, converter);
									_tagToTypeMap.Add(converterAttribute.JSONTag, converterAttribute.ObjectType);
									_typeToTagMap.Add(converterAttribute.ObjectType, converterAttribute.JSONTag);
								}
							}										
						}
					}
				}
			}

			private static string kTypeFieldName = "__objectType_";

			private static Type GetRuntimeType(JSONElement node)
			{
				Type type = null;

				if (node is JSONObject)
				{
					JSONObject jsonObj = (JSONObject)node;
					JSONElement typeElement;

					//If the object contains a valid object type field, find type from attribute map
					if (jsonObj._fields.TryGetValue(kTypeFieldName, out typeElement) && typeElement is JSONString)
					{
						string JSONTag = ((JSONString)typeElement)._value;
						BuildTypeMap();

						if (!_tagToTypeMap.TryGetValue(JSONTag, out type))
						{
							Debug.LogError("JSON Node tag '" + JSONTag + "' is not mapped to a type, check a class has a JSONTagAttribute with the same tag");
						}
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
				else if(obj == null)
				{
					return false;
				}
				//If object is null by default OR is of a different then always write 
				else if(defualtObj == null || defualtObj.GetType() != obj.GetType())
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
					JSONField[] JSONFields = GetJSONFields(obj.GetType());
					foreach (JSONField JSONField in JSONFields)
					{
						ObjectConverter fieldConverter = GetConverter(JSONField.GetFieldType());
						object fieldObj = JSONField.GetValue(obj);
						object defualtFieldObj = JSONField.GetValue(defualtObj);

						if (ShouldWriteObject(JSONField.GetFieldType(), fieldConverter, fieldObj, defualtFieldObj))
							return true;
					}
				}					

				return false;
			}		

			private static Type ReadTypeFromRuntimeTypeInfo(JSONElement node)
			{
				Type type = null;

				if (node != null)
				{
					JSONElement childJSONElement = JSONUtils.FindChildWithAttributeValue(node, kJSONElementRuntimeTypeAttributeTag, "");
					if (childJSONElement != null)
					{
						type = (Type)FromJSONElement(typeof(Type), childJSONElement);
					}
				}

				return type;
			}

			private static void AddRuntimeTypeInfoIfNecessary(Type fieldType, Type objType, JSONElement parentNode)
			{
				//If field is abstract or generic AND node is abstract or generic, insert type node so we can convert it
				if (parentNode is JSONObject && NeedsRuntimeTypeInfo(fieldType, objType))
				{					
					JSONElement typeNode = ToJSONElement(objType);
					((JSONObject)parentNode)._fields[kJSONElementRuntimeTypeAttributeTag] = typeNode;
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