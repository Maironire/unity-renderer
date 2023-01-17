using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL.Providers
{
    public abstract class AssetBundleWebRequestBasedProvider
    {
        protected static async UniTask<AssetBundle> FromWebRequestAsync(IWebRequestAsyncOperation webRequest, string url, CancellationToken cancellationToken)
        {
            await webRequest.WithCancellation(cancellationToken);

            if (webRequest.isDisposed)
            {
                Debug.Log($"Operation is disposed. Url: {url}");
                throw new AssetBundleException($"Operation is disposed. Url: {url}");
            }

            if (!webRequest.isSucceeded)
            {
                Debug.Log($"Request failed {webRequest.webRequest.error}. Url: {url}");
                throw new AssetBundleException($"Request failed {webRequest.webRequest.error}. Url: {url}");
            }

            return DownloadHandlerAssetBundle.GetContent(webRequest.webRequest);
        }
    }
}
