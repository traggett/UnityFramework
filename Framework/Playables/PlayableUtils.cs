using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		public static class PlayableUtils
		{
			public static bool IsPlayableOfType(Playable playable, Type type)
			{
				Type playableType = playable.GetPlayableType();
				return SystemUtils.IsTypeOf(type, playableType);
			}


			public static bool IsScriptPlayable<T>(Playable playable, out T playableBehaviour) where T : class, IPlayableBehaviour, new()
			{
				if (IsPlayableOfType(playable, typeof(T)))
				{
					ScriptPlayable<T> scriptPlayable = (ScriptPlayable<T>)playable;
					playableBehaviour = scriptPlayable.GetBehaviour();
					return true;
				}

				playableBehaviour = null;
				return false;
			}

			public static bool FindPlayableOfType<T>(Playable root, out Playable playable)
			{
				if (IsPlayableOfType(root, typeof(T)))
				{
					playable = root;
					return true;
				}

				for (int i = 0; i < root.GetInputCount(); i++)
				{
					Playable nextRoot = root.GetInput(i);

					if (FindPlayableOfType<T>(nextRoot, out playable))
					{
						return true;
					}
				}

				playable = default;
				return false;
			}

			//Get all playable behaviours in a playable graph of type T, or implementing interface of type T
			public static List<T> GetPlayableBehaviours<T>(PlayableGraph graph) where T : class
			{
				List<T> playables = new List<T>();

				int rootCount = graph.GetRootPlayableCount();

				for (int i = 0; i < rootCount; i++)
				{
					Playable root = graph.GetRootPlayable(i);

					int inputCount = root.GetInputCount(); ;

					for (int j = 0; j < inputCount; j++)
					{
						GetPlayableBehaviours(root.GetInput(j), ref playables);
					}
				}

				return playables;
			}

			public static bool IsOutputtingToPlayableType(Playable playable, Type type)
			{
				int numOutputs = playable.GetOutputCount();

				for (int i=0; i<numOutputs; i++)
				{
					if (IsPlayableOfType(playable.GetOutput(i), type))
					{
						return true;
					}
				}

				return false;
			}

			public static PlayableBehaviour GetPlayableBehaviour(Playable playable, Type playableType)
			{
				Type scriptPlayableType = typeof(ScriptPlayable<>).MakeGenericType(new[] { playableType });

				MethodInfo castMethod = scriptPlayableType.GetMethod("op_Explicit", new Type[] { typeof(Playable) });

				if (castMethod != null)
				{
					object scriptPlayable = castMethod.Invoke(null, new object[] { playable });

					MethodInfo method = scriptPlayableType.GetMethod("GetBehaviour");

					if (method != null)
					{
						return method.Invoke(scriptPlayable, new object[0]) as PlayableBehaviour;
					}
				}

				return null;
			}

			private static void GetPlayableBehaviours<T>(Playable root, ref List<T> playables) where T : class
			{
				int inputCount = root.GetInputCount();

				for (int i = 0; i < inputCount; i++)
				{
					Playable node = root.GetInput(i);

					if (node.IsValid())
					{
						if (IsPlayableOfType(node, typeof(T)))
						{
							T playable = GetPlayableBehaviour(node, typeof(T)) as T;

							if (playable != null)
							{
								playables.Add(playable);
							}
						}

						GetPlayableBehaviours(node, ref playables);
					}
				}
			}
		}
	}
}