using UnityEngine;
using System;

namespace Framework
{
	using Utils;

	namespace TimelineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventSetParticleSystemEmit : Event
		{
			#region Public Data
			public ComponentRef<ParticleSystem> _particleSystem;
			public bool _emission = false;
			#endregion

			#region Event
			public override void Trigger()
			{
				ParticleSystem particleSystem = _particleSystem;

				if (particleSystem != null)
				{
					ParticleSystem.EmissionModule emission = particleSystem.emission;
					emission.enabled = _emission;
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return Color.blue;
			}

			public override string GetEditorDescription()
			{
				return "Set ParticleSystem Emit: " + _emission;
			}
#endif
			#endregion
		}
	}
}
