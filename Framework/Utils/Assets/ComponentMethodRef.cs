using System;
using System.Reflection;

using UnityEngine;

namespace Framework
{

	namespace Utils
	{
		[Serializable]
		public struct ComponentMethodRef<T>
		{
			#region Public Data
			[SerializeField]
			private ComponentRef<Component> _component;
			[SerializeField]
			private string _methodName;
			#endregion

#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorCollapsed;
#endif

			public static implicit operator string(ComponentMethodRef<T> property)
			{
				return property._component + "." + property._methodName + "()";
			}

			public T RunMethod()
			{
				T returnObj = default(T);

				Component component = _component.GetComponent();

				if (component != null)
				{
					MethodInfo method = component.GetType().GetMethod(_methodName);

					if (method != null)
					{
						returnObj = (T)method.Invoke(component, null);
					}
				}

				return returnObj;
			}

			public Component GetComponent()
			{
				return _component.GetComponent();
			}

#if UNITY_EDITOR
			public ComponentMethodRef(ComponentRef<Component> componentRef, string methodName)
			{
				_component = componentRef;
				_methodName = methodName;
				_editorCollapsed = false;
			}

			public ComponentRef<Component> GetComponentRef()
			{
				return _component;
			}

			public string GetMethodName()
			{
				return _methodName;
			}
#endif
		}
	}
}