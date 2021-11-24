using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    namespace AnimationSystem
    {
        public class AnimatorLODControllerOverrides : MonoBehaviour
        {
            #region Public Data
            [Serializable]
            public struct AnimationClipData
            {
                public AnimationClip _originalClip;
                public AnimationClip _overrideClip;
            }

            [Serializable]
            public struct AnimatorLOD
            {
                [Range(1, 8)]
                public int _minLODLevel;
                public AvatarMask _avatarMask;
                [HideInInspector]
                public AnimationClipData[] _overrideClips;
            }

            [Serializable]
            public struct AnimatorData
            {
                public RuntimeAnimatorController _controller;
                public AnimatorLOD[] _animatorLODs;
            }

            public AnimatorData[] _animatorData;
            #endregion

            #region Private Data
            private static List<AnimatorLODControllerOverrides> _controllerOverrides = new List<AnimatorLODControllerOverrides>();
            #endregion

            #region Unity Messages
            private void Awake()
            {
                _controllerOverrides.Add(this);
            }

            private void OnDestory()
            {
                _controllerOverrides.Remove(this);
            }
            #endregion

            #region Public Interface
            public static AnimatorLODOverrideController GetOverrideController(Animator animator)
            {
                foreach (AnimatorLODControllerOverrides controllerOverrides in _controllerOverrides)
                {
                    for (int i = 0; i < controllerOverrides._animatorData.Length; i++)
                    {
                        if (controllerOverrides._animatorData[i]._controller == animator.runtimeAnimatorController)
                        {
                            AnimatorLODOverrideController controller = new AnimatorLODOverrideController(animator.runtimeAnimatorController, controllerOverrides._animatorData[i]._animatorLODs)
                            {
                                name = animator.runtimeAnimatorController.name + " (LOD)"
                            };
                            return controller;
                        }
                    }
                }

                return null;
            }
            #endregion
        }
    }
}