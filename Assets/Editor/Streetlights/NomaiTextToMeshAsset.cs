using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class NomaiTextToMeshAsset
{
    [MenuItem("Streetlights/Text Arcs to Asset under Selected", validate = true)]
    public static bool SetUpV() => Selection.activeGameObject != null;


    [MenuItem("Streetlights/Text Arcs to Asset under Selected")]
    public static void SetUp()
    {
        //Only if "Arc" in name.

        Transform[] children = Selection.activeGameObject.GetComponentsInChildren<Transform>();

        foreach (var tf in children)
        {
            if (tf.name.Contains("Arc ")) CreateMeshAsset(tf);
        }
    }
    static void CreateMeshAsset(Transform tf)
    {
        var mf = tf.GetComponent<MeshFilter>();
        if (mf == null) return;

        string name = $"Arc-{System.Guid.NewGuid()}.asset";
        string path = $"Assets/TextArcMeshes/{name}";

        Mesh mesh = mf.sharedMesh;

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets(); //Doesn't update properly without
        EditorUtility.SetDirty(mesh);
    }


}
