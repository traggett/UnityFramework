using UnityEngine;

namespace Framework
{
    namespace AnimationSystem
    {
        [RequireComponent(typeof(Renderer))]
        public class AnimatorLODRenderer : MonoBehaviour
        {
            #region Public Data
            public Renderer _renderer;
            public AnimatorLODGroup _animatorLOD;
            #endregion

            #region MonoBehaviour
            private void OnWillRenderObject()
            {
                _animatorLOD.OnLODRendered(this, Camera.current);
            }
            #endregion
        }
    }
}