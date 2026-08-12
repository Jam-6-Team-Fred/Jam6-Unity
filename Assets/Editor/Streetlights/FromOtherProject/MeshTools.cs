using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Protostellar.EditorStuff
{
    public static class MeshTools
    {
        static GameObject selected => Selection.activeGameObject;

        [MenuItem("Streetlights/Mesh/Merge Under Selected", validate = true)]
        public static bool V_MergeMeshesUnderSelected() => ValidatePresets.SelectedHasChildren;

        [MenuItem("Streetlights/Mesh/Merge Under Selected")]
        public static void MergeMeshesUnderSelected()
        {
            MeshFilter[] meshFilters = selected.GetComponentsInChildren<MeshFilter>(false);

            Mesh mesh = MeshManaging.MergeMeshes(meshFilters);
            MeshManaging.MeshToLocal(mesh, selected.transform);

            GameObject go = new GameObject("Merged Mesh");
            go.transform.parent = selected.transform.parent;
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = mesh;
            mr.sharedMaterial = selected.GetComponentInChildren<MeshRenderer>().sharedMaterial;

            Undo.RegisterCreatedObjectUndo(go, "Merge Mesh");
        }


        //--------------------------------------------- Selected to Asset ---------------------------------------------//
        [MenuItem("Streetlights/Mesh/All Under Selected To Asset", validate = true)]
        public static bool V_AllUnderSelectedMeshToAsset() => ValidatePresets.ObjectSelected;
        [MenuItem("Streetlights/Mesh/All Under Selected To Asset")]
        public static void AllUnderSelectedMeshToAsset()
        {
            var mfs = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>(false);
            string parentName = Selection.activeGameObject.name;

            foreach (var mf in mfs)
            {
                var mesh = mf.sharedMesh;
                string folder = $"Assets/MESHES/{parentName}";

                CreateAsset(mesh, folder, mf.gameObject.name);
            }

            Debug.Log($"Saved to MESHES/{parentName}");
        }
        static void CreateAsset(Mesh mesh, string folder, string name)
        {
            if (AssetDatabase.Contains(mesh)) return; //Already asset
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);   //Perhaps make function?

            string path = $"{folder}/{name}.asset";
            AssetDatabase.CreateAsset(mesh, path);

            AssetDatabase.SaveAssets(); //Doesn't update properly without
            EditorUtility.SetDirty(mesh);
        }

        [MenuItem("Streetlights/Mesh/Merge All Under Selected To One Asset")]
        public static bool V_MERGEAllUnderSelectedMeshToONEAsset() => ValidatePresets.ObjectSelected;
        [MenuItem("Streetlights/Mesh/Merge All Under Selected To One Asset")]
        public static void MERGEAllUnderSelectedMeshToONEAsset()
        {
            //---------------- Mesh Filters ----------------//
            MeshFilter[] meshFilters = selected.GetComponentsInChildren<MeshFilter>(false);

            Mesh mesh = MeshManaging.MergeMeshes(meshFilters);
            MeshManaging.MeshToLocal(mesh, selected.transform);

            string folder = "Assets/MESHES/Merged";
            CreateAsset(mesh, folder, $"{selected.name}");

            //---------------- Mesh Colliders ----------------//
            MeshCollider[] meshColliders = selected.GetComponentsInChildren<MeshCollider>(false);
            if (meshColliders.Length == 0) return;

            mesh = MeshManaging.MergeMeshes(meshColliders);
            MeshManaging.MeshToLocal(mesh, selected.transform);

            folder = "Assets/MESHES/Merged";
            CreateAsset(mesh, folder, $"{selected.name}_coll");
        }


        [MenuItem("Streetlights/Mesh/Modify/Recalculate Normals", validate = true)]
        public static bool V_RecalculateNormals() => ValidatePresets.ObjectSelected;
        [MenuItem("Streetlights/Mesh/Modify/Recalculate Normals")]
        public static void RecalculateNormals()
        {
            if (Selection.activeGameObject.TryGetComponent(out MeshFilter mf))
            {
                EditorUtility.SetDirty(mf.sharedMesh);

                mf.sharedMesh.RecalculateNormals();
                mf.sharedMesh.RecalculateTangents();
            }
        }

        [MenuItem("Streetlights/Mesh/Modify/Set To Planar UVs", validate = true)]
        public static bool V_PlanarUVs() => ValidatePresets.ObjectSelected;
        [MenuItem("Streetlights/Mesh/Modify/Set To Planar UVs")]
        public static void PlanarUVs()
        {
            if (!Selection.activeGameObject.TryGetComponent(out MeshFilter mf)) return;

            Mesh mesh = mf.sharedMesh;
            EditorUtility.SetDirty(mesh);

            Vector3[] verts = mesh.vertices;
            Vector2[] uvs = PlanarUVs(mesh, verts);
            mesh.SetUVs(0, uvs);
        }
        [MenuItem("Streetlights/Mesh/Modify/Set To Messy UVs", validate = true)]
        public static bool V_MessyUVs() => ValidatePresets.ObjectSelected;
        [MenuItem("Streetlights/Mesh/Modify/Set To Messy UVs")]
        public static void MessyUVs()
        {
            if (!Selection.activeGameObject.TryGetComponent(out MeshFilter mf)) return;

            Mesh mesh = mf.sharedMesh;
            EditorUtility.SetDirty(mesh);

            Vector3[] verts = mesh.vertices;
            Vector2[] uvs = PlanarUVs(mesh, verts);
            for (int i = 0; i < verts.Length; i++) uvs[i] = new Vector2(uvs[i].x % 1f, uvs[i].y % 1f);

            mesh.SetUVs(0, uvs);
        }
        static Vector2[] PlanarUVs(Mesh mesh, Vector3[] verts)
        {
            var normals = mesh.normals;
            float bound = 0.5f;

            Vector2[] uvs = new Vector2[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 n = normals[i];

                float upDot = Vector3.Dot(n, Vector3.up).Abs();
                float rightDot = Vector3.Dot(n, Vector3.right).Abs();

                if (upDot > bound) uvs[i] = new Vector2(verts[i].x, verts[i].z);
                else if (rightDot > bound) uvs[i] = new Vector2(verts[i].z, verts[i].y);
                else uvs[i] = new Vector2(verts[i].x, verts[i].y);

                uvs[i] *= 0.25f;
            }

            return uvs;
        }



    }
}