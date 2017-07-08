using System;

using UnityEngine;

namespace Framework
{
	using DynamicValueSystem;
	using Utils;
	using Maths;

	namespace NodeGraphSystem
	{
		public class NodeGraphComponent : MonoBehaviour, IValueSourceContainer
		{
			#region Public Data
			public bool _unscaledTime = false;
			public NodeGraphRefProperty _nodeGraphRef;
			#endregion

			#region Protected Data
			protected NodeGraph _nodegraph;
			#endregion

			#region Private Data 
			private Node[] _outputNodes;

			private interface IInput
			{
				int GetNodeId();
			}

			#region Input Serializable Structs (Curse you Unity for not serialising genric types!)			
			[Serializable]
			public class FloatInput : IInput, IValueSource<float>
			{
				public int _nodeId;
				public DynamicFloat _valueSource;
				public int GetNodeId() { return _nodeId; }
				public float GetValue() { return _valueSource; }
			}
			[SerializeField]
			private FloatInput[] _floatInputs;

			[Serializable]
			public class IntInput : IInput, IValueSource<int>
			{
				public int _nodeId;
				public DynamicInt _valueSource;
				public int GetNodeId() { return _nodeId; }
				public int GetValue() { return _valueSource; }
			}
			[SerializeField]
			private IntInput[] _intInputs;

			[Serializable]
			public class FloatRangeInput : IInput, IValueSource<FloatRange>
			{
				public int _nodeId;
				public DynamicFloatRange _valueSource;
				public int GetNodeId() { return _nodeId; }
				public FloatRange GetValue() { return _valueSource; }
			}
			[SerializeField]
			private FloatRangeInput[] _floatRangeInputs;

			[Serializable]
			public class IntRangeInput : IInput, IValueSource<IntRange>
			{
				public int _nodeId;
				public DynamicIntRange _valueSource;
				public int GetNodeId() { return _nodeId; }
				public IntRange GetValue() { return _valueSource; }
			}
			[SerializeField]
			private IntRangeInput[] _intRangeInputs;

			[Serializable]
			public class Vector2Input : IInput, IValueSource<Vector2>
			{
				public int _nodeId;
				public DynamicVector2 _valueSource;
				public int GetNodeId() { return _nodeId; }
				public Vector2 GetValue() { return _valueSource; }
			}
			[SerializeField]
			private Vector2Input[] _vector2Inputs;

			[Serializable]
			public class Vector3Input : IInput, IValueSource<Vector3>
			{
				public int _nodeId;
				public DynamicVector3 _valueSource;
				public int GetNodeId() { return _nodeId; }
				public Vector3 GetValue() { return _valueSource; }
			}
			[SerializeField]
			private Vector3Input[] _vector3Inputs;

			[Serializable]
			public class Vector4Input : IInput, IValueSource<Vector4>
			{
				public int _nodeId;
				public DynamicVector4 _valueSource;
				public int GetNodeId() { return _nodeId; }
				public Vector4 GetValue() { return _valueSource; }
			}
			[SerializeField]
			private Vector4Input[] _vector4Inputs;

			[Serializable]
			public class QuaternionInput : IInput, IValueSource<Quaternion>
			{
				public int _nodeId;
				public DynamicQuaternion _valueSource;
				public int GetNodeId() { return _nodeId; }
				public Quaternion GetValue() { return _valueSource; }
			}
			[SerializeField]
			private QuaternionInput[] _quaternionInputs;

			[Serializable]
			public class ColorInput : IInput, IValueSource<Color>
			{
				public int _nodeId;
				public DynamicColor _valueSource;
				public int GetNodeId() { return _nodeId; }
				public Color GetValue() { return _valueSource; }
			}
			[SerializeField]
			private ColorInput[] _colorInputs;

			[Serializable]
			public class StringInput : IInput, IValueSource<string>
			{
				public int _nodeId;
				public DynamicString _valueSource;
				public int GetNodeId() { return _nodeId; }
				public string GetValue() { return _valueSource; }
			}
			[SerializeField]
			private StringInput[] _stringInputs;

			[Serializable]
			public class BoolInput : IInput, IValueSource<bool>
			{
				public int _nodeId;
				public DynamicBool _valueSource;
				public int GetNodeId() { return _nodeId; }
				public bool GetValue() { return _valueSource; }
			}
			[SerializeField]
			private BoolInput[] _boolInputs;

			[Serializable]
			public class GradientInput : IInput, IValueSource<Gradient>
			{
				public int _nodeId;
				public DynamicGradient _valueSource;
				public int GetNodeId() { return _nodeId; }
				public Gradient GetValue() { return _valueSource; }
			}
			[SerializeField]
			private GradientInput[] _gradientInputs;

			[Serializable]
			public class AnimationCurveInput : IInput, IValueSource<AnimationCurve>
			{
				public int _nodeId;
				public DynamicAnimationCurve _valueSource;
				public int GetNodeId() { return _nodeId; }
				public AnimationCurve GetValue() { return _valueSource; }
			}
			[SerializeField]
			private AnimationCurveInput[] _animationCurveInputs;

			[Serializable]
			public class ComponentInput : IInput, IValueSource<Component>
			{
				public int _nodeId;
				public DynamicComponentRef _valueSource;
				public int GetNodeId() { return _nodeId; }
				public Component GetValue() { return _valueSource; }
			}
			[SerializeField]
			private ComponentInput[] _componentInputs;

			[Serializable]
			public class TransformInput : IInput, IValueSource<Transform>
			{
				public int _nodeId;
				public DynamicTransformRef _valueSource;
				public int GetNodeId() { return _nodeId; }
				public Transform GetValue() { return _valueSource; }
			}
			[SerializeField]
			private TransformInput[] _transformInputs;

			[Serializable]
			public class GameObjectInput : IInput, IValueSource<GameObject>
			{
				public int _nodeId;
				public DynamicGameObjectRef _valueSource;
				public int GetNodeId() { return _nodeId; }
				public GameObject GetValue() { return _valueSource; }
			}
			[SerializeField]
			private GameObjectInput[] _gameObjectInputs;

			[Serializable]
			public class MaterialInput : IInput, IValueSource<Material>
			{
				public int _nodeId;
				public DynamicMaterialRef _valueSource;
				public int GetNodeId() { return _nodeId; }
				public Material GetValue() { return _valueSource.GetValue().GetMaterial(); }
			}
			[SerializeField]
			private MaterialInput[] _materialInputs;

			[Serializable]
			public class TextureInput : IInput, IValueSource<Texture>
			{
				public int _nodeId;
				public DynamicTextureRef _valueSource;
				public int GetNodeId() { return _nodeId; }
				public Texture GetValue() { return _valueSource; }
			}
			[SerializeField]
			private TextureInput[] _textureInputs;
			#endregion

			#endregion

			#region MonoBehaviour
			void OnEnable()
			{
				_nodegraph = null;

				if (Application.isPlaying)
				{
					LoadNodeGraph();
					if (_nodegraph != null)
						_nodegraph.Init();
				}
					
			}

			void Update()
			{

			}

			void LateUpdate()
			{
				if (_nodegraph != null)
					_nodegraph.Update(_unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
			}
			#endregion

			#region Public Interface
			public void LoadNodeGraph()
			{
				_nodegraph = _nodeGraphRef.LoadNodeGraph();

				if (_nodegraph != null)
				{
					GameObjectRef.FixupGameObjectRefs(this.gameObject, _nodegraph);
					FixupInputs();

					_outputNodes = _nodegraph.GetOutputNodes();
				}
			}

			public NodeGraph GetNodeGraph()
			{
				return _nodegraph;
			}

			public Node[] GetOutputNodes()
			{
				return _outputNodes;
			}
			#endregion

			#region IValueSourceContainer 
			public object GetValueSource(int index)
			{
				if (_outputNodes != null && _outputNodes.Length > index)
					return _outputNodes[index];

				return null;
			}

#if UNITY_EDITOR
			public int GetNumberOfValueSource()
			{
				EnsureNodeGraphIsLoaded();

				if (_outputNodes != null)
					return _outputNodes.Length;

				return 0;
			}

			public string GetValueSourceName(int index)
			{
				EnsureNodeGraphIsLoaded();

				if (_outputNodes != null && _outputNodes.Length > index)
					return "(OutputNode) " + _outputNodes[index]._editorDescription;

				return null;
			}
#endif
			#endregion

			#region Private Functions 
			private void FixupInputs()
			{
				FixupInputs<FloatInput, float>(ref _floatInputs);
				FixupInputs<IntInput, int>(ref _intInputs); 
				FixupInputs<FloatRangeInput, FloatRange>(ref _floatRangeInputs);
				FixupInputs<IntRangeInput, IntRange>(ref _intRangeInputs); 
				FixupInputs<Vector2Input, Vector2>(ref _vector2Inputs);
				FixupInputs<Vector3Input, Vector3>(ref _vector3Inputs);
				FixupInputs<Vector4Input, Vector4>(ref _vector4Inputs); 
				FixupInputs<QuaternionInput, Quaternion>(ref _quaternionInputs); 
				FixupInputs<ColorInput, Color>(ref _colorInputs);
				FixupInputs<StringInput, string>(ref _stringInputs);
				FixupInputs<BoolInput, bool>(ref _boolInputs);
				FixupInputs<AnimationCurveInput, AnimationCurve>(ref _animationCurveInputs);
				FixupInputs<GradientInput, Gradient>(ref _gradientInputs);
				FixupInputs<TransformInput, Transform>(ref _transformInputs);
				FixupInputs<GameObjectInput, GameObject>(ref _gameObjectInputs);
				FixupInputs<ComponentInput, Component>(ref _componentInputs);
				FixupInputs<MaterialInput, Material>(ref _materialInputs);
				FixupInputs<TextureInput, Texture>(ref _textureInputs); 
			}
			
			private void FixupInputs<TInput, TValue>(ref TInput[] inputArray) where TInput : class, IInput, IValueSource<TValue>
			{
				if (inputArray != null)
				{
					for (int i= 0; i<inputArray.Length; i++)
					{
						Node node = _nodegraph.GetNode(inputArray[i].GetNodeId());

						if (node != null && node is InputNode<TValue>)
						{
							((InputNode<TValue>)node).SetInputSource(inputArray[i]);
						}
						else
						{
							Debug.LogError("Serialized input node data mismatch");
						}
					}
				}
			}

#if UNITY_EDITOR
			private void EnsureNodeGraphIsLoaded()
			{
				if (!Application.isPlaying)
				{
					if (_nodegraph == null || _nodegraph._nodes.Length == 0)
						LoadNodeGraph();
				}	
			}
#endif
			#endregion
		}
	}
}