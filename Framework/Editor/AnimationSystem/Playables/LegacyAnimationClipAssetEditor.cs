using UnityEngine.Timeline;
using UnityEditor.Timeline;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Editor
		{
            [CustomTimelineEditor(typeof(LegacyAnimationClipAsset))]
            public class LegacyAnimationClipAssetEditor : ClipEditor
            {
                public static readonly string k_NoClipAssignedError = "No animation clip assigned";
                public static readonly string k_LegacyClipError = "Legacy animation clips are not supported";
        
                public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
                {
                    LegacyAnimationClipAsset asset = clip.asset as LegacyAnimationClipAsset;

                    if (asset != null && asset._animationClip != null && !asset._animationClip.legacy)
                    {
                        asset._animationClip = null;
                        UnityEngine.Debug.LogError("Only Legacy Animation Clips are supported");
                    }
                }

                public override ClipDrawOptions GetClipOptions(TimelineClip clip)
                {
                    ClipDrawOptions clipOptions = base.GetClipOptions(clip);
                    LegacyAnimationClipAsset asset = clip.asset as LegacyAnimationClipAsset;

                    if (asset != null)
                        clipOptions.errorText = GetErrorText(asset, clipOptions.errorText);

                    return clipOptions;
                }

                string GetErrorText(LegacyAnimationClipAsset animationAsset, string defaultError)
                {
                    if (animationAsset._animationClip == null)
                        return k_NoClipAssignedError;
                    
                    if (!animationAsset._animationClip.legacy)
                        return k_LegacyClipError;

                    return defaultError;
                }
            }
        }
    }
}