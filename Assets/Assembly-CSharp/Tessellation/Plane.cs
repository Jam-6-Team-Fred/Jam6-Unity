using UnityEngine;

namespace Tessellation
{
	public class Plane
	{
		protected static int s_propID_planeThickness = Shader.PropertyToID("_PlaneThickness");

		protected int baseTileCountX;

		protected int baseTileCountY;

		protected float thickness;

		protected Patch[,] patches;

		protected UnityEngine.Plane[] frustumPlanes;

		protected MaterialPropertyBlock matPropBlock;

		public Plane(int baseTileCountX, int baseTileCountY, float thickness)
		{
			this.baseTileCountX = baseTileCountX;
			this.baseTileCountY = baseTileCountY;
			this.thickness = thickness;
			patches = new Patch[baseTileCountX, baseTileCountY];
			frustumPlanes = new UnityEngine.Plane[6];
			matPropBlock = new MaterialPropertyBlock();
			Init();
		}

		~Plane()
		{
			Clear();
		}

		public int GetBaseTileCountX()
		{
			return baseTileCountX;
		}

		public int GetBaseTileCountY()
		{
			return baseTileCountY;
		}

		public float GetThickness()
		{
			return thickness;
		}

		public void SetBaseTileCount(int newBaseTileCountX, int newBaseTileCountY)
		{
			if (baseTileCountX != newBaseTileCountX || baseTileCountY != newBaseTileCountY)
			{
				Clear();
				baseTileCountX = newBaseTileCountX;
				baseTileCountY = newBaseTileCountY;
				patches = new Patch[baseTileCountX, baseTileCountY];
				Init();
			}
		}

		public void SetThickness(float newThickness)
		{
			thickness = Mathf.Max(newThickness, 0f);
			if (matPropBlock != null)
			{
				matPropBlock.SetFloat(s_propID_planeThickness, thickness);
			}
		}

		public int GetPatchCount()
		{
			int num = 0;
			for (int i = 0; i < baseTileCountX; i++)
			{
				for (int j = 0; j < baseTileCountY; j++)
				{
					num += patches[i, j].GetLeafCount();
				}
			}
			return num;
		}

		public void Init()
		{
			Vector2 size = new Vector2(1f / (float)baseTileCountX * 2f, 1f / (float)baseTileCountY * 2f);
			for (int i = 0; i < baseTileCountX; i++)
			{
				for (int j = 0; j < baseTileCountY; j++)
				{
					Vector2 center = new Vector2(((float)i - (float)(baseTileCountX - 1) * 0.5f) * size.x, ((float)j - (float)(baseTileCountY - 1) * 0.5f) * size.y);
					patches[i, j] = PatchPool.GetPatch(Patch.RenderMode.Planar, thickness);
					patches[i, j].SetLocalBounds(center, size);
				}
			}
			for (int k = 0; k < baseTileCountX; k++)
			{
				for (int l = 0; l < baseTileCountY; l++)
				{
					Patch upper = ((l < baseTileCountY - 1) ? patches[k, l + 1] : null);
					Patch right = ((k < baseTileCountX - 1) ? patches[k + 1, l] : null);
					Patch lower = ((l > 0) ? patches[k, l - 1] : null);
					Patch left = ((k > 0) ? patches[k - 1, l] : null);
					patches[k, l].SetNeighbors(upper, right, lower, left);
				}
			}
			matPropBlock.Clear();
			matPropBlock.SetFloat(s_propID_planeThickness, thickness);
		}

		public void Clear()
		{
			for (int i = 0; i < baseTileCountX; i++)
			{
				for (int j = 0; j < baseTileCountY; j++)
				{
					if (patches[i, j] != null)
					{
						patches[i, j].Clear();
						patches[i, j] = null;
					}
				}
			}
		}

		public void Tessellate(int maxLevel, Vector3 localPos, Vector3 localRadius)
		{
			for (int i = 0; i < baseTileCountX; i++)
			{
				for (int j = 0; j < baseTileCountY; j++)
				{
					patches[i, j].Tessellate(maxLevel, localPos, localRadius);
				}
			}
		}

		public void Draw(Mesh mesh, Matrix4x4 matrix, Material material, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < baseTileCountX; i++)
			{
				for (int j = 0; j < baseTileCountY; j++)
				{
					patches[i, j].Draw(mesh, matrix, material, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlock);
				}
			}
		}

		public void Draw(Mesh mesh, Matrix4x4 matrix, Material[] materials, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < baseTileCountX; i++)
			{
				for (int j = 0; j < baseTileCountY; j++)
				{
					patches[i, j].Draw(mesh, matrix, materials, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlock);
				}
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
				for (int j = 0; j < baseTileCountX; j++)
				{
					for (int k = 0; k < baseTileCountY; k++)
					{
						patches[j, k].Draw(mesh, matrix, material, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlock);
					}
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
				for (int j = 0; j < baseTileCountX; j++)
				{
					for (int k = 0; k < baseTileCountY; k++)
					{
						patches[j, k].Draw(mesh, matrix, materials, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlock);
					}
				}
			}
		}

		public void Draw(MeshGroup meshGroup, Matrix4x4 matrix, Material material, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < baseTileCountX; i++)
			{
				for (int j = 0; j < baseTileCountY; j++)
				{
					patches[i, j].Draw(meshGroup, matrix, material, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlock);
				}
			}
		}

		public void Draw(MeshGroup meshGroup, Matrix4x4 matrix, Material[] materials, Camera camera = null, Patch.CullingMode cullingMode = Patch.CullingMode.Normal, int layer = 0)
		{
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, frustumPlanes);
			}
			for (int i = 0; i < baseTileCountX; i++)
			{
				for (int j = 0; j < baseTileCountY; j++)
				{
					patches[i, j].Draw(meshGroup, matrix, materials, camera, camera ? frustumPlanes : null, cullingMode, layer, matPropBlock);
				}
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
				for (int j = 0; j < baseTileCountX; j++)
				{
					for (int k = 0; k < baseTileCountY; k++)
					{
						patches[j, k].Draw(meshGroup, matrix, material, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlock);
					}
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
				for (int j = 0; j < baseTileCountX; j++)
				{
					for (int k = 0; k < baseTileCountY; k++)
					{
						patches[j, k].Draw(meshGroup, matrix, materials, cameras[i], cameras[i] ? frustumPlanes : null, cullingMode, layer, matPropBlock);
					}
				}
			}
		}

		public void DrawGizmos()
		{
			if (patches == null)
			{
				return;
			}
			for (int i = 0; i < baseTileCountX; i++)
			{
				for (int j = 0; j < baseTileCountY; j++)
				{
					if (patches[i, j] != null)
					{
						patches[i, j].DrawGizmos();
					}
				}
			}
		}
	}
}
