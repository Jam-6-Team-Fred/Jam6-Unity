using UnityEngine;

public interface IStreamingTexturesSubscriber
{
	void OnTexturesLoaded(StreamingTextureAssetBundle streamingTextureAssetBundle);

	void OnTexturesUnloaded();

	void OnBeginSubscription(StreamingIteratedTextureAssetBundle streamingTextureAssetBundle);

	void OnAssetBundleBeginLoad(StreamingIteratedTextureAssetBundle streamingTextureAssetBundle);

	void OnTextureLoaded(int index, Texture texture);

	void OnTextureUnloaded(int index);
}
