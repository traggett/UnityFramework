using UnityEngine;
using UnityEngine.Playables;
using System;

namespace Framework
{
	using StateMachineSystem;
	using TimelineSystem;
	
	using Utils;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory("Playables")]
		public class EventRunPlayableDirector : Event, ITimelineStateEvent
		{
			#region Public Data
			public ComponentRef<PlayableDirector> _director;
			public AssetRef<PlayableAsset> _playableAsset;
			public StateRef _goToState;
			#endregion

			#region Private Data
			private PlayableDirector _playableDirector;
			#endregion

			#region Event
			public override float GetDuration()
			{
				return (float)GetPlayableAsset().duration;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(0.859f, 0.439f, 0.576f);
			}

			public override string GetEditorDescription()
			{
				return "Play (<b>"+ GetPlayableAsset().name + "</b>) on " + _director + " then go to " + _goToState;
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				_playableDirector = _director.GetComponent();

				if (_playableDirector != null)
				{
					PlayableAsset playable = GetPlayableAsset();
					_playableDirector.Play(playable, DirectorWrapMode.None);
				}

				return eEventTriggerReturn.EventOngoing;
			}

			public eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime)
			{
				if (_playableDirector.playableGraph.IsValid() && !_playableDirector.playableGraph.IsDone())
				{
					return eEventTriggerReturn.EventOngoing;
				}

				stateMachine.GoToState(StateMachine.Run(stateMachine, _goToState));

				return eEventTriggerReturn.EventFinishedExitState;
			}

			public void End(StateMachineComponent stateMachine) { }

#if UNITY_EDITOR
			public StateMachineEditorLink[] GetEditorLinks()
			{
				StateMachineEditorLink[] links = new StateMachineEditorLink[1];
				links[0] = new StateMachineEditorLink(this, "goToState", "Go To");
				return links;
			}
#endif
			#endregion

			private PlayableAsset GetPlayableAsset()
			{
				if (_playableAsset.IsValid())
				{
#if UNITY_EDITOR
					return _playableAsset._editorAsset;
#else
					return _playableAsset.LoadAsset();
#endif
				}
				else
				{
					PlayableDirector playableDirector = _director.GetComponent();
					return playableDirector.playableAsset;
				}
			}
		}
	}
}
