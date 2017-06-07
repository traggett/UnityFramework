using UnityEngine;
using System;
using System.Reflection;

namespace Framework
{
	using StateMachineSystem;
	using Utils;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[ConditionalCategory("")]
		public class ConditionalMethodReturnsTrue : IConditional
		{
			public ComponentMethodRef<bool> _method = new ComponentMethodRef<bool>();

			private Component _component;
			private MethodInfo _methodInfo;

			#region IConditional
#if UNITY_EDITOR
			public string GetEditorDescription()
			{
				return "(<b>" + _method + "</b>)";
			}

			public bool AllowInverseVariant()
			{
				return true;
			}
#endif

			public void OnStartConditionChecking(StateMachine stateMachine)
			{
				_component = _method._object.GetComponent();
				if (_component != null)
					_methodInfo = _component.GetType().GetMethod(_method._methodName);
			}
			
			public bool IsConditionMet(StateMachine stateMachine)
			{
				if (_methodInfo != null && (bool)_methodInfo.Invoke(_component, null))
				{
					return true;
				}

				return false;
			}

			public void OnEndConditionChecking(StateMachine stateMachine)
			{

			}
			#endregion
		}
	}
}