using System.Collections.Generic;
using UnityEngine;

public class DetailCreatorPreset : ScriptableObject
{
    public DetailCreator.SpawnAreaOptions spawnAreaType;
    public float spawnRadius;
    public float verticalDistance;
    public Vector3 spawnAreaScale;
    public bool hideSpawnArea;
    public bool removeNonSnapped;
    public bool preventOverlap;
    public float minDistance;
    public float correctionDistance;
    public int overlapIterations;
    public float minAmount;
    public float maxAmount;
    public bool syncPrefabOptions;
    public int prefabListLength;
    public List<DetailCreator.PrefabOptions> prefabsToSpawn = new List<DetailCreator.PrefabOptions>();
    public bool clearOnGenerate;
    public int seed;
    public bool autoSeed;
    public bool ignoreSnapTargets;
    public List<GameObject> snapTargets = new List<GameObject>();
}
