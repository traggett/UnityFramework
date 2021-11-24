using UnityEngine;

namespace Framework
{
    namespace AnimationSystem
    {
        [RequireComponent(typeof(Animator), typeof(LODGroup))]
        public class AnimatorLODGroup : MonoBehaviour
        {
            #region Public Data
            [HideInInspector]
            public float _animatedValue;
            public Camera _camera;
            #endregion

            #region Private Data
            private Animator _animator;
            private LODGroup _LODGroup;
            private AnimatorLODOverrideController _overrideController;
            private LOD[] _LODs;
            private bool[] _LODHasSkinnedMeshes;
            private bool _forceLODRenderLevel;
            private Avatar _origAvatar;
            private int _currentLODLevel;
            #endregion

            #region Unity Messages
            private void Awake()
            {
                _animator = GetComponent<Animator>();
                _origAvatar = _animator.avatar;
                _LODGroup = GetComponent<LODGroup>();
            }

            private void Start()
            {
                //Create override controller
                _overrideController = AnimatorLODControllerOverrides.GetOverrideController(_animator);

                if (_overrideController == null)
                {
                    this.enabled = false;
                    return;
                }

                _animator.runtimeAnimatorController = _overrideController;

                //Cache LOD level types
                _LODs = _LODGroup.GetLODs();
                _LODHasSkinnedMeshes = new bool[_LODs.Length];

                for (int i = 0; i < _LODs.Length; i++)
                {
                    SetupLOD(ref _LODs[i], out _LODHasSkinnedMeshes[i]);
                }

                _currentLODLevel = -1;
            }

            private void Update()
            {
                ReEnableForcedDisabledLOD();

                //If no LOD was rendered or culled (last LOD level), set controller LOD to lowest level
                if (_currentLODLevel == -1)
                {
                    if (_overrideController.OnLODLevelChanged(-1))
                    {
                        _LODGroup.RecalculateBounds();
                    }
                }
                else
                {
				    //Swith to avatar based on LOD
				    SwitchToAvatar(_overrideController.GetAvatar() ?? _origAvatar);

                    //Reset LOD level
                    _currentLODLevel = -1;
                }
            }
            #endregion

            #region Public Interface
            public void OnLODRendered(AnimatorLODRenderer renderer, Camera camera)
            {
                if (camera == GetCamera())
                {
                    int prevLODLevel = _currentLODLevel;

                    //Work out LOD level this renderer belongs to, store it
                    _currentLODLevel = GetLODLevel(renderer);

                    //Set LOD level on animator if different to last frame
                    if (_currentLODLevel != _overrideController.GetCurrentLOD())
                    {
                        //Ok so if animation LOD level has decreased this frame, we need to force previous LOD renderes to show for a frame to hide being in wrong pos.
                        if (_overrideController.OnLODLevelChanged(_currentLODLevel))
                        {
                            //If lod level has decreased, force previous frame renders to show for this frame to cover bones not being updated
                            if (_currentLODLevel < prevLODLevel)
                            {
                                ForceEnablePreviousLODRenderers(_currentLODLevel);
                            }

                            _LODGroup.RecalculateBounds();
                        }
                    }
                }
            }
            #endregion

            #region Private Functions
            private void SetupLOD(ref LOD lod, out bool hasSkinnedMeshes)
            {
                //Check has no renderers
                if (lod.renderers == null || lod.renderers.Length == 0)
                {
                    hasSkinnedMeshes = false;
                    return;
                }
                else
                {
                    //Check if uses static renderers
                    hasSkinnedMeshes = false;

                    for (int i = 0; i < lod.renderers.Length; i++)
                    {
                        if (!hasSkinnedMeshes && lod.renderers[i] is SkinnedMeshRenderer)
                        {
                            hasSkinnedMeshes = true;
                        }

                        AnimatorLODRenderer LODRenderer = lod.renderers[i].gameObject.GetComponent<AnimatorLODRenderer>();

                        if (LODRenderer == null)
                            LODRenderer = lod.renderers[i].gameObject.AddComponent<AnimatorLODRenderer>();

                        LODRenderer._renderer = lod.renderers[i];
                        LODRenderer._animatorLOD = this;
                    }
                }
            }

            private Camera GetCamera()
            {
                if (_camera == null)
                    return Camera.main;

                return _camera;
            }

            private int GetLODLevel(AnimatorLODRenderer renderer)
            {
                for (int i = 0; i < _LODs.Length; i++)
                {
                    for (int j = 0; j < _LODs[i].renderers.Length; j++)
                    {
                        if (_LODs[i].renderers[j] == renderer._renderer)
                        {
                            return i;
                        }
                    }
                }

                return -1;
            }

            private void ForceEnablePreviousLODRenderers(int level)
            {
                for (int i = 0; i < _LODs[level].renderers.Length; i++)
                {
                    _LODs[level].renderers[i].enabled = false;
                }

                for (int i = 0; i < _LODs[level + 1].renderers.Length; i++)
                {
                    _LODs[level + 1].renderers[i].enabled = true;
                }

                _forceLODRenderLevel = true;
            }


            private void ReEnableForcedDisabledLOD()
            {
                if (_forceLODRenderLevel)
                {
                    for (int i = 0; i < _LODs.Length; i++)
                    {
                        for (int j = 0; j < _LODs[i].renderers.Length; j++)
                        {
                            _LODs[i].renderers[j].enabled = true;
                        }
                    }
                    _forceLODRenderLevel = false;
                }
            }

            private void SetSkinnedMeshLODsEnabled(bool enabled)
            {
                for (int i = 0; i < _LODs.Length; i++)
                {
                    for (int j = 0; j < _LODs[i].renderers.Length; j++)
                    {
                        if (_LODs[i].renderers[j] is SkinnedMeshRenderer)
                        {
                            _LODs[i].renderers[j].gameObject.SetActive(enabled);
                        }
                    }
                }
            }

            private void SwitchToAvatar(Avatar avatar)
            {
                if (_animator.avatar != avatar)
                {
                    SetSkinnedMeshLODsEnabled(false);

                    AnimatorStateInfo[] state = new AnimatorStateInfo[_animator.layerCount];

                    for (int i = 0; i < state.Length; i++)
                    {
                        state[i] = _animator.GetCurrentAnimatorStateInfo(i);
                        if (_animator.IsInTransition(i))
                        {
                            AnimatorTransitionInfo trans = _animator.GetAnimatorTransitionInfo(i);
                            state[i] = _animator.GetNextAnimatorStateInfo(i);
                        }

                    }

                    object[] parms = new object[_animator.parameterCount];
                    for (int i = 0; i < parms.Length; i++)
                    {
                        switch (_animator.parameters[i].type)
                        {
                            case AnimatorControllerParameterType.Bool:
                                parms[i] = _animator.GetBool(_animator.parameters[i].name);
                                break;
                            case AnimatorControllerParameterType.Float:
                                parms[i] = _animator.GetFloat(_animator.parameters[i].name);
                                break;
                            case AnimatorControllerParameterType.Int:
                                parms[i] = _animator.GetInteger(_animator.parameters[i].name);
                                break;
                            case AnimatorControllerParameterType.Trigger:
                                parms[i] = _animator.GetBool(_animator.parameters[i].name);
                                break;
                        }
                    }

                    _animator.logWarnings = false;
                    _animator.avatar = avatar;

                    for (int i = 0; i < state.Length; i++)
                    {
                        _animator.Play(state[i].fullPathHash, i, state[i].normalizedTime);
                    }

                    for (int i = 0; i < parms.Length; i++)
                    {
                        switch (_animator.parameters[i].type)
                        {
                            case AnimatorControllerParameterType.Bool:
                                _animator.SetBool(_animator.parameters[i].name, (bool)parms[i]);
                                break;
                            case AnimatorControllerParameterType.Float:
                                _animator.SetFloat(_animator.parameters[i].name, (float)parms[i]);
                                break;
                            case AnimatorControllerParameterType.Int:
                                _animator.SetInteger(_animator.parameters[i].name, (int)parms[i]);
                                break;
                            case AnimatorControllerParameterType.Trigger:
                                if ((bool)parms[i])
                                {
                                    _animator.SetTrigger(_animator.parameters[i].name);
                                }
                                break;
                        }
                    }

                    SetSkinnedMeshLODsEnabled(true);
                }
            }
            #endregion
        }
    }
}