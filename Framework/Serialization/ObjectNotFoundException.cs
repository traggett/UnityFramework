using System;

namespace Framework
{
	using Utils;

	namespace Serialization
	{
		public class ObjectNotFoundException : Exception
		{
			private Type _objectType;

			public ObjectNotFoundException(Type objectType) : base(SystemUtils.GetTypeName(objectType) + "Not found")
			{

			}
		}
	}
}