using System;
using System.Reflection;

using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		//Cant have generic type of <void> so have to manually overrider return type in its own class
		[Serializable]
		public sealed class ComponentVoidMethodRef : ComponentMethodRef<object>
		{
			public new void RunMethod()
			{
				Component component = _object.GetComponent();

				if (component != null)
				{
					MethodInfo method = component.GetType().GetMethod(_methodName);

					if (method != null)
					{
						method.Invoke(component, null);
					}
				}
			}

			protected override Type GetReturnType()
			{
				return typeof(void);
			}
		}
	}
}