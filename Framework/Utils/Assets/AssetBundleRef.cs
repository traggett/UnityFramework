#if UNITY_ANDROID && !UNITY_EDITOR
#define _ANDROID_BUNDLES
#endif

using UnityEngine;
using System.Collections;
using System;
using Object = UnityEngine.Object;
#if _ANDROID_BUNDLES
using Google.Play.AssetDelivery;
#else
using System.IO;
#endif

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public class AssetBundleRef
		{
			#region Serialised Data
			[SerializeField] private string _fileName;
			#endregion

			#region Private Data
			private AssetBundle _assetBundle;
			#endregion

			#region Public Interface
			public AssetBundleRef(string filename)
			{
				_fileName = filename;
				_assetBundle = null;
			}

			public IEnumerator Load(bool loadAllAssets = false, Action<float> onProgress = null)
			{
				onProgress?.Invoke(0f);

				if (_assetBundle == null)
				{
					float loadProgess = loadAllAssets ? 0.5f : 1f;

#if _ANDROID_BUNDLES

					// Loads the AssetBundle from disk, downloading the asset pack containing it if necessary.
					PlayAssetBundleRequest bundleRequest = PlayAssetDelivery.RetrieveAssetBundleAsync(_fileName);

					if (bundleRequest == null)
					{
						throw new Exception("Can't load Asset Bundle " + _fileName);
					}
			
					while (!bundleRequest.IsDone)
					{
						onProgress?.Invoke(bundleRequest.DownloadProgress * loadProgess);
						yield return new WaitForEndOfFrame();
					}

					_assetBundle = bundleRequest.AssetBundle;
#else

					string path = Path.Combine(Application.streamingAssetsPath, _fileName);
					AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(path);

					if (bundleRequest == null)
					{
						throw new Exception("Can't load Asset Bundle at " + _fileName);
					}

					while (!bundleRequest.isDone)
					{
						onProgress?.Invoke(bundleRequest.progress * loadProgess);
						yield return new WaitForEndOfFrame();
					}

					_assetBundle = bundleRequest.assetBundle;
#endif

					onProgress?.Invoke(loadProgess);

					if (loadAllAssets)
					{
						AssetBundleRequest loadRequest = _assetBundle.LoadAllAssetsAsync();

						if (loadRequest == null)
							yield break;

						while (!loadRequest.isDone)
						{
							onProgress?.Invoke(loadProgess + (loadRequest.progress * loadProgess));
							yield return new WaitForEndOfFrame();
						}

						onProgress?.Invoke(1f);
					}
				}

				yield break;
			}

			public IEnumerator Unload(bool unloadAllLoadedObjects = true, Action<float> onProgress = null)
			{
				AsyncOperation bundleRequest = _assetBundle.UnloadAsync(unloadAllLoadedObjects);
				_assetBundle = null;

				while (!bundleRequest.isDone)
				{
					onProgress?.Invoke(bundleRequest.progress);
					yield return new WaitForEndOfFrame();
				}

				yield break;
			}

			public T GetAsset<T>(string assetPath) where T : Object
			{
				if (_assetBundle != null)
					return _assetBundle.LoadAsset<T>(assetPath);

				return null;
			}
			#endregion
		}
	}
}