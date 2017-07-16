namespace Framework
{
	using Serialization;
	
	namespace SaveSystem
	{
		public abstract class SaveDataBlock
		{
			public void SetValue(string id, object value)
			{
				SerializedObjectMemberInfo valueField;
				if (SerializedObjectMemberInfo.FindSerializedField(value.GetType(), id, out valueField))
				{
					valueField.SetValue(this, value);
				}
			}
		}
	}
}