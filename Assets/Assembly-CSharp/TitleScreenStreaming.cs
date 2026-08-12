using UnityEngine;

[AddComponentMenu("Streaming/Title Screen Streaming", 200)]
public class TitleScreenStreaming : MonoBehaviour
{
	[SerializeField]
	private string _preloadSceneName = "TimberHearth";

	[SerializeField]
	private StreamingMaterialTable[] _preloadStreamingMaterialTables = new StreamingMaterialTable[0];

	private StreamingAssetBundleState _bakedTerrainsBundleState;

	private StreamingAssetBundleState _batchedRenderersBundleState;

	private StreamingAssetBundleState _bakedVISRenderersBundleState;

	private StreamingAssetBundleState _terrainSceneMeshBundleState;

	private StreamingAssetBundleState _structuresSceneMeshBundleState;

	private void Awake()
	{
		if (StreamingManager.isStreamingEnabled)
		{
			StreamingManager.loadingPriority = StreamingManager.LoadingPriority.High;
			string assetBundleName = (_preloadSceneName + "/BakedTerrains").ToLowerInvariant();
			string assetBundleName2 = (_preloadSceneName + "/BatchedRenderers").ToLowerInvariant();
			string assetBundleName3 = (_preloadSceneName + "/BakedVertexInstanceStreamRenderers").ToLowerInvariant();
			string assetBundleName4 = (_preloadSceneName + "/DetailPatches").ToLowerInvariant();
			string assetBundleName5 = (_preloadSceneName + "/Decals").ToLowerInvariant();
			string assetBundleName6 = (_preloadSceneName + "/meshes/terrain").ToLowerInvariant();
			string assetBundleName7 = (_preloadSceneName + "/meshes/structures").ToLowerInvariant();
			string assetBundleName8 = (_preloadSceneName + "/meshes/props").ToLowerInvariant();
			string assetBundleName9 = (_preloadSceneName + "/meshes/characters").ToLowerInvariant();
			string assetBundleName10 = (_preloadSceneName + "/meshes/effects").ToLowerInvariant();
			for (int i = 0; i < _preloadStreamingMaterialTables.Length; i++)
			{
				_preloadStreamingMaterialTables[i].CachePropertyIDs();
				StreamingManager.RegisterStreamingMaterialTable(_preloadStreamingMaterialTables[i]);
			}
			StreamingManager.LoadStreamingAssets(assetBundleName);
			StreamingManager.LoadStreamingAssets(assetBundleName2);
			StreamingManager.LoadStreamingAssets(assetBundleName3);
			StreamingManager.LoadStreamingAssets(assetBundleName4);
			StreamingManager.LoadStreamingAssets(assetBundleName5);
			StreamingManager.LoadStreamingAssets(assetBundleName6);
			StreamingManager.LoadStreamingAssets(assetBundleName7);
			StreamingManager.LoadStreamingAssets(assetBundleName8);
			StreamingManager.LoadStreamingAssets(assetBundleName9);
			StreamingManager.LoadStreamingAssets(assetBundleName10);
			for (int j = 0; j < _preloadStreamingMaterialTables.Length; j++)
			{
				StreamingManager.LoadStreamingAssets(_preloadStreamingMaterialTables[j].assetBundle);
			}
			_bakedTerrainsBundleState = StreamingManager.GetStreamingAssetBundleState(assetBundleName);
			_batchedRenderersBundleState = StreamingManager.GetStreamingAssetBundleState(assetBundleName2);
			_bakedVISRenderersBundleState = StreamingManager.GetStreamingAssetBundleState(assetBundleName3);
			_terrainSceneMeshBundleState = StreamingManager.GetStreamingAssetBundleState(assetBundleName6);
			_structuresSceneMeshBundleState = StreamingManager.GetStreamingAssetBundleState(assetBundleName7);
		}
	}

	public bool AreRequiredAssetsLoaded()
	{
		if (_bakedTerrainsBundleState.isLoaded && _batchedRenderersBundleState.isLoaded && _bakedVISRenderersBundleState.isLoaded && _terrainSceneMeshBundleState.isLoaded)
		{
			return _structuresSceneMeshBundleState.isLoaded;
		}
		return false;
	}
}
