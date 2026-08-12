using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

[CustomEditor(typeof(GhostNodeMap))]
public class GhostNodeMapEditor : Editor
{
    //SerializedObject so;
    //SerializedProperty edges;
    GhostNodeMap map;

    static float xzRadius = 8f;
    static float yRadius = 5f;
    static bool showRangeY = false;
    static bool showRangeXZ = false;
    //static bool activated = true;
    static bool findClosestIfFailed = true;
    //GhostNodeMarker first;
    void Awake()
    {
        //so = new SerializedObject(target);
        //edges = so.FindProperty("_markerEdges");
        map = target as GhostNodeMap;
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }
    void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();
        showRangeXZ = EditorGUILayout.Toggle("Show XZ Range", showRangeXZ);
        showRangeY = EditorGUILayout.Toggle("Show Y Range", showRangeY);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        xzRadius = EditorGUILayout.FloatField("Max XZ Radius", xzRadius);
        yRadius = EditorGUILayout.FloatField("Max Y Distance", yRadius);
        EditorGUILayout.EndHorizontal();

        findClosestIfFailed = GUILayout.Toggle(findClosestIfFailed, "Find Closest if None in Range");
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll(); //Update view.
        }

        if (GUILayout.Button("Generate Node Map"))
        {
            AutoFind();
        }
        base.OnInspectorGUI();

        //activated = EditorGUILayout.Toggle("Activated", activated);
        //EditorGUILayout.ObjectField("First Node:", first, typeof(GhostNodeMarker), false);
    }
    void OnSceneGUI(SceneView view)
    {
        /*
        var selected = Selection.activeGameObject;
        if (selected.TryGetComponent(out GhostNodeMarker g))
        {
            if (first == null)
            {
                first = g;
            }
            else
            {
                //list.Add();

                EditorUtility.SetDirty(target);
                so.ApplyModifiedProperties();
            }
        }
        */

        if (showRangeY || showRangeXZ)
        {

            var markers = map.GetComponentsInChildren<GhostNodeMarker>();
            if (showRangeXZ)
            {
                foreach (var item in markers)
                {
                    var tf = item.transform;
                    Handles.DrawWireDisc(tf.position, Vector3.up, xzRadius);
                }
            }
            if (showRangeY)
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                foreach (var item in markers)
                {
                    var tf = item.transform;
                    Vector3 p0 = tf.position - Vector3.up * yRadius;
                    Vector3 p1 = tf.position + Vector3.up * yRadius;
                    Handles.DrawLine(p0, p1);
                    Handles.DrawWireDisc(p0, Vector3.up, xzRadius);
                    Handles.DrawWireDisc(p1, Vector3.up, xzRadius);
                }
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Never;
            }
        }

        /*
        var fieldName = "_markerEdges";
        var temp = typeof(GhostNodeMap).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(map) as List<GhostMarkerEdge>;
        foreach (var item in temp)
        {
            Handles.DrawLine(item.markerOne.transform.position, item.markerTwo.transform.position);
        }
        */
    }

    void AutoFind()
    {
        //Find pairs
        List<MarkerPair> markerPairs = new List<MarkerPair>();
        var markers = map.GetComponentsInChildren<GhostNodeMarker>();
        foreach (var marker in markers)
        {
            Vector3 pos = marker.transform.position;
            Vector2 posXZ = new Vector2(pos.x, pos.z);
            bool foundAMarker = false;
            foreach (var otherMarker in markers)
            {
                if (marker == otherMarker) continue;

                Vector3 otherPos = otherMarker.transform.position;
                Vector2 otherPosXZ = new Vector2(otherPos.x, otherPos.z);
                if (Mathf.Abs(otherPos.y - pos.y) < yRadius)
                {
                    if (Vector2.Distance(otherPosXZ, posXZ) < xzRadius)
                    {
                        markerPairs.Add(new MarkerPair() { markers = new GhostNodeMarker[2] { marker, otherMarker } });
                        foundAMarker = true;
                    }
                }
            }

            if (!foundAMarker && findClosestIfFailed) //No marker within radius, find two closest.
            {
                float closestDist = float.MaxValue;
                GhostNodeMarker[] closestMarker = new GhostNodeMarker[2];
                foreach (var otherMarker in markers)
                {
                    if (marker == otherMarker) continue;

                    Vector3 otherPos = otherMarker.transform.position;
                    Vector2 otherPosXZ = new Vector2(otherPos.x, otherPos.z);
                    if (Mathf.Abs(otherPos.y - pos.y) < yRadius)
                    {
                        float dist = Vector2.Distance(otherPosXZ, posXZ);
                        if (dist < closestDist)
                        {
                            closestMarker[1] = closestMarker[0];
                            closestMarker[0] = otherMarker;
                            closestDist = dist;
                        }
                    }
                }

                foreach (var item in closestMarker)
                {
                    if (item == null) continue;
                    markerPairs.Add(new MarkerPair() { markers = new GhostNodeMarker[2] { marker, item } });
                }
            }
        }
        //Debug.Log($"1: {markerPairs.Count}");

        //Remove duplicates.
        for (int i = 0; i < markerPairs.Count; i++)
        {
            var pair = markerPairs[i];
            for (int j = 0; j < markerPairs.Count; j++)
            {
                var compPair = markerPairs[j];
                if (pair == compPair) continue;

                int matchCount = 0;
                foreach (var item in pair.markers)
                {
                    foreach (var item2 in compPair.markers)
                    {
                        if (item == item2)
                        {
                            matchCount++;
                        }
                    }
                }
                if (matchCount == 2)
                {
                    markerPairs.RemoveAt(i);
                    i--;
                }
            }
        }
        //Debug.Log($"2: {markerPairs.Count}");

        List<GhostMarkerEdge> edgeList = new List<GhostMarkerEdge>();
        foreach (var pair in markerPairs)
        {
            var edge = new GhostMarkerEdge(null, null); //constructor doesn't work.
            edge.markerOne = pair.markers[0];
            edge.markerTwo = pair.markers[1];
            edgeList.Add(edge);
        }

        Undo.RecordObject(map, "Generate Map");
        EditorUtility.SetDirty(map);

        var fieldName = "_markerEdges";
        var fi = typeof(GhostNodeMap).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        fi.SetValue(map, edgeList);

        PrefabUtility.RecordPrefabInstancePropertyModifications(map);
    }
    public class MarkerPair
    {
        public GhostNodeMarker[] markers;
    }
}