using TerrainBaker;
using UnityEngine;

[SelectionBase]
public class BakedTerrain : MonoBehaviour
{
	[SerializeField]
	private GameObject _terrainMeshGroup;

	[SerializeField]
	private GameObject _subtractiveMeshGroup;

	[SerializeField]
	private GameObject _staticMeshGroup;

	[SerializeField]
	private bool _bakeColliders;

	[SerializeField]
	private Settings _settings = Settings.defaultSettings;

	[SerializeField]
	private string _exportPath = "Assets/Models/BakedTerrain.asset";

	public GameObject terrainMeshGroup => _terrainMeshGroup;

	public GameObject subtractiveMeshGroup => _subtractiveMeshGroup;

	public GameObject staticMeshGroup => _staticMeshGroup;

	public bool bakeColliders => _bakeColliders;

	public Settings settings => _settings;

	public string exportPath => _exportPath;
}
