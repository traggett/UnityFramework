using System;
using System.Collections.Generic;

namespace Framework
{
	namespace LocalisationSystem
	{
		[Serializable]
		public sealed class LocalisationKeys
		{
			#region Public Data
			public HashSet<string> _keys = new HashSet<string>();
			#endregion
		}
	}
}