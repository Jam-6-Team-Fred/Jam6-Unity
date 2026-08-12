using System;
using UnityEngine;

namespace Tessellation
{
	public class Cube
	{
		protected static int[,] s_neighborIndices = new int[6, 4]
		{
			{ 2, 3, 4, 5 },
			{ 4, 3, 2, 5 },
			{ 3, 0, 5, 1 },
			{ 4, 0, 2, 1 },
			{ 5, 0, 3, 1 },
			{ 2, 0, 4, 1 }
		};

		protected static Quaternion[] s_faceRotations = new Quaternion[6]
		{
			Quaternion.LookRotation(Vector3.forward, Vector3.up),
			Quaternion.LookRotation(Vector3.back, Vector3.down),
			Quaternion.LookRotation(Vector3.right, Vector3.forward),
			Quaternion.LookRotation(Vector3.back, Vector3.right),
			Quaternion.LookRotation(Vector3.left, Vector3.back),
			Quaternion.LookRotation(Vector3.forward, Vector3.left)
		};

		protected static Quaternion[] s_neighborTransforms = new Quaternion[4]
		{
			Quaternion.FromToRotation(Vector3.up, Vector3.forward),
			Quaternion.FromToRotation(Vector3.up, Vector3.right),
			Quaternion.FromToRotation(Vector3.up, Vector3.back),
			Quaternion.FromToRotation(Vector3.up, Vector3.left)
		};

		protected static Vector3[] s_neighborOffsets = new Vector3[4]
		{
			Vector3.back * 2f,
			Vector3.left * 2f,
			Vector3.forward * 2f,
			Vector3.right * 2f
		};

		protected static Matrix4x4[] s_faceMatrices = new Matrix4x4[6]
		{
			Matrix4x4.TRS(Vector3.zero, s_faceRotations[0], Vector3.one),
			Matrix4x4.TRS(Vector3.zero, s_faceRotations[1], Vector3.one),
			Matrix4x4.TRS(Vector3.zero, s_faceRotations[2], Vector3.one),
			Matrix4x4.TRS(Vector3.zero, s_faceRotations[3], Vector3.one),
			Matrix4x4.TRS(Vector3.zero, s_faceRotations[4], Vector3.one),
			Matrix4x4.TRS(Vector3.zero, s_faceRotations[5], Vector3.one)
		};

		protected static int s_propID_faceRotationMatrix = Shader.PropertyToID("_FaceRotMatrix");

		protected static int s_propID_invFaceRotationMatrix = Shader.PropertyToID("_InvFaceRotMatrix");

		protected Patch[] faces;

		protected UnityEngine.Plane[] frustumPlanes;

		protected MaterialPropertyBlock[] matPropBlocks;

		protected virtual Patch.RenderMode patchRenderMode => Patch.RenderMode.Cubic;

		public Cube()
		{
			faces = new Patch[6];
			frustumPlanes = new UnityEngine.Plane[6];
			matPropBlocks = new MaterialPropertyBlock[6];
			for (int i = 0; i < 6; i++)
			{
				matPropBlocks[i] = new MaterialPropertyBlock();
			}
			Init();
		}

		~Cube()
		{
			Clear();
		}

		public int GetPatchCount()
		{
			int num = 0;
			for (int i = 0; i < 6; i++)
			{
				num += faces[i].GetLeafCount();
			}
			return num;
		}

		public void Init()
		{
			for (int i = 0; i < 6; i++)
			{
				faces[i] = PatchPool.GetPatch(patchRenderMode);
				matPropBlocks[i].Clear();
			}
			for (int j = 0; j < 6; j++)
			{
				faces[j].SetNeighbors(faces[s_neighborIndices[j, 0]], faces[s_neighborIndices[j, 1]], faces[s_neighborIndices[j, 2]], faces[s_neighborIndices[j, 3]]);
				Matrix4x4 value = Matrix4x4.TRS(Vector3.zero, s_faceRotations[j], Vector3.one);
				matPropBlocks[j].SetMatrix(s_propID_faceRotationMatrix, value);
				matPropBlocks[j].SetMatrix(s_propID_invFaceRotationMatrix, value.inverse);
			}
		}

		public void Clear()
		{
			for (int i = 0; i < 6; i++)
			{
				if (faces[i] != null)
				{
					faces[i].Clear();
					faces[i] = null;
				}
			}
		}

		public virtual void Tessellate(int maxLevel, Vector3 localPos, float localRadius)
		{
			throw new NotImplementedException();
		}

		public void Draw(Mesh mesh, Matrix4x4 matrix, Material material, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < 6; i++)
			{
				faces[i].Draw(mesh, matrix * s_faceMatrices[i], material, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlocks[i]);
			}
		}

		public void Draw(Mesh mesh, Matrix4x4 matrix, Material[] materials, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < 6; i++)
			{
				faces[i].Draw(mesh, matrix * s_faceMatrices[i], materials, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlocks[i]);
			}
		}

		public void Draw(Mesh mesh, Matrix4x4 matrix, Material material, Camera[] cameras, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			for (int i = 0; i < cameras.Length; i++)
			{
				if (cameras[i] != null)
				{
					GeometryUtility.CalculateFrustumPlanes(cameras[i], frustumPlanes);
				}
				for (int j = 0; j < 6; j++)
				{
					faces[j].Draw(mesh, matrix * s_faceMatrices[j], material, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlocks[j]);
				}
			}
		}

		public void Draw(Mesh mesh, Matrix4x4 matrix, Material[] materials, Camera[] cameras, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			for (int i = 0; i < cameras.Length; i++)
			{
				if (cameras[i] != null)
				{
					GeometryUtility.CalculateFrustumPlanes(cameras[i], frustumPlanes);
				}
				for (int j = 0; j < 6; j++)
				{
					faces[j].Draw(mesh, matrix * s_faceMatrices[j], materials, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlocks[j]);
				}
			}
		}

		public void Draw(MeshGroup meshGroup, Matrix4x4 matrix, Material material, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < 6; i++)
			{
				faces[i].Draw(meshGroup, matrix * s_faceMatrices[i], material, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlocks[i]);
			}
		}

		public void Draw(MeshGroup meshGroup, Matrix4x4 matrix, Material[] materials, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < 6; i++)
			{
				faces[i].Draw(meshGroup, matrix * s_faceMatrices[i], materials, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlocks[i]);
			}
		}

		public void Draw(MeshGroup meshGroup, Matrix4x4 matrix, Material material, Camera[] cameras, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			for (int i = 0; i < cameras.Length; i++)
			{
				if (cameras[i] != null)
				{
					GeometryUtility.CalculateFrustumPlanes(cameras[i], frustumPlanes);
				}
				for (int j = 0; j < 6; j++)
				{
					faces[j].Draw(meshGroup, matrix * s_faceMatrices[j], material, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlocks[j]);
				}
			}
		}

		public void Draw(MeshGroup meshGroup, Matrix4x4 matrix, Material[] materials, Camera[] cameras, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			for (int i = 0; i < cameras.Length; i++)
			{
				if (cameras[i] != null)
				{
					GeometryUtility.CalculateFrustumPlanes(cameras[i], frustumPlanes);
				}
				for (int j = 0; j < 6; j++)
				{
					faces[j].Draw(meshGroup, matrix * s_faceMatrices[j], materials, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlocks[j]);
				}
			}
		}
	}
}
