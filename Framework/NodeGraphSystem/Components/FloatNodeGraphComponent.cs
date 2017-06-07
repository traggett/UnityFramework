namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		public class FloatNodeGraphComponent : NodeGraphComponent, IValueSource<float>
		{
			#region IValueSource<float>
			public float GetValue()
			{
				if (_nodegraph != null)
					return _nodegraph.GetValue<float>();

				return 0.0f;
			}
			#endregion
		}
	}
}