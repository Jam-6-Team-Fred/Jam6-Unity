using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;

[CanEditMultipleObjects]
[CustomEditor(typeof(GhostNodeMarker))]
public class GhostNodeMarkerEditor : Editor
{
    static bool connectMultiSelected = true;
    const string fieldName = "_markerEdges";
    void Awake()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }
    void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Disconnect All")) DisconnectNodes();
        connectMultiSelected = GUILayout.Toggle(connectMultiSelected, "Connect Two Selected Nodes");

        base.OnInspectorGUI();

        CheckForSelection();
    }
    void OnSceneGUI(SceneView view)
    {
        CheckForSelection();
    }
    void CheckForSelection()
    {
        if (!connectMultiSelected) return;

        var selected = targets;
        if (selected.Length != 2) return;

        GhostNodeMarker markerA = (GhostNodeMarker)selected[0];
        GhostNodeMarker markerB = (GhostNodeMarker)selected[1];

        GhostNodeMap map = markerA.GetComponentInParent<GhostNodeMap>();
        if (map == null) { Debug.LogWarning("Node map not found in parent!"); return; }

        var fi = typeof(GhostNodeMap).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        List<GhostMarkerEdge> edgeList = (List<GhostMarkerEdge>)fi.GetValue(map);

        GhostMarkerEdge newEdge = new GhostMarkerEdge(null, null); //constructor doesn't work.
        newEdge.markerOne = markerA;
        newEdge.markerTwo = markerB;

        //Check if already exists.
        for (int i = 0; i < edgeList.Count; i++)
        {
            var edge = edgeList[i];
            if ((edge.markerOne == markerA && edge.markerTwo == markerB) || 
                (edge.markerOne == markerB && edge.markerTwo == markerA))
            {
                return;
            }
        }

        edgeList.Add(newEdge);
        SetMap(map, fi, edgeList, "Add Connection");

        Selection.activeGameObject = null; //markerB.gameObject;
    }
    void SetMap(GhostNodeMap map, FieldInfo fi, List<GhostMarkerEdge> edgeList, string undo)
    {
        Undo.RecordObject(map, undo);
        EditorUtility.SetDirty(map);
        fi.SetValue(map, edgeList);
        PrefabUtility.RecordPrefabInstancePropertyModifications(map);
    }

    void DisconnectNodes()
    {
        var t = targets; //Casting not working?
        GhostNodeMarker[] selected = new GhostNodeMarker[t.Length];
        for (int i = 0; i < t.Length; i++) selected[i] = (GhostNodeMarker)t[i];

        GhostNodeMap map = selected[0].GetComponentInParent<GhostNodeMap>();
        if (map == null) { Debug.LogWarning("Node map not found in parent!"); return; }

        var fi = typeof(GhostNodeMap).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        List<GhostMarkerEdge> edgeList = (List<GhostMarkerEdge>)fi.GetValue(map);

        foreach (var marker in selected)
        {
            for (int i = 0; i < edgeList.Count; i++)
            {
                if (edgeList[i].markerOne == marker || edgeList[i].markerTwo == marker)
                {
                    edgeList.RemoveAt(i);
                    i--;
                }
            }
        }

        SetMap(map, fi, edgeList, "Disconnect");
    }
}