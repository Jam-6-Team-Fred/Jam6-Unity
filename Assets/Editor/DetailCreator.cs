using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

public class DetailCreator : EditorWindow
{
    Vector2 scroll;
    int _presetIndex = 0;
    string[] _presets = new string[0];
    string[] _presetNames = new string[0];
    DetailCreatorPreset _currentPreset;
    bool _savePreset;
    bool _deletePreset;
    bool _saveNewPreset;
    string _newPresetName;
    Transform _targetObj = null;
    bool _targetObjAssigned = false;
    SpawnAreaOptions _spawnAreaType;
    float _spawnRadius = 5f;
    float _verticalDistance = 5f;
    Vector3 _spawnAreaScale = Vector3.one * 10f;
    bool _hideSpawnArea = true;
    bool _removeNonSnapped = true;
    bool _preventOverlap = true;
    float _minDistance = 1f;
    float _correctionDistance = 0.2f;
    int _overlapIterations = 3;
    float _minAmount = 20;
    float _maxAmount = 40;
    bool _allowStacking = false;
    bool _syncPrefabOptions = false;
    int _syncingOptionsIndex;
    bool _showPrefabList = false;
    int _prefabListLength;
    List<PrefabOptions> _prefabsToSpawn = new List<PrefabOptions>();
    bool _clearOnGenerate = true;
    bool _generatePrefabs;
    bool _clearPrefabs;
    int _seed;
    bool _autoSeed = true;
    bool _ignoreSnapTargets = false;
    [SerializeField] List<GameObject> _snapTargets = new List<GameObject>();
    List<Vector2> _prefabPositions = new List<Vector2>();
    List<GameObject> _prefabsToEnable = new List<GameObject>();
    Transform _targetParent = null;
    bool _confirmPrefabs = false;

    [System.Serializable]
    public enum SpawnAreaOptions
    {
        Cylinder,
        Box
    }

    [System.Serializable]
    public enum AxisOptions
    {
        X,
        Y,
        Z,
        All
    }

    [System.Serializable]
    public class PrefabOptions
    {
        public bool showOptions = false;
        public GameObject prefab = null;
        public int weight = 1;
        public float minScale = 0.8f;
        public float maxScale = 1.2f;
        public float phase = 1f;
        public AxisOptions axis = AxisOptions.Y;
        public bool snapToSurface = true;
        public bool canSnapAbove = false;
        public bool alignToSurface = true;
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;

        List<GameObject> emptyTargets = new List<GameObject>();
        foreach (GameObject obj in _snapTargets)
        {
            if (obj == null)
            {
                emptyTargets.Add(obj);
            }
        }
        emptyTargets.ForEach(obj => _snapTargets.Remove(obj));

        UpdatePresetsList();
        if (_presetNames.Length > 1)
        {
            LoadPreset((DetailCreatorPreset)AssetDatabase.LoadAssetAtPath($"Assets/Editor/DetailCreatorPresets/{_presetNames[1]}.asset", typeof(DetailCreatorPreset)));
        }
    }

    [MenuItem("Tools/Detail Creator")]
    public static void InitWindow()
    {
        EditorWindow window = GetWindow<DetailCreator>();
        window.titleContent = new GUIContent("Detail Creator");
    }

    public void Update()
    {
        _targetObjAssigned = _targetObj != null;

        if (_targetObjAssigned)
        {
            if ((EditorSceneManager.IsPreviewSceneObject(_targetObj.gameObject) && PrefabStageUtility.GetCurrentPrefabStage() == null)
            || (!EditorSceneManager.IsPreviewSceneObject(_targetObj.gameObject) && PrefabStageUtility.GetCurrentPrefabStage() != null))
            {
                _targetObj = null;
                _targetParent = null;
                _targetObjAssigned = false;
            }
        }
    }

    public void OnGUI()
    {
        SerializedObject so = new SerializedObject(this);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        EditorGUI.indentLevel++;
        UpdatePresetsList();

        EditorGUI.BeginChangeCheck();
        _presetIndex = EditorGUILayout.Popup(new GUIContent("Preset"), _presetIndex, _presetNames);
        if (EditorGUI.EndChangeCheck() && _presetIndex > 0)
        {
            LoadPreset((DetailCreatorPreset)AssetDatabase.LoadAssetAtPath($"Assets/Editor/DetailCreatorPresets/{_presetNames[_presetIndex]}.asset", typeof(DetailCreatorPreset)));
        }

        EditorGUILayout.BeginHorizontal();
        _savePreset = GUILayout.Button(new GUIContent("Save Preset"));
        if (_savePreset && _presetIndex > 0)
        {
            SavePreset(_presetNames[_presetIndex]);
        }
        _deletePreset = GUILayout.Button(new GUIContent("Delete Preset"));
        if (_deletePreset && _presetIndex > 0)
        {
            AssetDatabase.DeleteAsset($"Assets/Editor/DetailCreatorPresets/{_presetNames[_presetIndex]}.asset");
            AssetDatabase.SaveAssets();
            UpdatePresetsList();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        _saveNewPreset = GUILayout.Button(new GUIContent("Save New Preset"));
        _newPresetName = EditorGUILayout.TextField(_newPresetName);
        if (_saveNewPreset && !string.IsNullOrWhiteSpace(_newPresetName))
        {
            DetailCreatorPreset newPreset = CreateInstance<DetailCreatorPreset>();
            SavePreset(_newPresetName);
            UpdatePresetsList();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        _targetObj = (Transform)EditorGUILayout.ObjectField(new GUIContent("Target Transform",
            "The scene object that prefabs are spawned around. Prefabs will also become children of this object."),
            _targetObj, typeof(Transform), true);

        if (!_targetObjAssigned)
        {
            EditorGUILayout.Space(10);
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Drag in an object from the scene to use as a reference for the position and rotation.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
            return;
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Amount",
            "The number of prefabs to spawn."), GUILayout.Width(100));
        _minAmount = EditorGUILayout.DelayedFloatField(_minAmount, GUILayout.Width(60));
        EditorGUILayout.MinMaxSlider(ref _minAmount, ref _maxAmount, 1, 100);
        _maxAmount = EditorGUILayout.DelayedFloatField(_maxAmount, GUILayout.Width(60));
        _minAmount = Mathf.RoundToInt(Mathf.Clamp(_minAmount, 1, 100));
        _maxAmount = Mathf.RoundToInt(Mathf.Clamp(_maxAmount, 1, 100));
        if (_minAmount > _maxAmount)
        {
            _maxAmount = _minAmount;
        }
        EditorGUILayout.EndHorizontal();

        _spawnAreaType = (SpawnAreaOptions)EditorGUILayout.EnumPopup(new GUIContent("Spawn Area Type",
            "The shape of the area that prefabs can spawn in."), _spawnAreaType);

        EditorGUI.indentLevel++;
        if (_spawnAreaType == SpawnAreaOptions.Cylinder)
        {
            _spawnRadius = EditorGUILayout.FloatField(new GUIContent("Spawn Radius",
                "The radius around the target transform where prefabs can spawn."), _spawnRadius);
            _verticalDistance = EditorGUILayout.FloatField(new GUIContent("Spawn Height",
                "The vertical area where prefabs can stick to a surface."), _verticalDistance);
        }
        else
        {
            _spawnAreaScale = EditorGUILayout.Vector3Field(new GUIContent("Scale",
                "The X, Y and Z dimensions of the spawn area."), _spawnAreaScale);
        }
        EditorGUI.indentLevel--;

        _hideSpawnArea = EditorGUILayout.Toggle(new GUIContent("Hide Spawn Area",
            "Only shows an outline of the spawn area when selecting the target transform."), _hideSpawnArea);
        _removeNonSnapped = EditorGUILayout.Toggle(new GUIContent("Remove Floating Prefabs",
            "Skip generating the prefabs that aren't snapped to a surface."), _removeNonSnapped);
        _preventOverlap = EditorGUILayout.Toggle(new GUIContent("Prevent Overlap",
            "Prevents prefabs from overlapping each other when spawning (not 100% effective)."), _preventOverlap);
        if (_preventOverlap)
        {
            EditorGUI.indentLevel++;
            _minDistance = EditorGUILayout.DelayedFloatField(new GUIContent("Minimum Distance",
                "It will count as an overlap if two prefabs are closer than this."), _minDistance);
            _correctionDistance = EditorGUILayout.DelayedFloatField(new GUIContent("Correction",
                "The distance it moves other prefabs away to prevent overlapping."), _correctionDistance);
            _overlapIterations = EditorGUILayout.IntSlider(new GUIContent("Iterations",
                "The number of times to check for overlaps."), _overlapIterations, 1, 10);
            EditorGUI.indentLevel--;
        }

        _allowStacking = EditorGUILayout.Toggle(new GUIContent("Allow Prefab Stacking",
            "Lets prefabs generate on top of each other if there are enough generating at the same time."), _allowStacking);

        EditorGUILayout.Space(10);

        _ignoreSnapTargets = EditorGUILayout.Toggle(new GUIContent("Ignore Snap Targets",
            "Snaps to everything except the snap targets, rather than only snapping to the snap targets."), _ignoreSnapTargets);

        EditorGUI.indentLevel--;

        EditorGUILayout.PropertyField(so.FindProperty(nameof(_snapTargets)), new GUIContent("Snap Targets",
            "Specific colliders that the prefabs should snap to. Leave empty to disable."));

        EditorGUILayout.Space(10);

        EditorGUI.indentLevel++;
        _syncPrefabOptions = EditorGUILayout.Toggle(new GUIContent("Sync Prefabs",
            "Syncs all of the prefab values across each of the prefabs."), _syncPrefabOptions);
        EditorGUI.indentLevel--;

        EditorGUILayout.BeginHorizontal();
        _showPrefabList = EditorGUILayout.Foldout(_showPrefabList, new GUIContent("Prefabs to Spawn"), true);
        char upArrow = '\u25B2';
        char downArrow = '\u25BC';
        if (GUILayout.Button(downArrow.ToString(), GUILayout.Width(30))) _prefabListLength--;
        if (GUILayout.Button(upArrow.ToString(), GUILayout.Width(30))) _prefabListLength++;
        _prefabListLength = EditorGUILayout.DelayedIntField(_prefabListLength, GUILayout.Width(50));
        _prefabListLength = Mathf.Clamp(_prefabListLength, 0, 500);
        EditorGUILayout.EndHorizontal();

        if (_prefabsToSpawn.Count < _prefabListLength)
        {
            for (int i = 0; i < _prefabListLength - _prefabsToSpawn.Count; i++)
            {
                _prefabsToSpawn.Add(new PrefabOptions());
            }
        }
        else if (_prefabsToSpawn.Count > _prefabListLength)
        {
            for (int i = 0; i < _prefabsToSpawn.Count - _prefabListLength; i++)
            {
                _prefabsToSpawn.RemoveAt(_prefabsToSpawn.Count - 1);
            }
        }

        List<PrefabOptions> prefabsToRemove = new List<PrefabOptions>();

        if (_showPrefabList && _prefabListLength > 0)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < _prefabListLength; i++)
            {
                if (_prefabsToSpawn[i].prefab == null)
                {
                    _prefabsToSpawn[i].showOptions = EditorGUILayout.Foldout(_prefabsToSpawn[i].showOptions,
                        new GUIContent($"- Element {i + 1} -"), true);
                }
                else
                {
                    _prefabsToSpawn[i].showOptions = EditorGUILayout.Foldout(_prefabsToSpawn[i].showOptions,
                        new GUIContent($"{_prefabsToSpawn[i].prefab.name}"), true);
                }

                if (_prefabsToSpawn[i].showOptions)
                {
                    EditorGUI.indentLevel++;

                    _prefabsToSpawn[i].prefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent($"Prefab"),
                        _prefabsToSpawn[i].prefab, typeof(GameObject), false);

                    EditorGUI.BeginChangeCheck();
                    _prefabsToSpawn[i].weight = EditorGUILayout.DelayedIntField(new GUIContent("Weight",
                        "How often this prefab spawns compared to others. Not measured in percentage."), _prefabsToSpawn[i].weight);
                    if (EditorGUI.EndChangeCheck() && _syncPrefabOptions)
                    {
                        _syncingOptionsIndex = i;
                    }
                    else if (_syncPrefabOptions && _prefabsToSpawn[i].weight != _prefabsToSpawn[_syncingOptionsIndex].weight)
                    {
                        _prefabsToSpawn[i].weight = _prefabsToSpawn[_syncingOptionsIndex].weight;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Scale",
                        "The minimum and maximum scale that this prefab can spawn with."), GUILayout.Width(75));

                    EditorGUI.BeginChangeCheck();
                    _prefabsToSpawn[i].minScale = EditorGUILayout.DelayedFloatField(_prefabsToSpawn[i].minScale, GUILayout.Width(60));

                    EditorGUILayout.MinMaxSlider(ref _prefabsToSpawn[i].minScale, ref _prefabsToSpawn[i].maxScale, 0, 3);

                    _prefabsToSpawn[i].maxScale = EditorGUILayout.DelayedFloatField(_prefabsToSpawn[i].maxScale, GUILayout.Width(60));
                    if (EditorGUI.EndChangeCheck() && _syncPrefabOptions)
                    {
                        _syncingOptionsIndex = i;
                    }
                    else if (_syncPrefabOptions && (_prefabsToSpawn[i].minScale != _prefabsToSpawn[_syncingOptionsIndex].minScale
                        || _prefabsToSpawn[i].maxScale != _prefabsToSpawn[_syncingOptionsIndex].maxScale))
                    {
                        _prefabsToSpawn[i].minScale = _prefabsToSpawn[_syncingOptionsIndex].minScale;
                        _prefabsToSpawn[i].maxScale = _prefabsToSpawn[_syncingOptionsIndex].maxScale;
                    }

                    _prefabsToSpawn[i].minScale = Mathf.Round(Mathf.Clamp(_prefabsToSpawn[i].minScale, 0, 3) * 100) / 100;
                    _prefabsToSpawn[i].maxScale = Mathf.Round(Mathf.Clamp(_prefabsToSpawn[i].maxScale, 0, 3) * 100) / 100;
                    if (_prefabsToSpawn[i].minScale > _prefabsToSpawn[i].maxScale)
                    {
                        _prefabsToSpawn[i].maxScale = _prefabsToSpawn[i].minScale;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginChangeCheck();
                    _prefabsToSpawn[i].phase = EditorGUILayout.Slider(new GUIContent("Phase",
                        "The amount of variation in the rotation on the selected axis."), _prefabsToSpawn[i].phase, 0f, 1f);
                    if (EditorGUI.EndChangeCheck() && _syncPrefabOptions)
                    {
                        _syncingOptionsIndex = i;
                    }
                    else if (_syncPrefabOptions && _prefabsToSpawn[i].phase != _prefabsToSpawn[_syncingOptionsIndex].phase)
                    {
                        _prefabsToSpawn[i].phase = _prefabsToSpawn[_syncingOptionsIndex].phase;
                    }

                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    _prefabsToSpawn[i].axis = (AxisOptions)EditorGUILayout.EnumPopup(new GUIContent("Axis",
                        "The axis to rotate on."), _prefabsToSpawn[i].axis);
                    if (EditorGUI.EndChangeCheck() && _syncPrefabOptions)
                    {
                        _syncingOptionsIndex = i;
                    }
                    else if (_syncPrefabOptions && _prefabsToSpawn[i].axis != _prefabsToSpawn[_syncingOptionsIndex].axis)
                    {
                        _prefabsToSpawn[i].axis = _prefabsToSpawn[_syncingOptionsIndex].axis;
                    }

                    EditorGUI.indentLevel--;

                    EditorGUI.BeginChangeCheck();
                    _prefabsToSpawn[i].snapToSurface = EditorGUILayout.Toggle(new GUIContent("Snap to Surface",
                        "Moves the object down to the nearest surface."), _prefabsToSpawn[i].snapToSurface);
                    if (EditorGUI.EndChangeCheck() && _syncPrefabOptions)
                    {
                        _syncingOptionsIndex = i;
                    }
                    else if (_syncPrefabOptions && _prefabsToSpawn[i].snapToSurface != _prefabsToSpawn[_syncingOptionsIndex].snapToSurface)
                    {
                        _prefabsToSpawn[i].snapToSurface = _prefabsToSpawn[_syncingOptionsIndex].snapToSurface;
                    }

                    if (_prefabsToSpawn[i].snapToSurface)
                    {
                        EditorGUI.BeginChangeCheck();
                        _prefabsToSpawn[i].canSnapAbove = EditorGUILayout.Toggle(new GUIContent("Snap From Top",
                            "Snaps down from the top of the spawn area rather than the center."), _prefabsToSpawn[i].canSnapAbove);
                        if (EditorGUI.EndChangeCheck() && _syncPrefabOptions)
                        {
                            _syncingOptionsIndex = i;
                        }
                        else if (_syncPrefabOptions && _prefabsToSpawn[i].canSnapAbove != _prefabsToSpawn[_syncingOptionsIndex].canSnapAbove)
                        {
                            _prefabsToSpawn[i].canSnapAbove = _prefabsToSpawn[_syncingOptionsIndex].canSnapAbove;
                        }
                    }

                    if (_prefabsToSpawn[i].snapToSurface)
                    {
                        EditorGUI.BeginChangeCheck();
                        _prefabsToSpawn[i].alignToSurface = EditorGUILayout.Toggle(new GUIContent("Align to Surface",
                            "Rotates the prefab to be perpendicular to the surface it is on."), _prefabsToSpawn[i].alignToSurface);
                        if (EditorGUI.EndChangeCheck() && _syncPrefabOptions)
                        {
                            _syncingOptionsIndex = i;
                        }
                        else if (_syncPrefabOptions && _prefabsToSpawn[i].alignToSurface != _prefabsToSpawn[_syncingOptionsIndex].alignToSurface)
                        {
                            _prefabsToSpawn[i].alignToSurface = _prefabsToSpawn[_syncingOptionsIndex].alignToSurface;
                        }
                    }

                    if (GUILayout.Button(new GUIContent("Remove Prefab")))
                    {
                        prefabsToRemove.Add(_prefabsToSpawn[i]);
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(10);
                }
            }

            foreach (PrefabOptions prefab in prefabsToRemove)
            {
                _prefabListLength = Mathf.Max(_prefabListLength - 1, 0);
                _prefabsToSpawn.Remove(prefab);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(20);

        _clearOnGenerate = EditorGUILayout.Toggle(new GUIContent("Clear on Generate",
            "Clear the last generated prefabs when generating more."), _clearOnGenerate);

        EditorGUILayout.BeginHorizontal();
        _seed = EditorGUILayout.DelayedIntField(new GUIContent("Seed",
            "The seed to use when generating."), _seed);
        _seed = Mathf.Clamp(_seed, 0, int.MaxValue);
        if (GUILayout.Button(new GUIContent("Randomize",
            "Randomize the seed.")))
        {
            _seed = Random.Range(0, 10000000);
        }
        EditorGUILayout.EndHorizontal();
        _autoSeed = EditorGUILayout.Toggle(new GUIContent("Auto Randomize",
            "Automatically randomize the seed when generating."), _autoSeed);

        EditorGUILayout.Space();

        _targetParent = (Transform)EditorGUILayout.ObjectField(new GUIContent("Target Parent",
            "The parent transform that generated objects will move to after Confirm Prefabs is clicked."),
            _targetParent, typeof(Transform), true);

        EditorGUILayout.Space();

        _generatePrefabs = GUILayout.Button(new GUIContent("Generate Prefabs"));

        if (_targetObj.childCount > 0)
        {
            _clearPrefabs = GUILayout.Button(new GUIContent("Clear Prefabs"));
            EditorGUILayout.LabelField(new GUIContent("Warning: Clearing the prefabs clears all children of the target transform. " +
                "Make sure you don't have anything important under the target transform before clearing."), EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.Space();

        if (_targetParent != null && _targetParent != _targetObj && _targetObj.childCount > 0)
        {
            _confirmPrefabs = GUILayout.Button(new GUIContent("Confirm Prefabs"));
            EditorGUILayout.LabelField(new GUIContent("Confirming the prefabs moves them to be parents of the Target Parent."), EditorStyles.wordWrappedLabel);
        }

        if (_generatePrefabs)
        {
            GeneratePrefabs();
        }
        if (_clearPrefabs)
        {
            ClearPrefabs();
        }
        if (_confirmPrefabs)
        {
            ConfirmPrefabs();
        }

        so.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
    }

    private void UpdatePresetsList()
    {
        string[] presets = AssetDatabase.FindAssets("", new[] { "Assets/Editor/DetailCreatorPresets" });
        if (presets != _presets)
        {
            _presets = presets;
            List<string> presetNames = new List<string>();
            foreach (string guid in _presets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = path.Split('/')[3];
                presetNames.Add(name.Replace(".asset", ""));
            }
            presetNames.Insert(0, "");
            if (_presetNames.Length < presetNames.Count) _presetIndex++;
            else if (_presetNames.Length > presetNames.Count)
            {
                _presetIndex--;
                if (_presetIndex == 0 && presetNames.Count > 1) _presetIndex++;
            }
            _presetNames = presetNames.ToArray();
        }
    }

    private void GeneratePrefabs()
    {
        if (_prefabsToSpawn.Count <= 0) return;

        if (_autoSeed)
        {
            _seed = Random.Range(00000000, 10000000);
        }
        Random.InitState(_seed);

        if (_clearOnGenerate)
        {
            ClearPrefabs();
        }

        int amount = Random.Range((int)_minAmount, (int)_maxAmount + 1);

        for (int i = 0; i < amount; i++)
        {
            Vector2 planePos;
            if (_spawnAreaType == SpawnAreaOptions.Cylinder)
            {
                planePos = Random.insideUnitCircle * _spawnRadius;
            }
            else
            {
                planePos = new Vector2(Random.Range(-_spawnAreaScale.x, _spawnAreaScale.x),
                    Random.Range(-_spawnAreaScale.z, _spawnAreaScale.z)) * 0.5f;
            }

            if (_preventOverlap)
            {
                for (int j = 0; j < _prefabPositions.Count; j++)
                {
                    Vector2 dir = _prefabPositions[j] - planePos;
                    if (dir.sqrMagnitude < _minDistance * _minDistance)
                    {
                        _prefabPositions[j] += dir.normalized * _correctionDistance;
                    }
                }
            }
            _prefabPositions.Add(planePos);
        }

        if (_preventOverlap && _overlapIterations > 1)
        {
            for (int k = 0; k < _overlapIterations; k++)
            {
                Vector2[] oldPositions = _prefabPositions.ToArray();
                _prefabPositions.Clear();
                for (int m = 0; m < amount; m++)
                {
                    CorrectOverlap(oldPositions[m]);
                }
            }
        }

        for (int h = 0; h < amount; h++)
        {
            int index = GetRandomPrefab();
            if (_prefabsToSpawn[index].prefab == null)
            {
                continue;
            }
            CreatePrefab(index, _prefabPositions[h]);
        }
        foreach (GameObject prefab in _prefabsToEnable)
        {
            prefab.SetActive(true);
        }
        _prefabsToEnable.Clear();
        _prefabPositions.Clear();
    }

    private void CorrectOverlap(Vector2 pos)
    {
        for (int j = 0; j < _prefabPositions.Count; j++)
        {
            Vector2 dir = _prefabPositions[j] - pos;
            if (dir.sqrMagnitude < _minDistance * _minDistance)
            {
                _prefabPositions[j] += dir.normalized * _correctionDistance;
            }
        }
        _prefabPositions.Add(pos);
    }

    private int GetRandomPrefab()
    {
        int sum = _prefabsToSpawn.Sum(prefab => prefab.weight);
        int rand = Random.Range(0, sum);
        for (int i = 0; i < _prefabsToSpawn.Count; i++)
        {
            if (rand < _prefabsToSpawn[i].weight)
            {
                return i;
            }

            rand -= _prefabsToSpawn[i].weight;
        }
        Debug.LogError("How did you get here?");
        return -1;
    }

    private void CreatePrefab(int index, Vector2 planePos)
    {
        Vector3 position = _targetObj.TransformPoint(new Vector3(planePos.x, 0, planePos.y));
        //Debug.Log(planePos);
        //Debug.Log("test: " + (_targetObj.transform.position - position));
        if (_spawnAreaType == SpawnAreaOptions.Box)
        {
            position = _targetObj.TransformPoint(new Vector3(planePos.x, 0, planePos.y));
        }
        Vector3 rotation = Vector3.zero;
        Vector3 transformPosition;
        float verticalDistAtPoint;
        float scale = _targetObj.localScale.y;

        if (_prefabsToSpawn[index].canSnapAbove)
        {
            transformPosition = _targetObj.TransformPoint(new Vector3(planePos.x,
                _verticalDistance, planePos.y));
            verticalDistAtPoint = _verticalDistance * 2;
            if (_spawnAreaType == SpawnAreaOptions.Box)
            {
                transformPosition = _targetObj.TransformPoint(new Vector3(planePos.x,
                _spawnAreaScale.y * 0.5f, planePos.y));
                verticalDistAtPoint = _spawnAreaScale.y;
            }
        }
        else
        {
            transformPosition = _targetObj.TransformPoint(new Vector3(planePos.x, 0f, planePos.y));
            verticalDistAtPoint = _verticalDistance;
            if (_spawnAreaType == SpawnAreaOptions.Box)
            {
                transformPosition = _targetObj.TransformPoint(new Vector3(planePos.x, 0f, planePos.y));
                verticalDistAtPoint = _spawnAreaScale.y * 0.5f;
            }
        }

        verticalDistAtPoint *= scale;

        PhysicsScene physicsScene = Physics.defaultPhysicsScene;
        Scene scene = _targetObj.gameObject.scene;
        if (scene.IsValid())
        {
            physicsScene = scene.GetPhysicsScene();
        }

        bool flag = false;
        if (_prefabsToSpawn[index].snapToSurface && physicsScene.Raycast(transformPosition,
            -_targetObj.transform.up, out RaycastHit hit, verticalDistAtPoint, OWLayerMask.physicalMask))
        {
            if (_snapTargets.Count == 0 || (_snapTargets.Count > 0 &&
                (_snapTargets.Contains(hit.collider.gameObject) && !_ignoreSnapTargets
                || !_snapTargets.Contains(hit.collider.gameObject) && _ignoreSnapTargets)))
            {
                position = hit.point;
                if (_prefabsToSpawn[index].alignToSurface)
                {
                    flag = true;
                    rotation = hit.normal;
                }
            }
            else if (_removeNonSnapped) return;
        }
        else if (_removeNonSnapped) return;

        if (!flag)
        {
            rotation = _targetObj.transform.up;
        }

        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(_prefabsToSpawn[index].prefab);
        prefab.transform.position = position;
        prefab.transform.rotation = Quaternion.identity;
        prefab.transform.SetParent(_targetObj, true);

        prefab.transform.up = rotation;
        prefab.transform.localScale = Vector3.one * Random.Range(_prefabsToSpawn[index].minScale, _prefabsToSpawn[index].maxScale);
        switch (_prefabsToSpawn[index].axis)
        {
            case AxisOptions.X:
                prefab.transform.rotation = Quaternion.AngleAxis(Random.Range(-360f * _prefabsToSpawn[index].phase,
                    360f * _prefabsToSpawn[index].phase), prefab.transform.right) * prefab.transform.rotation;
                break;
            case AxisOptions.Y:
                prefab.transform.rotation = Quaternion.AngleAxis(Random.Range(-360f * _prefabsToSpawn[index].phase,
                    360f * _prefabsToSpawn[index].phase), prefab.transform.up) * prefab.transform.rotation;
                break;
            case AxisOptions.Z:
                prefab.transform.rotation = Quaternion.AngleAxis(Random.Range(-360f * _prefabsToSpawn[index].phase,
                    360f * _prefabsToSpawn[index].phase), prefab.transform.forward) * prefab.transform.rotation;
                break;
            case AxisOptions.All:
                prefab.transform.rotation = Quaternion.Euler(Random.rotation.eulerAngles * _prefabsToSpawn[index].phase);
                break;
        }

        Undo.RegisterCompleteObjectUndo(prefab, "Detail Generation");
        Undo.RegisterCompleteObjectUndo(prefab, "Detail Prefab");

        if (!_allowStacking)
        {
            prefab.SetActive(false);
            _prefabsToEnable.Add(prefab);
        }

        //EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private void ClearPrefabs()
    {
        if (_targetObj.childCount <= 0) return;

        var children = new List<GameObject>();
        foreach (Transform child in _targetObj)
        {
            children.Add(child.gameObject);
        }
        children.ForEach(child => Undo.DestroyObjectImmediate(child));
        _clearPrefabs = false;
    }

    private void ConfirmPrefabs()
    {
        if (_targetObj.childCount <= 0 || _targetParent == null) return;

        var children = new List<GameObject>();
        foreach (Transform child in _targetObj)
        {
            children.Add(child.gameObject);
        }
        children.ForEach(child => child.transform.parent = _targetParent);
        _confirmPrefabs = false;
    }

    private void SavePreset(string name)
    {
        DetailCreatorPreset asset = CreateInstance<DetailCreatorPreset>();
        asset.spawnAreaType = _spawnAreaType;
        asset.spawnRadius = _spawnRadius;
        asset.verticalDistance = _verticalDistance;
        asset.spawnAreaScale = _spawnAreaScale;
        asset.hideSpawnArea = _hideSpawnArea;
        asset.removeNonSnapped = _removeNonSnapped;
        asset.preventOverlap = _preventOverlap;
        asset.minDistance = _minDistance;
        asset.correctionDistance = _correctionDistance;
        asset.overlapIterations = _overlapIterations;
        asset.minAmount = _minAmount;
        asset.maxAmount = _maxAmount;
        asset.syncPrefabOptions = _syncPrefabOptions;
        asset.prefabListLength = _prefabListLength;
        asset.prefabsToSpawn = _prefabsToSpawn;
        asset.clearOnGenerate = _clearOnGenerate;
        asset.seed = _seed;
        asset.autoSeed = _autoSeed;
        asset.ignoreSnapTargets = _ignoreSnapTargets;
        asset.snapTargets = _snapTargets;
        AssetDatabase.CreateAsset(asset, $"Assets/Editor/DetailCreatorPresets/{name}.asset");
        AssetDatabase.SaveAssets();
    }

    private void LoadPreset(DetailCreatorPreset preset)
    {
        _spawnAreaType = preset.spawnAreaType;
        _spawnRadius = preset.spawnRadius;
        _verticalDistance = preset.verticalDistance;
        _spawnAreaScale = preset.spawnAreaScale;
        _hideSpawnArea = preset.hideSpawnArea;
        _removeNonSnapped = preset.removeNonSnapped;
        _preventOverlap = preset.preventOverlap;
        _minDistance = preset.minDistance;
        _correctionDistance = preset.correctionDistance;
        _overlapIterations = preset.overlapIterations;
        _minAmount = preset.minAmount;
        _maxAmount = preset.maxAmount;
        _syncPrefabOptions = preset.syncPrefabOptions;
        _prefabListLength = preset.prefabListLength;
        _prefabsToSpawn = preset.prefabsToSpawn;
        _clearOnGenerate = preset.clearOnGenerate;
        _seed = preset.seed;
        _autoSeed = preset.autoSeed;
        _ignoreSnapTargets = preset.ignoreSnapTargets;
        _snapTargets = preset.snapTargets;
    }

    private void OnSceneGUI(SceneView _)
    {
        if (_targetObjAssigned)
        {
            if (Selection.activeGameObject == _targetObj.gameObject
            || (Selection.activeGameObject != _targetObj.gameObject && !_hideSpawnArea))
            {
                Matrix4x4 originalMatrix = Handles.matrix;
                Handles.matrix = _targetObj.localToWorldMatrix;
                if (_spawnAreaType == SpawnAreaOptions.Cylinder)
                {
                    Vector3 vector = Vector3.up * _verticalDistance;
                    Vector3 vector2 = -vector;
                    Handles.DrawWireDisc(vector, Vector3.up, _spawnRadius);
                    Handles.DrawWireDisc(vector2, Vector3.down, _spawnRadius);
                    Handles.DrawWireDisc(Vector3.zero, Vector3.up, _spawnRadius);
                    Handles.DrawLine(vector + Vector3.right * _spawnRadius, vector2 + Vector3.right * _spawnRadius);
                    Handles.DrawLine(vector + Vector3.left * _spawnRadius, vector2 + Vector3.left * _spawnRadius);
                    Handles.DrawLine(vector + Vector3.forward * _spawnRadius, vector2 + Vector3.forward * _spawnRadius);
                    Handles.DrawLine(vector + Vector3.back * _spawnRadius, vector2 + Vector3.back * _spawnRadius);
                }
                else
                {
                    Handles.DrawWireCube(Vector3.zero, _spawnAreaScale);
                }
                Handles.matrix = originalMatrix;
            }
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
}