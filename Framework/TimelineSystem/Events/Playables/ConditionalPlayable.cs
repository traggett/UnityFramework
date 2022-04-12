using System;

namespace Framework
{
	using Maths;
	using StateMachineSystem;
	using UnityEngine.Playables;
	using Utils;

	namespace TimelineSystem
	{
		[Serializable]
		[ConditionCategory("Playables")]
		public class ConditionalPlayable : Condition
		{
			#region Public Data
			public ComponentRef<PlayableDirector> _director;
			public AssetRef<PlayableAsset> _playableAsset;
			public DirectorWrapMode _wrapMode;
			#endregion

			private PlayableDirector _playableDirector;

			#region Conditional
#if UNITY_EDITOR
			public override string GetDescription()
			{
				return "Play Timeline: <b>" + _playableAsset + "</b>";
			}

			public override string GetTakenText()
			{
				return "Timeline Finished";
			}
#endif

			public override void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
				_playableDirector = _director.GetComponent();

				if (_playableDirector != null)
				{
					if (_playableAsset.IsValid())
					{
						PlayableAsset playable = _playableAsset.LoadAsset();
						_playableDirector.Play(playable, _wrapMode);
					}
					else
					{
						_playableDirector.Play(_playableDirector.playableAsset, _wrapMode);
					}
				}
			}

			public override bool IsConditionMet(StateMachineComponent stateMachine)
			{
				return _playableDirector != null && _playableDirector.time >= _playableDirector.duration && _wrapMode != DirectorWrapMode.Loop;
			}

			public override void OnEndConditionChecking(StateMachineComponent stateMachine)
			{
				
			}
			#endregion
		}
	}	
}