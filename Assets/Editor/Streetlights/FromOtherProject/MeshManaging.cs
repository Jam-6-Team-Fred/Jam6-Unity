using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO grab newer version
public static class MeshManaging
{
    public static Mesh MergeMeshes(params MeshFilter[] meshes)
    {
        Mesh mergedMesh = new Mesh();

        int length = meshes.Length;
        CombineInstance[] instances = new CombineInstance[length];
        for (int i = 0; i < length; i++)
        {
            MeshFilter mf = meshes[i];
            instances[i].mesh = mf.sharedMesh;
            instances[i].transform = mf.transform.localToWorldMatrix;
        }

        mergedMesh.CombineMeshes(instances, true, true, false);
        mergedMesh.RecalculateTangents();

        return mergedMesh;
    }
    public static Mesh MergeMeshes(params MeshCollider[] meshes)
    {
        Mesh mergedMesh = new Mesh();

        int length = meshes.Length;
        CombineInstance[] instances = new CombineInstance[length];
        for (int i = 0; i < length; i++)
        {
            MeshCollider mc = meshes[i];
            instances[i].mesh = mc.sharedMesh;
            instances[i].transform = mc.transform.localToWorldMatrix;
        }

        mergedMesh.CombineMeshes(instances, true, true, false);
        mergedMesh.RecalculateTangents();

        return mergedMesh;
    }

    public static void MeshToLocal(Mesh mesh, Transform local)
    {
        Vector3[] verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++) verts[i] = local.InverseTransformPoint(verts[i]);

        mesh.SetVertices(verts);
        mesh.RecalculateBounds();
    }
    
}