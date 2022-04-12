using System;
using System.Collections;
using UnityEngine.Playables;

namespace Framework
{
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		public class PlayableGraphState : State
		{
			public ComponentRef<PlayableDirector> _director;
			public AssetRef<PlayableAsset> _playableAsset;
			public StateRef _goToState;

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

#if UNITY_EDITOR
			public override StateMachineEditorLink[] GetEditorLinks()
			{
				StateMachineEditorLink[] links = new StateMachineEditorLink[1];
				links[0] = new StateMachineEditorLink(this, "goToState", "Playable Graph Finished");
				return links;
			}

			public override string GetAutoDescription()
			{
				string label = "Run " + _playableAsset + " on " + _director;
				return label;
			}

			public override string GetStateIdLabel()
			{
				return "PlayableGraph (State" + _stateId.ToString("00") + ")";
			}
#endif
		}
	}
}