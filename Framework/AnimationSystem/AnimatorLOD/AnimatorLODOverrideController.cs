using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    namespace AnimationSystem
    {
        public class AnimatorLODOverrideController : AnimatorOverrideController
        {
            #region Private Data
            private struct AnimationsLOD
            {
                public readonly int _minLODLevel;
                public readonly List<KeyValuePair<AnimationClip, AnimationClip>> _overrideClipsList;
                public readonly Avatar _avatar;

                public AnimationsLOD(int lodLevel, List<KeyValuePair<AnimationClip, AnimationClip>> overrideClipsList, Avatar avatar)
                {
                    _minLODLevel = lodLevel;
                    _overrideClipsList = overrideClipsList;
                    _avatar = avatar;
                }
            }
            private AnimationsLOD[] _animationsLODs;
            private int _currentLOD;
            private int _currentAnimationLODSet;
            #endregion

            #region Public Interface
            public AnimatorLODOverrideController(RuntimeAnimatorController controller, AnimatorLODControllerOverrides.AnimatorLOD[] animatorLODs) : base(controller)
            {
                _animationsLODs = new AnimationsLOD[animatorLODs.Length + 1];

                //Create default (zero) LOD with no overrides
                {
                    List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();

                    foreach (AnimationClip origClip in controller.animationClips)
                    {
                        overrideClips.Add(new KeyValuePair<AnimationClip, AnimationClip>(origClip, null));
                    }

                    _animationsLODs[0] = new AnimationsLOD(0, overrideClips, null);
                }

                //Create LODs
                for (int i = 0; i < animatorLODs.Length; i++)
                {
                    List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();

                    foreach (AnimationClip origClip in controller.animationClips)
                    {
                        AnimationClip overrideClip = null;

                        for (int j = 0; j < animatorLODs[i]._overrideClips.Length; j++)
                        {
                            if (animatorLODs[i]._overrideClips[j]._originalClip == origClip)
                            {
                                overrideClip = animatorLODs[i]._overrideClips[j]._overrideClip;
                                break;
                            }
                        }

                        overrideClips.Add(new KeyValuePair<AnimationClip, AnimationClip>(origClip, overrideClip));
                    }

                    _animationsLODs[i + 1] = new AnimationsLOD(animatorLODs[i]._minLODLevel, overrideClips, CreateAvatar(controller, animatorLODs[i]._avatarMask));
                }

                _currentLOD = 0;
                _currentAnimationLODSet = 0;
            }

            private static Avatar CreateAvatar(RuntimeAnimatorController controller, AvatarMask avatarMask)
            {
                GameObject avatarGameObject = new GameObject(controller.name + " LOD Avatar");

                if (avatarMask != null)
                {
                    for (int i = 0; i < avatarMask.transformCount; i++)
                    {
                        if (avatarMask.GetTransformActive(i))
                        {
                            string[] pathObjects = avatarMask.GetTransformPath(i).Split('/');
                            AddPath(avatarGameObject.transform, pathObjects, 0);
                        }
                    }
                }

                Avatar avatar = AvatarBuilder.BuildGenericAvatar(avatarGameObject, "");
                Destroy(avatarGameObject);

                return avatar;
            }

            private static void AddPath(Transform root, string[] pathObjects, int pathIndex)
            {
                if (pathIndex < pathObjects.Length)
                {
                    Transform pathObject = null;

                    foreach (Transform child in root)
                    {
                        if (child.name == pathObjects[pathIndex])
                        {
                            pathObject = child;
                            break;
                        }
                    }

                    if (pathObject == null)
                    {
                        GameObject childObject = new GameObject(pathObjects[pathIndex]);
                        childObject.transform.SetParent(root, false);
                        pathObject = childObject.transform;
                    }

                    AddPath(pathObject, pathObjects, pathIndex + 1);
                }
            }

            public bool OnLODLevelChanged(int level)
            {
                //Work out animation set for this LOD level
                int animationSet = GetAnimationSetForLODLevel(level);

                //If different to current set, set it
                if (animationSet != -1 && level != _currentLOD)
                {
                    ApplyOverrides(_animationsLODs[animationSet]._overrideClipsList);
                    _currentLOD = level;
                    _currentAnimationLODSet = animationSet;
                    return true;
                }

                return false;
            }

            public int GetCurrentLOD()
            {
                return _currentLOD;
            }

            public Avatar GetAvatar()
            {
                return _animationsLODs[_currentAnimationLODSet]._avatar;
            }
            #endregion

            #region Private Functions
            private int GetAnimationSetForLODLevel(int level)
            {
                if (level < 0)
                {
                    return _animationsLODs.Length - 1;
                }

                for (int i = _animationsLODs.Length - 1; i >= 0; i--)
                {
                    if (level >= _animationsLODs[i]._minLODLevel || i == 0)
                    {
                        return i;
                    }
                }

                return -1;
            }
            #endregion
        }
    }
}