using UnityEngine;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public abstract class GPUAnimatorBase : MonoBehaviour
			{
				#region Public Data
				[HideInInspector]
				public GPUAnimatorRenderer _renderer;
				public float _sphericalBoundsRadius;
				#endregion

				#region Public Interface
				public abstract void Initialise(GPUAnimatorRenderer renderer);
				public abstract float GetCurrentAnimationFrame();
				public abstract float GetCurrentAnimationWeight();
				public abstract float GetPreviousAnimationFrame();

				public abstract SkinnedMeshRenderer GetSkinnedMeshRenderer();
				public abstract float GetSphericalBoundsRadius();
				public abstract Matrix4x4 GetWorldMatrix();
				public abstract Vector3 GetWorldPos();
				public abstract Vector3 GetWorldScale();
				#endregion
			}
		}
    }
}