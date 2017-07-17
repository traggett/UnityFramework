using System;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public struct Condition
		{
			#region Public Data
			public IConditional _conditional;
			public bool _not;
			#endregion

			#region Private Data		
#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorFoldout;

#endif
			#endregion
		}
	}
}