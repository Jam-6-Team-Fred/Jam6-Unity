using System;
using UnityEngine;

namespace Tessellation
{
	public class Ring
	{
		private const float kPiOverFour = (float)Math.PI / 4f;

		private const float kThreePiOverFour = (float)Math.PI * 3f / 4f;

		private const float kFivePiOverFour = 3.926991f;

		private const float kInvPiOverTwo = 2f / (float)Math.PI;

		protected static Quaternion[] s_faceRotations = new Quaternion[4]
		{
			Quaternion.LookRotation(Vector3.up, Vector3.forward),
			Quaternion.LookRotation(Vector3.up, Vector3.right),
			Quaternion.LookRotation(Vector3.up, Vector3.back),
			Quaternion.LookRotation(Vector3.up, Vector3.left)
		};

		protected static Matrix4x4[] s_faceMatrices = new Matrix4x4[4]
		{
			Matrix4x4.TRS(Vector3.zero, s_faceRotations[0], Vector3.one),
			Matrix4x4.TRS(Vector3.zero, s_faceRotations[1], Vector3.one),
			Matrix4x4.TRS(Vector3.zero, s_faceRotations[2], Vector3.one),
			Matrix4x4.TRS(Vector3.zero, s_faceRotations[3], Vector3.one)
		};

		protected static int s_propID_faceRotationMatrix = Shader.PropertyToID("_FaceRotMatrix");

		protected static int s_propID_faceID = Shader.PropertyToID("_FaceID");

		protected Patch[] patches;

		protected UnityEngine.Plane[] frustumPlanes;

		protected MaterialPropertyBlock[] matPropBlocks;

		public Ring()
		{
			patches = new Patch[4];
			frustumPlanes = new UnityEngine.Plane[6];
			matPropBlocks = new MaterialPropertyBlock[4];
			for (int i = 0; i < 4; i++)
			{
				matPropBlocks[i] = new MaterialPropertyBlock();
			}
			Init(0f);
		}

		~Ring()
		{
			Clear();
		}

		public int GetPatchCount()
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				num += patches[i].GetLeafCount();
			}
			return num;
		}

		public void Init(float patchThickness)
		{
			for (int i = 0; i < 4; i++)
			{
				patches[i] = PatchPool.GetPatch(Patch.RenderMode.Cylindrical, patchThickness);
				matPropBlocks[i].Clear();
			}
			for (int j = 0; j < 4; j++)
			{
				int num = ((j == 0) ? 3 : (j - 1));
				int num2 = ((j != 3) ? (j + 1) : 0);
				patches[j].SetNeighbors(null, patches[num], null, patches[num2]);
				matPropBlocks[j].SetMatrix(s_propID_faceRotationMatrix, s_faceMatrices[j]);
				matPropBlocks[j].SetFloat(s_propID_faceID, j);
			}
		}

		public void Clear()
		{
			for (int i = 0; i < 4; i++)
			{
				if (patches[i] != null)
				{
					patches[i].Clear();
					patches[i] = null;
				}
			}
		}

		public void Tessellate(int maxLevel, Vector3 localPos, float localRadius, float ringThickness = 0f)
		{
			float num = Mathf.Atan2(localPos.x, localPos.z);
			float num2 = Mathf.Abs(new Vector2(localPos.x, localPos.z).magnitude - 1f);
			num2 = Mathf.Max(num2 - ringThickness, 0f);
			int num3;
			float num4;
			if (num <= (float)Math.PI * -3f / 4f)
			{
				num3 = 2;
				num4 = Mathf.InverseLerp(-3.926991f, (float)Math.PI * -3f / 4f, num);
			}
			else if (num <= -(float)Math.PI / 4f)
			{
				num3 = 3;
				num4 = Mathf.InverseLerp((float)Math.PI * -3f / 4f, -(float)Math.PI / 4f, num);
			}
			else if (num <= (float)Math.PI / 4f)
			{
				num3 = 0;
				num4 = Mathf.InverseLerp(-(float)Math.PI / 4f, (float)Math.PI / 4f, num);
			}
			else if (num <= (float)Math.PI * 3f / 4f)
			{
				num3 = 1;
				num4 = Mathf.InverseLerp((float)Math.PI / 4f, (float)Math.PI * 3f / 4f, num);
			}
			else
			{
				num3 = 2;
				num4 = Mathf.InverseLerp((float)Math.PI * 3f / 4f, 3.926991f, num);
			}
			num4 = 1f - num4;
			num4 = num4 * 2f - 1f;
			Vector3 vector = new Vector3(num4, num2, localPos.y);
			Vector3 faceBoundsSize = new Vector3(localRadius * (2f / (float)Math.PI), localRadius, localRadius);
			patches[num3].Tessellate(maxLevel, vector, faceBoundsSize);
			patches[num3].GetNeighbor(1).Tessellate(maxLevel, vector + new Vector3(2f, 0f, 0f), faceBoundsSize);
			patches[num3].GetNeighbor(3).Tessellate(maxLevel, vector + new Vector3(-2f, 0f, 0f), faceBoundsSize);
		}

		public void Draw(Mesh mesh, Matrix4x4 matrix, Material material, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < 4; i++)
			{
				patches[i].Draw(mesh, matrix * s_faceMatrices[i], material, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlocks[i]);
			}
		}

		public void Draw(Mesh mesh, Matrix4x4 matrix, Material[] materials, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < 4; i++)
			{
				patches[i].Draw(mesh, matrix * s_faceMatrices[i], materials, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlocks[i]);
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
				for (int j = 0; j < 4; j++)
				{
					patches[j].Draw(mesh, matrix * s_faceMatrices[j], material, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlocks[j]);
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
				for (int j = 0; j < 4; j++)
				{
					patches[j].Draw(mesh, matrix * s_faceMatrices[j], materials, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlocks[j]);
				}
			}
		}

		public void Draw(MeshGroup meshGroup, Matrix4x4 matrix, Material material, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < 4; i++)
			{
				patches[i].Draw(meshGroup, matrix * s_faceMatrices[i], material, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlocks[i]);
			}
		}

		public void Draw(MeshGroup meshGroup, Matrix4x4 matrix, Material[] materials, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < 4; i++)
			{
				patches[i].Draw(meshGroup, matrix * s_faceMatrices[i], materials, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlocks[i]);
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
				for (int j = 0; j < 4; j++)
				{
					patches[j].Draw(meshGroup, matrix * s_faceMatrices[j], material, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlocks[j]);
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
				for (int j = 0; j < 4; j++)
				{
					patches[j].Draw(meshGroup, matrix * s_faceMatrices[j], materials, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlocks[j]);
				}
			}
		}
	}
}
