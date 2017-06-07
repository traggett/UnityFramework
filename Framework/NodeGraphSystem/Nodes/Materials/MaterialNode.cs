using UnityEngine;

namespace Framework
{
	namespace NodeGraphSystem
	{
		public abstract class MaterialNode : Node
		{
			#region Public Data
			public MaterialNodeInputField _material = new MaterialNodeInputField();
			public NodeInputField<string> _propertyName = string.Empty;
			#endregion

			#region Private Data
			protected int _cachedShaderID = 0;
			protected string _cachedShaderVarialble;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return MaterialNodes.kNodeColor;
			}
#endif
			#endregion

			protected bool UpdateCachedShader()
			{
				if (_cachedShaderID == 0 || string.Compare(_propertyName, _cachedShaderVarialble) != 0)
				{
					_cachedShaderID = Shader.PropertyToID(_propertyName);
					_cachedShaderVarialble = _propertyName;
					return true;
				}

				if (Application.isEditor)
					return true;

				return false;
			}
		}
	}
}