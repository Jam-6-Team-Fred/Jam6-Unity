// TODO: what are half of these variables? ask streetlights
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NomaiTextLine))]
public class NomaiTextEditor : Editor
{
    NomaiTextLine focus;
    const float TAU = Mathf.PI * 2f;
    const string UNDO = "Update Text Mesh";
    void OnEnable()
    {
        focus = target as NomaiTextLine;
        SceneView.duringSceneGui += OnSceneGUI;
        //Undo.undoRedoPerformed += GenerateMesh;
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        //Undo.undoRedoPerformed -= GenerateMesh;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            GenerateMesh();
        }
    }

    void OnSceneGUI(SceneView obj)
    {
        Handles.matrix = focus.transform.localToWorldMatrix;

        //Center handle
        Vector3 newCenter = Handles.PositionHandle(focus.Center, Quaternion.identity);
        if (newCenter != focus.Center)
        {
            Undo.RecordObject(focus, UNDO);
            focus.Center = newCenter;
            GenerateMesh();
        }

        //Lines - remove?
        for (int i = 0; i < focus.Points.Length - 1; i++)
        {
            Handles.DrawLine(focus.Points[i], focus.Points[i + 1]);
        }

        Handles.DrawWireDisc(focus.Center, Vector3.forward, focus.InnerRadius);
        Handles.DrawWireDisc(focus.Center, Vector3.forward, focus.Radius);

        Handles.matrix = Matrix4x4.identity;
    }



    void GenerateMesh()
    {
        //Get points

        //if (focus.Points.Length <= 2) return;
        if (focus.Points.Length <= 8)
        {
            Debug.LogWarning("Not enough points.");
            return;
        }

        Vector3 dirToCentre = (GetPosAtT(0f) - GetPosAtT(1f)).normalized;

        int x = 10;
        int x2 = x / 2;

        float div = 1f / (focus.Points.Length);

        void AddPoint(int index, int tI)
        {
            float t = tI * div;
            focus.Points[index] = GetPosAtT(t);

            t = 1 - t;
            t = Mathf.Pow(t, 0.5f);
            focus.Points[index] += dirToCentre * t * focus.centerOffsetMag;
        }

        for (int i = 0; i < focus.Points.Length; i++) AddPoint(i, i);

        /*
        div *= 0.5f;
        for (int i = 0; i < x; i++) AddPoint(i, i);
        div = 1f / (focus.Points.Length - x2);
        for (int i = x; i < focus.Points.Length; i++) AddPoint(i, i - x2);
        #1#

        //Update lengths
        focus.TotalLength = 0f;
        focus.Lengths = new float[focus.Points.Length - 1];
        for (int i = 0; i < focus.Points.Length - 1; i++)
        {
            float length = Vector3.Distance(focus.Points[i], focus.Points[i + 1]);
            focus.Lengths[i] = length;

            focus.TotalLength += length;
        }

        //Create mesh
        var md = FlatLineAdjustedWidth(focus.Points);
        Vector2[] textureUVs = new Vector2[md.Verts.Count];
        Vector2[] revealUVs = new Vector2[md.Verts.Count];
        float lScaling = 1.75f;
        float l = 0f;

        for (int i = 0; i < focus.Lengths.Length + 1; i++)
        {
            int uv = i * 2;
            float t = 1f - l / focus.TotalLength;    //0-1

            float l2 = l * Mathf.Lerp(lScaling, 1.25f * lScaling, Mathf.Clamp01(t));

            textureUVs[uv] = new Vector2(l2, 0f);
            textureUVs[uv + 1] = new Vector2(l2, 1f);

            revealUVs[uv] = new Vector2(t, 1f);
            revealUVs[uv + 1] = new Vector2(t, 0f);

            if (i < focus.Lengths.Length) l += focus.Lengths[i];
        }

        MeshFilter mf = focus.GetComponent<MeshFilter>();
        Undo.RecordObject(mf, UNDO);

        Mesh mesh = new Mesh();
        if (mf.sharedMesh != null)  //Use old so don't have to regenerate asset again.
        {
            mesh = mf.sharedMesh;
            mesh.Clear();
        }
        else
        {
            string name = $"Arc-{System.Guid.NewGuid()}.asset"; //Automatically make asset.
            string path = $"Assets/TextArcMeshes/{name}";
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets(); //Doesn't update properly without
            EditorUtility.SetDirty(mesh);
        }

        mesh.SetVertices(md.Verts);
        mesh.SetTriangles(md.Tris, 0);
        mesh.SetUVs(0, textureUVs);
        mesh.SetUVs(1, revealUVs);
        mesh.RecalculateNormals();

        mf.sharedMesh = mesh;
    }
    Vector3 GetPosAtT(float t)
    {
        t = t.Remap(0f, 1f, 0.1f, 1f);  //Remap to avoid issues near 0.
        t = Mathf.Pow(t, 0.5f); //Reduce bunching at end.

        float angle = TAU * t * focus.Turns;
        angle += focus.AngleOffset;
        if (focus.FlipDirection) angle = -angle;

        float keepInPlace = focus.Turns * TAU;
        if (focus.FlipDirection) angle += keepInPlace;
        else angle -= keepInPlace;

        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        t = Mathf.Pow(t, focus.CenterPullPow);
        float r = Mathf.Lerp(focus.InnerRadius, focus.Radius, t);
        Vector3 pos = focus.Center + (Vector3)dir * r;

        return pos;
    }

    MeshData FlatLineAdjustedWidth(params Vector3[] points)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        if (points.Length < 2) return new MeshData(verts, tris);

        Vector3 normal = Vector3.forward;
        float div = 1f / points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            //-------- Get OP and SideVec --------//
            Vector3 pos = points[i];
            Vector3 forwardVec = Vector2.zero;

            float t = i * div;
            //t = Mathf.Pow(t, 0.2f);
            float width = Mathf.Lerp(focus.InnerWidth, focus.OuterWidth, t);
            float sideDist = width * 0.5f;

            bool startPoint = i <= 0;
            bool lastPoint = i + 1 >= points.Length;

            //-------- If adjust width to be more even, and not start or end point --------//
            if (startPoint) forwardVec = (points[i + 1] - pos).normalized;
            else if (lastPoint) forwardVec = (pos - points[i - 1]).normalized;
            else
            {
                //-------- Get next and previous --------//
                Vector3 previous = points[i - 1];
                Vector3 next = points[i + 1];

                //-------- Get Adjusted Width --------//
                float widthAdjust = GetSideWidth(previous, pos, next, normal);
                sideDist *= widthAdjust;

                Vector2 prevVec = (pos - previous).normalized;
                Vector2 nextVec = (next - pos).normalized;
                forwardVec = ((prevVec + nextVec) * 0.5f).normalized;
            }

            //-------- Get Side points --------//
            Vector3 sideVec = Vector3.Cross(forwardVec, normal).normalized * sideDist;
            Vector3 sidePointA = pos + sideVec;
            Vector3 sidePointB = pos - sideVec;

            //-------- If not last point --------//
            if (!lastPoint)
            {
                //-------- Root --------//
                int r = verts.Count;

                //-------- Add tris --------//
                tris.Add(r);
                tris.Add(r + 2);
                tris.Add(r + 1);

                tris.Add(r + 3);
                tris.Add(r + 1);
                tris.Add(r + 2);
            }

            //-------- Add points --------//
            verts.Add(sidePointA);
            verts.Add(sidePointB);
        }

        return new MeshData(verts, tris);
    }



    /// <summary>
    /// <para> Assumes the points on either side of the line are perpendicular to the line. </para>
    /// <para> Gets the width that makes the perpendicular width appears constant. </para>
    /// <para> For instance, line of width 1, 90 degree turn requires diagonal length of root 2 (1.4142...). </para>
    /// </summary>
    public static float GetSideWidth(Vector3 lastPoint, Vector3 point, Vector3 nextPoint, Vector3 normal)
    {
        Vector3 dirFromLastPoint = (point - lastPoint).normalized;
        Vector3 dirFromNextPoint = (point - nextPoint).normalized;

        //-------- Get perpendicular vector --------//
        Vector3 average = ((dirFromLastPoint - dirFromNextPoint) * 0.5f).normalized;
        Vector3 perpendicular = Vector3.Cross(average, normal);

        //-------- Get Cross Products --------//
        Vector3 perpendicularLast = Vector3.Cross(dirFromLastPoint, perpendicular);
        Vector3 perpendicularNext = Vector3.Cross(dirFromNextPoint, perpendicular);

        //-------- Get Dot Product --------//
        Vector3 half = (perpendicularLast + perpendicularNext).normalized;
        if (half.sqrMagnitude < 0.3f) half = average; //In case is 0, fall back to average

        float dot = Vector3.Dot(dirFromLastPoint, half);
        dot = Mathf.Abs(dot);

        //-------- Avoid Divide by 0 --------//
        float min = 0.001f;
        if (dot < min) dot = min;

        return 1f / dot;
    }
}
public class MeshData
{
    protected List<Vector3> verts;
    protected List<int> tris;
    public List<Vector3> Verts => verts;
    public List<int> Tris => tris;

    public MeshData()
    {
        verts = new List<Vector3>();
        tris = new List<int>();
    }
    public MeshData(List<Vector3> verts, List<int> tris)
    {
        this.verts = verts;
        this.tris = tris;
    }
}*/