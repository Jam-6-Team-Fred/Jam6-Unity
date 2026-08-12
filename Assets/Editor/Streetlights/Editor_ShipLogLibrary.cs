using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// TODO: rewrite
[CustomEditor(typeof(ShipLogLibrary))]
public class Editor_ShipLogLibrary : Editor
{
    ShipLogLibrary focus;
    void OnEnable()
    {
        focus = target as ShipLogLibrary;
        SceneView.duringSceneGui += OnSceneGUI;
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView obj)
    {
        Vector3 size = new Vector3(110f, 150f);
        void DrawCard(string name, float x, float y, float sizeScale = 2f)
        {
            Vector3 pos = new Vector3(x, y);
            Handles.Label(pos, name);
            Handles.DrawWireCube(pos, size * sizeScale);
        }

        Handles.color = Color.cyan;
        DrawCard("Quantum Moon", -229, 1950);
        DrawCard("Ash Twin Project", 645, 282);
        DrawCard("Orbital Probe Cannon", 1941, 610);
        DrawCard("Vessel", 105, -1016);
        DrawCard("Esker's Camp", 1716, 1222, 1f);
        Handles.color = Color.white;

        var data = focus.entryData;
        for (int i = 0; i < data.Length; i++)
        {
            Vector2 oldPos = data[i].cardPosition;
            Handles.Label(oldPos, data[i].id);
            float scale = IsSubEntry(data[i].id) ? 0.5f : 1f;
            scale *= IsMainCuriousity(data[i].id) ? 2f : 1f;
            Handles.DrawWireCube(oldPos, size * scale);

            Vector2 newPos = Handles.PositionHandle(oldPos, Quaternion.identity);
            newPos = newPos.Snap(0.5f);

            if (oldPos != newPos)
            {
                Undo.RecordObject(focus, "Move Card");
                focus.entryData[i].cardPosition = newPos;
                EditorUtility.SetDirty(focus);
            }
        }
    }
    bool IsSubEntry(string id)
    {
        return id == "DB_SECRET_ROOM";
    }
    bool IsMainCuriousity(string id)
    {
        return id == "DB_NORTHERN_OBSERVATORY";
    }

}
