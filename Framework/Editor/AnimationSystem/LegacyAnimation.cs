using UnityEngine;
using UnityEditor;

namespace Framework
{
    namespace AnimationSystem
    {
        namespace Editor
        {
            public class LegacyAnimation
            {
                [MenuItem("Assets/Mark as Legacy")]
                static void MarkAsLegacy()
                {
                    MarkAsLegacy(Selection.activeObject as AnimationClip);
                }

                [MenuItem("Assets/Mark as Legacy", true)]
                static bool ValidateMarkAsLegacy()
                {
                    AnimationClip asset = Selection.activeObject as AnimationClip;

                    if (asset == null)
                    {
                        return false;
                    }

                    return true;
                }

                static bool MarkAsLegacy(AnimationClip asset)
                {
                    asset.legacy = true;
                    return true;
                }
            }
        }
    }
}