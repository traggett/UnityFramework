
namespace Framework
{
	namespace Graphics
	{
		namespace MeshInstancing
		{
			namespace GPUAnimations
			{
				public class MultiLayerGPUAnimatedRenderer : GPUAnimatorRenderer
				{
					#region Private Data	
					private float[] _secondLayerWeights;
					private float[] _secondLayerMainAnimationFrames;
					private float[] _secondLayerMainAnimationWeights;
					private float[] _secondLayerBackgroundAnimationFrames;
					#endregion

					#region GPUAnimatorRenderer
					protected override bool Initialise()
					{
						if (base.Initialise())
						{
							_secondLayerWeights = new float[_maxMeshes];
							_secondLayerMainAnimationFrames = new float[_maxMeshes];
							_secondLayerMainAnimationWeights = new float[_maxMeshes];
							_secondLayerBackgroundAnimationFrames = new float[_maxMeshes];

							return true;
						}

						return false;
					}

					protected override void UpdateProperties()
					{
						base.UpdateProperties();

						int numRenderedInstances = GetNumRenderedInstances();

						int index = 0;
						for (int i = 0; i < numRenderedInstances; i++)
						{
							int instanceIndex = GetRenderedInstanceIndex(i);

							GPUAnimator animator = (GPUAnimator)_instanceData[instanceIndex]._animator;

							//Work out what other layer has heighest weight
							int secondLayerIndex = 1;
							float secondLayerWeight = animator.GetLayerWeight(secondLayerIndex);

							for (int j = 2; j < animator.GetNumLayers(); j++)
							{
								float weight = animator.GetLayerWeight(j);

								if (weight > secondLayerWeight)
								{
									secondLayerIndex = j;
									secondLayerWeight = weight;
								}
							}

							_secondLayerWeights[index] = secondLayerWeight;
							_secondLayerMainAnimationFrames[index] = animator.GetMainAnimationFrame(secondLayerIndex);
							_secondLayerMainAnimationWeights[index] = animator.GetMainAnimationWeight(secondLayerIndex);
							_secondLayerBackgroundAnimationFrames[index] = animator.GetBackgroundAnimationFrame(secondLayerIndex);

							index++;
						}

						_propertyBlock.SetFloatArray("_layerTwoWeight", _secondLayerWeights);
						_propertyBlock.SetFloatArray("_layerTwoMainAnimFrame", _secondLayerMainAnimationFrames);
						_propertyBlock.SetFloatArray("_layerTwoMainAnimWeight", _secondLayerMainAnimationWeights);
						_propertyBlock.SetFloatArray("_layerTwoBackgroundAnim", _secondLayerBackgroundAnimationFrames);
					}
					#endregion
				}
			}
		}
	}
}
