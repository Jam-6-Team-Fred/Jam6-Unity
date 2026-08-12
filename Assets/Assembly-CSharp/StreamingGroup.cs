using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Streaming/Streaming Group", 100)]
public class StreamingGroup : MonoBehaviour
{
	private static List<StreamingGroup> s_streamingGroups = new List<StreamingGroup>(16);

	[SerializeField]
	private string _sceneName = "";

	[SerializeField]
	public StreamingMaterialTable[] _streamingMaterialTables = new StreamingMaterialTable[0]; // CHANGED

	[SerializeField]
	private bool _reloadAssetsOnDeath;

	private string _bakedTerrainsBundle;

	private string _batchedRenderersBundle;

	private string _batchedRenderersColliderBundle;

	private string _bakedVISRenderersBundle;

	private string _detailPatchesBundle;

	private string _decalsBundle;

	private string _terrainSceneMeshBundle;

	private string _terrainColliderSceneMeshBundle;

	private string _structuresSceneMeshBundle;

	private string _structuresColliderSceneMeshBundle;

	private string _propsSceneMeshBundle;

	private string _propsColliderSceneMeshBundle;

	private string _charactersSceneMeshBundle;

	private string _charactersColliderSceneMeshBundle;

	private string _effectsSceneMeshBundle;

	private string _effectsColliderSceneMeshBundle;

	private StreamingAssetBundleState _bakedTerrainsBundleState;

	private StreamingAssetBundleState _batchedRenderersBundleState;

	private StreamingAssetBundleState _batchedRenderersColliderBundleState;

	private StreamingAssetBundleState _bakedVISRenderersBundleState;

	private StreamingAssetBundleState _terrainSceneMeshBundleState;

	private StreamingAssetBundleState _terrainColliderSceneMeshBundleState;

	private StreamingAssetBundleState _structuresSceneMeshBundleState;

	private StreamingAssetBundleState _structuresColliderSceneMeshBundleState;

	private const int kTerrainMeshLoadPriority = 8;

	private const int kStructureMeshLoadPriority = 7;

	private const int kCharacterMeshLoadPriority = 6;

	private const int kPropMeshLoadPriority = 5;

	private const int kDetailMeshLoadPriority = 4;

	private const int kTerrainTextureLoadPriority = 3;

	private const int kGeneralTextureLoadPriority = 2;

	private const int kDetailTextureLoadPriority = 1;

	private const int kCharacterTextureLoadPriority = 0;

	private const int kColliderLoadPriorityOffset = 10;

	private const int kTimberHearthPriorityOffset = 100;

	private int _numRequiredAssetsRequests;

	private int _numGeneralAssetsRequests;

	private int _requiredAssetsPriorityBias;

	private int _generalAssetsPriorityBias;

	private bool _locked;

	private bool _streamCollidersOnWakeUp;

	private void Reset()
	{
		_sceneName = base.gameObject.scene.name;
	}

	private void Awake()
	{
		_bakedTerrainsBundle = (_sceneName + "/BakedTerrains").ToLowerInvariant();
		_batchedRenderersBundle = (_sceneName + "/BatchedRenderers").ToLowerInvariant();
		_batchedRenderersColliderBundle = (_sceneName + "/BatchedColliders").ToLowerInvariant();
		_bakedVISRenderersBundle = (_sceneName + "/BakedVertexInstanceStreamRenderers").ToLowerInvariant();
		_detailPatchesBundle = (_sceneName + "/DetailPatches").ToLowerInvariant();
		_decalsBundle = (_sceneName + "/Decals").ToLowerInvariant();
		_terrainSceneMeshBundle = (_sceneName + "/meshes/terrain").ToLowerInvariant();
		_terrainColliderSceneMeshBundle = (_sceneName + "/meshes/terrain_colliders").ToLowerInvariant();
		_structuresSceneMeshBundle = (_sceneName + "/meshes/structures").ToLowerInvariant();
		_structuresColliderSceneMeshBundle = (_sceneName + "/meshes/structures_colliders").ToLowerInvariant();
		_propsSceneMeshBundle = (_sceneName + "/meshes/props").ToLowerInvariant();
		_propsColliderSceneMeshBundle = (_sceneName + "/meshes/props_colliders").ToLowerInvariant();
		_charactersSceneMeshBundle = (_sceneName + "/meshes/characters").ToLowerInvariant();
		_charactersColliderSceneMeshBundle = (_sceneName + "/meshes/characters_colliders").ToLowerInvariant();
		_effectsSceneMeshBundle = (_sceneName + "/meshes/effects").ToLowerInvariant();
		_effectsColliderSceneMeshBundle = (_sceneName + "/meshes/effects_colliders").ToLowerInvariant();
		_bakedTerrainsBundleState = StreamingManager.GetStreamingAssetBundleState(_bakedTerrainsBundle);
		_batchedRenderersBundleState = StreamingManager.GetStreamingAssetBundleState(_batchedRenderersBundle);
		_batchedRenderersColliderBundleState = StreamingManager.GetStreamingAssetBundleState(_batchedRenderersColliderBundle);
		_bakedVISRenderersBundleState = StreamingManager.GetStreamingAssetBundleState(_bakedVISRenderersBundle);
		_terrainSceneMeshBundleState = StreamingManager.GetStreamingAssetBundleState(_terrainSceneMeshBundle);
		_terrainColliderSceneMeshBundleState = StreamingManager.GetStreamingAssetBundleState(_terrainColliderSceneMeshBundle);
		_structuresSceneMeshBundleState = StreamingManager.GetStreamingAssetBundleState(_structuresSceneMeshBundle);
		_structuresColliderSceneMeshBundleState = StreamingManager.GetStreamingAssetBundleState(_structuresColliderSceneMeshBundle);
		_numRequiredAssetsRequests = 0;
		_numGeneralAssetsRequests = 0;
		_requiredAssetsPriorityBias = 0;
		_generalAssetsPriorityBias = 0;
		_locked = false;
		for (int i = 0; i < _streamingMaterialTables.Length; i++)
		{
			_streamingMaterialTables[i].CachePropertyIDs();
			StreamingManager.RegisterStreamingMaterialTable(_streamingMaterialTables[i]);
		}
		GlobalMessenger.AddListener("FlashbackStart", OnPlayerDeath);
		GlobalMessenger.AddListener("TriggerDeathOutsideTimeLoop", OnPlayerDeath);
		GlobalMessenger.AddListener("TriggerDeathOfReality", OnPlayerDeath);
		GlobalMessenger.AddListener("TriggerDeathByVoid", OnPlayerDeath);
		GlobalMessenger.AddListener("TriggerDeathByRingworldEscape", OnPlayerDeath);
		GlobalMessenger.AddListener("TriggerDeathByDreamworldEscape", OnPlayerDeath);
		GlobalMessenger.AddListener("TriggerDeathByQuantumMoon", OnPlayerDeath);
		GlobalMessenger.AddListener("FinishOpenEyes", OnFinishOpenEyes);
		GlobalMessenger.AddListener("DeathSequenceComplete", OnDeathSequenceComplete);
		GlobalMessenger.AddListener("MemoryUplinkComplete", OnMemoryUplinkComplete);
		LoadManager.OnStartSceneLoad += OnStartSceneLoad;
		LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
		GlobalMessenger.AddListener("TriggerMemoryUplinkAssetDump", OnMemoryUplinkAssetDump);
		_streamCollidersOnWakeUp = !_sceneName.Equals("TimberHearth");
		s_streamingGroups.Add(this);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("FlashbackStart", OnPlayerDeath);
		GlobalMessenger.RemoveListener("TriggerDeathOutsideTimeLoop", OnPlayerDeath);
		GlobalMessenger.RemoveListener("TriggerDeathOfReality", OnPlayerDeath);
		GlobalMessenger.RemoveListener("TriggerDeathByVoid", OnPlayerDeath);
		GlobalMessenger.RemoveListener("TriggerDeathByRingworldEscape", OnPlayerDeath);
		GlobalMessenger.RemoveListener("TriggerDeathByDreamworldEscape", OnPlayerDeath);
		GlobalMessenger.RemoveListener("TriggerDeathByQuantumMoon", OnPlayerDeath);
		GlobalMessenger.RemoveListener("FinishOpenEyes", OnFinishOpenEyes);
		GlobalMessenger.RemoveListener("DeathSequenceComplete", OnDeathSequenceComplete);
		GlobalMessenger.RemoveListener("MemoryUplinkComplete", OnMemoryUplinkComplete);
		LoadManager.OnStartSceneLoad -= OnStartSceneLoad;
		LoadManager.OnCompleteSceneLoad -= OnCompleteSceneLoad;
		GlobalMessenger.RemoveListener("TriggerMemoryUplinkAssetDump", OnMemoryUplinkAssetDump);
		s_streamingGroups.Remove(this);
	}

	private void OnDeathSequenceComplete()
	{
		if (!_reloadAssetsOnDeath)
		{
			if (_streamCollidersOnWakeUp)
			{
				UnloadRequiredColliders();
			}
			UnloadRequiredAssets();
			UnloadGeneralAssets();
		}
		if (!_streamCollidersOnWakeUp)
		{
			StreamingManager.SetUseMeshBundleThreadedBake(_batchedRenderersColliderBundle, useThreadedBake: false);
			StreamingManager.SetUseMeshBundleThreadedBake(_terrainColliderSceneMeshBundle, useThreadedBake: false);
			StreamingManager.SetUseMeshBundleThreadedBake(_structuresColliderSceneMeshBundle, useThreadedBake: false);
			StreamingManager.SetUseMeshBundleThreadedBake(_propsColliderSceneMeshBundle, useThreadedBake: false);
			StreamingManager.SetUseMeshBundleThreadedBake(_charactersColliderSceneMeshBundle, useThreadedBake: false);
		}
	}

	private void OnPlayerDeath()
	{
		if (_reloadAssetsOnDeath)
		{
			StreamingManager.loadingPriority = StreamingManager.LoadingPriority.High;
			LoadRequiredAssets();
			LoadGeneralAssets();
		}
		if (!_reloadAssetsOnDeath)
		{
			UnloadRequiredAssets();
			UnloadGeneralAssets();
		}
		_locked = true;
	}

	private void OnFinishOpenEyes()
	{
		StreamingManager.loadingPriority = StreamingManager.LoadingPriority.Low;
		if (_streamCollidersOnWakeUp)
		{
			LoadRequiredColliders();
		}
		if (!_streamCollidersOnWakeUp)
		{
			StreamingManager.SetUseMeshBundleThreadedBake(_batchedRenderersColliderBundle, useThreadedBake: true);
			StreamingManager.SetUseMeshBundleThreadedBake(_terrainColliderSceneMeshBundle, useThreadedBake: true);
			StreamingManager.SetUseMeshBundleThreadedBake(_structuresColliderSceneMeshBundle, useThreadedBake: true);
			StreamingManager.SetUseMeshBundleThreadedBake(_propsColliderSceneMeshBundle, useThreadedBake: true);
			StreamingManager.SetUseMeshBundleThreadedBake(_charactersColliderSceneMeshBundle, useThreadedBake: true);
		}
	}

	private void OnMemoryUplinkComplete()
	{
		if (_streamCollidersOnWakeUp)
		{
			LoadRequiredColliders();
		}
	}

	private void OnStartSceneLoad(OWScene currentScene, OWScene loadingScene)
	{
		if (currentScene == OWScene.SolarSystem && loadingScene != OWScene.SolarSystem)
		{
			UnloadRequiredColliders();
		}
	}

	private void OnCompleteSceneLoad(OWScene currentScene, OWScene loadingScene)
	{
	}

	private void OnMemoryUplinkAssetDump()
	{
		_ = _reloadAssetsOnDeath;
		if (!_reloadAssetsOnDeath && _streamCollidersOnWakeUp)
		{
			UnloadRequiredColliders();
		}
	}

	private void LoadRequiredColliders(int priorityBias = 0)
	{
		StreamingManager.LoadStreamingAssets(_terrainColliderSceneMeshBundle, 18 + priorityBias);
		StreamingManager.LoadStreamingAssets(_structuresColliderSceneMeshBundle, 17 + priorityBias);
	}

	private void UnloadRequiredColliders(float delay = 0f)
	{
		StreamingManager.UnloadStreamingAssets(_terrainColliderSceneMeshBundle, delay);
		StreamingManager.UnloadStreamingAssets(_structuresColliderSceneMeshBundle, delay);
	}

	private void LoadRequiredAssets(int priorityBias = 0)
	{
		if (!_locked)
		{
			StreamingManager.LoadStreamingAssets(_bakedTerrainsBundle, 8 + priorityBias);
			StreamingManager.LoadStreamingAssets(_batchedRenderersBundle, 8 + priorityBias);
			StreamingManager.LoadStreamingAssets(_bakedVISRenderersBundle, 8 + priorityBias);
			StreamingManager.LoadStreamingAssets(_terrainSceneMeshBundle, 8 + priorityBias);
			StreamingManager.LoadStreamingAssets(_structuresSceneMeshBundle, 7 + priorityBias);
			StreamingManager.LoadStreamingAssets(_batchedRenderersColliderBundle, 18 + priorityBias);
			if (!_streamCollidersOnWakeUp)
			{
				StreamingManager.LoadStreamingAssets(_terrainColliderSceneMeshBundle, 18 + priorityBias);
				StreamingManager.LoadStreamingAssets(_structuresColliderSceneMeshBundle, 17 + priorityBias);
			}
			_requiredAssetsPriorityBias = priorityBias;
		}
	}

	private void UpdateRequiredAssets(int priorityBias = 0)
	{
		if (!_locked)
		{
			StreamingManager.SetStreamingAssetsPriority(_bakedTerrainsBundle, 8 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_batchedRenderersBundle, 8 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_batchedRenderersColliderBundle, 18 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_bakedVISRenderersBundle, 8 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_terrainSceneMeshBundle, 8 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_terrainColliderSceneMeshBundle, 18 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_structuresSceneMeshBundle, 7 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_structuresColliderSceneMeshBundle, 17 + priorityBias);
			_requiredAssetsPriorityBias = priorityBias;
		}
	}

	private void UnloadRequiredAssets(float delay = 0f)
	{
		if (!_locked)
		{
			StreamingManager.UnloadStreamingAssets(_bakedTerrainsBundle, delay);
			StreamingManager.UnloadStreamingAssets(_batchedRenderersBundle, delay);
			StreamingManager.UnloadStreamingAssets(_bakedVISRenderersBundle, delay);
			StreamingManager.UnloadStreamingAssets(_terrainSceneMeshBundle, delay);
			StreamingManager.UnloadStreamingAssets(_structuresSceneMeshBundle, delay);
			StreamingManager.UnloadStreamingAssets(_batchedRenderersColliderBundle, delay);
		}
	}

	private void LoadGeneralAssets(int priorityBias = 0)
	{
		if (!_locked)
		{
			StreamingManager.LoadStreamingAssets(_detailPatchesBundle, 4 + priorityBias);
			StreamingManager.LoadStreamingAssets(_decalsBundle, 4 + priorityBias);
			StreamingManager.LoadStreamingAssets(_propsSceneMeshBundle, 5 + priorityBias);
			StreamingManager.LoadStreamingAssets(_propsColliderSceneMeshBundle, 15 + priorityBias);
			StreamingManager.LoadStreamingAssets(_charactersSceneMeshBundle, 6 + priorityBias);
			StreamingManager.LoadStreamingAssets(_charactersColliderSceneMeshBundle, 16 + priorityBias);
			StreamingManager.LoadStreamingAssets(_effectsSceneMeshBundle, 4 + priorityBias);
			StreamingManager.LoadStreamingAssets(_effectsColliderSceneMeshBundle, 14 + priorityBias);
			for (int i = 0; i < _streamingMaterialTables.Length; i++)
			{
				int streamingMaterialTablePriority = GetStreamingMaterialTablePriority(_streamingMaterialTables[i]);
				StreamingManager.LoadStreamingAssets(_streamingMaterialTables[i].assetBundle, streamingMaterialTablePriority + priorityBias);
			}
			_generalAssetsPriorityBias = priorityBias;
		}
	}

	private void UpdateGeneralAssets(int priorityBias = 0)
	{
		if (!_locked)
		{
			StreamingManager.SetStreamingAssetsPriority(_detailPatchesBundle, 4 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_decalsBundle, 4 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_propsSceneMeshBundle, 5 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_propsColliderSceneMeshBundle, 15 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_charactersSceneMeshBundle, 6 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_charactersColliderSceneMeshBundle, 16 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_effectsSceneMeshBundle, 4 + priorityBias);
			StreamingManager.SetStreamingAssetsPriority(_effectsColliderSceneMeshBundle, 14 + priorityBias);
			for (int i = 0; i < _streamingMaterialTables.Length; i++)
			{
				int streamingMaterialTablePriority = GetStreamingMaterialTablePriority(_streamingMaterialTables[i]);
				StreamingManager.SetStreamingAssetsPriority(_streamingMaterialTables[i].assetBundle, streamingMaterialTablePriority + priorityBias);
			}
			_generalAssetsPriorityBias = priorityBias;
		}
	}

	private void UnloadGeneralAssets(float delay = 0f)
	{
		if (!_locked)
		{
			StreamingManager.UnloadStreamingAssets(_detailPatchesBundle, delay);
			StreamingManager.UnloadStreamingAssets(_decalsBundle, delay);
			StreamingManager.UnloadStreamingAssets(_propsSceneMeshBundle, delay);
			StreamingManager.UnloadStreamingAssets(_propsColliderSceneMeshBundle, delay);
			StreamingManager.UnloadStreamingAssets(_charactersSceneMeshBundle, delay);
			StreamingManager.UnloadStreamingAssets(_charactersColliderSceneMeshBundle, delay);
			StreamingManager.UnloadStreamingAssets(_effectsSceneMeshBundle, delay);
			StreamingManager.UnloadStreamingAssets(_effectsColliderSceneMeshBundle, delay);
			for (int i = 0; i < _streamingMaterialTables.Length; i++)
			{
				StreamingManager.UnloadStreamingAssets(_streamingMaterialTables[i].assetBundle, delay);
			}
		}
	}

	private int GetStreamingMaterialTablePriority(StreamingMaterialTable streamingMaterialTable)
	{
		switch (streamingMaterialTable.type)
		{
		case StreamingMaterialTable.Type.Terrain:
			return 3;
		case StreamingMaterialTable.Type.Detail:
			return 1;
		case StreamingMaterialTable.Type.Character:
			return 0;
		default:
			return 2;
		}
	}

	public static StreamingGroup GetStreamingGroup(string sceneName)
	{
		for (int i = 0; i < s_streamingGroups.Count; i++)
		{
			if (s_streamingGroups[i]._sceneName == sceneName)
			{
				return s_streamingGroups[i];
			}
		}
		return null;
	}

	public void RequestRequiredAssets(int priorityBias = 0)
	{
		if (_numRequiredAssetsRequests == 0)
		{
			LoadRequiredAssets(priorityBias);
		}
		else if (priorityBias != _requiredAssetsPriorityBias)
		{
			UpdateRequiredAssets(priorityBias);
		}
		_numRequiredAssetsRequests++;
	}

	public void ReleaseRequiredAssets()
	{
		_numRequiredAssetsRequests--;
		if (_numRequiredAssetsRequests == 0)
		{
			UnloadRequiredAssets(15f);
		}
	}

	public void RequestGeneralAssets(int priorityBias = 0)
	{
		if (_numGeneralAssetsRequests == 0)
		{
			LoadGeneralAssets(priorityBias);
		}
		else if (priorityBias != _generalAssetsPriorityBias)
		{
			UpdateGeneralAssets(priorityBias);
		}
		_numGeneralAssetsRequests++;
	}

	public void ReleaseGeneralAssets()
	{
		_numGeneralAssetsRequests--;
		if (_numGeneralAssetsRequests == 0)
		{
			UnloadGeneralAssets(5f);
		}
	}

	public bool AreRequiredAssetsLoaded()
	{
		if (_bakedTerrainsBundleState.isLoaded && _batchedRenderersBundleState.isLoaded && _batchedRenderersColliderBundleState.isLoaded && _bakedVISRenderersBundleState.isLoaded && _terrainSceneMeshBundleState.isLoaded && _terrainColliderSceneMeshBundleState.isLoaded && _structuresSceneMeshBundleState.isLoaded)
		{
			return _structuresColliderSceneMeshBundleState.isLoaded;
		}
		return false;
	}
}
