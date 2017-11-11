using UnityEditor;

namespace Framework
{
	namespace AnimationSystem
	{
		[CustomEditor(typeof(AnimatedCameraSnapshot), true)]
		public class AnimatedCameraSnapshotInspector : UnityEditor.Editor
		{
			#region Editor Calls
			public override void OnInspectorGUI()
			{
				serializedObject.Update();
				AnimatedCameraSnapshot snapShot = this.target as AnimatedCameraSnapshot;
				AnimatedCameraState inner = snapShot._state;
				UnityEditor.Editor innerEditor = AnimatedCameraStateInspector.CreateEditor(inner);
				innerEditor.DrawDefaultInspector();
				serializedObject.ApplyModifiedProperties();
			}
			#endregion
		}
	}
}