using System;
using System.Collections.Generic;
using System.Reflection;

namespace Framework
{
	namespace Utils
	{
		public static class SystemUtils
		{
			private static Type[] _types;
			private static Assembly[] _assemblies;

			public static Assembly[] GetAssemblies()
			{
				if (_assemblies == null)
				{
					_assemblies = AppDomain.CurrentDomain.GetAssemblies();
				}
				
				return _assemblies;
			}

			public static Type[] GetAllTypes()
			{
				if (_types == null)
				{
					List<Type> result = new List<Type>();
					Assembly[] assemblies = GetAssemblies();

					foreach (Assembly assembly in assemblies)
					{
						Type[] types = assembly.GetTypes();

						foreach (Type type in types)
						{
							if (type.Name.Contains("_Injected"))
								continue;

							result.Add(type);
						}
					}

					_types = result.ToArray();
				}

				return _types;
			}

			public static Type[] GetAllSubTypes(Type baseType, bool allowAbstract = false)
			{
				List<Type> result = new List<Type>();

				Type[] types = GetAllTypes();

				foreach (Type type in types)
				{
					if (type != baseType && (allowAbstract || (!type.IsAbstract && !type.IsInterface)) && IsTypeOf(baseType, type))
					{
						result.Add(type);
					}
				}

				return result.ToArray();
			}

			public static bool IsTypeOf(Type baseType, Type type)
			{
				if (type.IsGenericType)
				{
					return type.GetGenericTypeDefinition() == baseType;
				}

				if (baseType.IsInterface)
				{
					return baseType.IsAssignableFrom(type);
				}

				return baseType == type || type.IsSubclassOf(baseType);
			}

			public static Type GetGenericImplementationType(Type generic, Type toCheck, int index = 0)
			{
				Type[] types = GetGenericImplementationTypes(generic, toCheck);

				if (types != null && index < types.Length)
					return types[index];

				return null;
			}

			public static Type[] GetGenericImplementationTypes(Type generic, Type toCheck)
			{
				if (generic.IsGenericType)
				{
					while (toCheck != null && toCheck != typeof(object))
					{
						if (toCheck.IsGenericType && toCheck.GetGenericTypeDefinition() == generic)
						{
							return toCheck.GetGenericArguments();
						}

						foreach (Type interfaceType in toCheck.GetInterfaces())
						{
							if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == generic)
							{
								return interfaceType.GetGenericArguments();
							}
						}

						toCheck = toCheck.BaseType;
					}
				}

				return null;
			}

			public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
			{
				while (toCheck != null && toCheck != typeof(object))
				{
					Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

					if (generic == cur)
					{
						return true;
					}

					foreach (Type interfaceType in cur.GetInterfaces())
					{
						cur = interfaceType.IsGenericType ? interfaceType.GetGenericTypeDefinition() : interfaceType;

						if (generic == cur)
						{
							return true;
						}
					}

					toCheck = toCheck.BaseType;
				}

				return false;
			}

			public static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
			{
				T attribute = null;

				try
				{
					object[] customAttributes = memberInfo.GetCustomAttributes(typeof(T), false);
					
					if (customAttributes.Length == 1 && customAttributes[0] is T)
					{
						attribute = (T)customAttributes[0];
					}
				}
				catch
				{
					return null;
				}

				return attribute;
			}

			public static T[] GetAttributes<T>(MemberInfo memberInfo) where T : Attribute
			{
				T[] attributes;

				try
				{
					object[] customAttributes = memberInfo.GetCustomAttributes(typeof(T), false);

					attributes = new T[customAttributes.Length];

					for (int i = 0; i < customAttributes.Length; i++)
					{
						attributes[i] = (T)customAttributes[i];
					}
				}
				catch
				{
					return null;
				}

				return attributes;
			}

			public static T GetStaticMethodAsDelegate<T>(Type type, string name) where T : class
			{
				T function = null;
				BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
				MethodInfo method = type.GetMethod(name, bindingFlags);

				try
				{
					function = Delegate.CreateDelegate(typeof(T), method) as T;
				}
				catch
				{
					throw new Exception(type + " doesn't not have a static method named " + name + ".");
				}

				return function;
			}

			public static string GetTypeName(Type type)
			{
				if (type == null)
					return null;
				else if (type == typeof(float))
					return "Float";
				else if (type == typeof(string))
					return "String";
				else if (type == typeof(int))
					return "Int";
				else if (type == typeof(bool))
					return "Bool";

				if (type.IsGenericType)
				{
					Type genericType = type.GetGenericTypeDefinition();
					return genericType.Name.Remove(genericType.Name.IndexOf('`'));
				}

				return type.Name;
			}
		}
	}
}