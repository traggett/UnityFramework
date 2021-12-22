using System;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Framework
{
	namespace Playables
	{
		[RequireComponent(typeof(PlayableDirector))]
		public class PlayableDirectorBindings : MonoBehaviour
		{
			[Serializable]
			public struct GenericBinding
			{
				public Object _key;
				public Object _value;
			}

			[Serializable]
			public struct ExposedReference
			{
				public string _key;
				public Object _value;
			}

			[Serializable]
			public class PlayableAssetData
			{
				public PlayableAsset _asset;
				public GenericBinding[] _sceneBindings;
				public ExposedReference[] _exposedReferences;
			}

			public PlayableAssetData[] _playableAssetData;


			private PlayableDirector _playableDirector;

			public void PrepareBindings(PlayableAsset asset)
			{
				if (_playableDirector == null)
					_playableDirector = GetComponent<PlayableDirector>();

				PlayableAssetData assetData = GetAssetData(asset);

				if (assetData != null)
				{
					for (int i=0; i < assetData._sceneBindings.Length; i++)
					{
						_playableDirector.SetGenericBinding(assetData._sceneBindings[i]._key, assetData._sceneBindings[i]._value);
					}

					for (int i = 0; i < assetData._exposedReferences.Length; i++)
					{
						_playableDirector.SetReferenceValue(assetData._exposedReferences[i]._key, assetData._exposedReferences[i]._value);
					}
				}
			}

			private PlayableAssetData GetAssetData(PlayableAsset asset)
			{
				if (asset != null && _playableAssetData != null)
				{
					for (int i = 0; i < _playableAssetData.Length; i++)
					{
						if (_playableAssetData[i]._asset == asset)
						{
							return _playableAssetData[i];
						}
					}
				}
				
				return null;
			}
		}
	}
}