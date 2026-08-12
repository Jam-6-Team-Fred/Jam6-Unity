using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

public static class EditorAnywhereFunctions
{
    static double startTime;
    
    /// <summary> True if this object is selected. </summary>
    public static bool IsSelected(this GameObject go)
    {
#if UNITY_EDITOR
        return Selection.activeGameObject == go;
#else
        return false;
#endif
    }
    /// <summary> False if this object is selected. </summary>
    public static bool IsNotSelected(this GameObject go)
    {
#if UNITY_EDITOR
        return Selection.activeGameObject != go;
#else
        return false;
#endif
    }

    public static void DirtyWork(Object newSO, Object parent)
    {
#if UNITY_EDITOR
        AssetDatabase.AddObjectToAsset(newSO, parent);
        AssetDatabase.SaveAssets(); //Doesn't update properly without

        EditorUtility.SetDirty(parent);
        EditorUtility.SetDirty(newSO);
#endif
    }

    public static void ParentSOInspector(string typeName, Action<string> CreateNewSO)
    {
#if UNITY_EDITOR
        GUILayoutOption guiLO = GUILayout.Height(30f);
        if (GUILayout.Button($"Add New {typeName}", guiLO)) CreateNewSO(typeName);
#endif
    }

    public static void NestedSOInspector(ref string newName, Object focus, Action Delete)
    {
#if UNITY_EDITOR
        GUILayoutOption guiLO = GUILayout.Height(30f);
        using (new GUILayout.HorizontalScope())
        {
            newName = EditorGUILayout.TextField("Name:", newName, guiLO);
            if (GUILayout.Button("Rename", guiLO)) RenameAsset(focus, newName);
        }
        if (GUILayout.Button("Delete", guiLO)) Delete();
#endif
    }

    public static void RenameAsset(Object obj, string newName, bool isChild = true, bool saveAssets = true)
    {
#if UNITY_EDITOR
        obj.name = newName;

        if (!isChild)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            AssetDatabase.RenameAsset(path, newName);
        }

        if (saveAssets) AssetDatabase.SaveAssets();

        EditorUtility.SetDirty(obj);
#endif
    }

    public static Ray GetMouseRay()
    {
#if UNITY_EDITOR
        Event e = Event.current;
        return HandleUtility.GUIPointToWorldRay(e.mousePosition);
#else
        return new Ray();
#endif
    }

    /// <summary> Returns mouse screen position on camera, assuming 2D view. </summary>
    public static Vector2 MouseScreenPosition()
    {
#if UNITY_EDITOR
        Ray mouseRay = GetMouseRay();
        float planeHeight = 0f;
        float dstToPlane = (planeHeight - mouseRay.origin.z) / mouseRay.direction.z;
        Vector3 mousePos = mouseRay.origin + mouseRay.direction * dstToPlane;

        return mousePos;
#else
        return new Vector2();
#endif
    }

    /// <summary> Scene name null when select in project. Scene name same as object name when in prefab editor. </summary>
    public static bool ViewingPrefab(this GameObject obj) => obj.scene.name == null || obj.scene.name == obj.name;

}
