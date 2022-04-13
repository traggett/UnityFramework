using System;
using System.Collections;
using UnityEngine.Playables;

namespace Framework
{
	using UnityEngine;
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		public class PlayableGraphState : State
		{
			#region Public Data	
			public ComponentRef<PlayableDirector> _director;
			public AssetRef<PlayableAsset> _playableAsset;

			[StateLink("And then")]
			public StateRef _goToState;
			#endregion

			#region State
#if UNITY_EDITOR
			public override string GetEditorLabel()
			{
				return "PlayableGraph (State" + _stateId.ToString("00") + ")";
			}

			public override string GetEditorDescription()
			{
				return "Run " + _playableAsset + " on " + _director;
			}

			public override Color GetEditorColor()
			{
				return new Color(102 / 255f, 129 / 255f, 116 / 255f);
			}
#endif

			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				PlayableDirector director = _director.GetComponent();
				PlayableAsset playable = _playableAsset.LoadAsset();

				if (director != null && playable != null)
				{
					director.Play(playable, DirectorWrapMode.None);

					while (director.playableGraph.IsValid() && !director.playableGraph.IsDone())
					{
						yield return null;
					}
				}

				stateMachine.GoToState(StateMachine.Run(stateMachine, _goToState));
			}
			#endregion
		}
	}
}