using System.Collections.Generic;
using UnityEngine;

public class ProxyShadowCasterSuperGroup : MonoBehaviour
{
	public class ShadowCasterData
	{
		public ProxyShadowCaster proxyShadowCaster;

		public bool dynamic;

		public Mesh cachedMesh;

		public int cachedSubMeshCount;

		public Matrix4x4 cachedLocalMatrix;

		public Matrix4x4 cachedGlobalMatrix;

		public void Set(ProxyShadowCaster psc)
		{
			proxyShadowCaster = psc;
			dynamic = psc.dynamic;
			cachedMesh = psc.mesh;
			cachedSubMeshCount = ((cachedMesh != null) ? cachedMesh.subMeshCount : (-1));
			cachedLocalMatrix = psc.localToGroupMatrix;
			cachedGlobalMatrix = Matrix4x4.identity;
		}

		public void Set(ShadowCasterData scd)
		{
			proxyShadowCaster = scd.proxyShadowCaster;
			dynamic = scd.dynamic;
			cachedMesh = scd.cachedMesh;
			cachedSubMeshCount = scd.cachedSubMeshCount;
			cachedLocalMatrix = scd.cachedLocalMatrix;
			cachedGlobalMatrix = scd.cachedGlobalMatrix;
		}
	}

	public class CascadeGroup
	{
		public List<ShadowCasterData> shadowCasters = new List<ShadowCasterData>(128);

		public ProxyShadowCasterSuperGroup superGroup;

		private int _numDynamicShadowCasters;

		private int _lastUpdatedFrame;

		public bool hasDynamicShadowCasters => _numDynamicShadowCasters > 0;

		public void AddShadowCaster(ProxyShadowCaster shadowCaster)
		{
			ShadowCasterData shadowCasterData = new ShadowCasterData();
			shadowCasterData.Set(shadowCaster);
			if (shadowCaster.earlyDraw)
			{
				shadowCasters.Insert(0, shadowCasterData);
			}
			else
			{
				shadowCasters.Add(shadowCasterData);
			}
			if (shadowCaster.dynamic)
			{
				_numDynamicShadowCasters++;
			}
		}

		public void RemoveShadowCaster(ProxyShadowCaster shadowCaster)
		{
			for (int i = 0; i < shadowCasters.Count; i++)
			{
				if (shadowCasters[i].proxyShadowCaster == shadowCaster)
				{
					if (shadowCaster.earlyDraw)
					{
						shadowCasters.RemoveAt(i);
					}
					else
					{
						shadowCasters.QuickRemoveAt(i);
					}
					if (shadowCaster.dynamic)
					{
						_numDynamicShadowCasters--;
					}
					break;
				}
			}
		}

		public void UpdateDynamicFlag(ProxyShadowCaster shadowCaster)
		{
			for (int i = 0; i < shadowCasters.Count; i++)
			{
				if (shadowCasters[i].proxyShadowCaster == shadowCaster)
				{
					if (!shadowCasters[i].dynamic && shadowCaster.dynamic)
					{
						_numDynamicShadowCasters++;
					}
					else if (shadowCasters[i].dynamic && !shadowCaster.dynamic)
					{
						_numDynamicShadowCasters--;
					}
					shadowCasters[i].dynamic = shadowCaster.dynamic;
					if (!shadowCaster.dynamic)
					{
						shadowCasters[i].cachedLocalMatrix = shadowCaster.localToGroupMatrix;
					}
					break;
				}
			}
		}

		public void PreProcessRenderers()
		{
			if (_lastUpdatedFrame >= Time.frameCount)
			{
				return;
			}
			Matrix4x4 localToWorldMatrix = superGroup.transform.localToWorldMatrix;
			for (int i = 0; i < shadowCasters.Count; i++)
			{
				if (shadowCasters[i].dynamic)
				{
					shadowCasters[i].cachedGlobalMatrix = shadowCasters[i].proxyShadowCaster.localToWorldMatrix;
					continue;
				}
				ShadowCasterData shadowCasterData = shadowCasters[i];
				shadowCasterData.cachedGlobalMatrix.m00 = localToWorldMatrix.m00 * shadowCasterData.cachedLocalMatrix.m00 + localToWorldMatrix.m01 * shadowCasterData.cachedLocalMatrix.m10 + localToWorldMatrix.m02 * shadowCasterData.cachedLocalMatrix.m20 + localToWorldMatrix.m03 * shadowCasterData.cachedLocalMatrix.m30;
				shadowCasterData.cachedGlobalMatrix.m01 = localToWorldMatrix.m00 * shadowCasterData.cachedLocalMatrix.m01 + localToWorldMatrix.m01 * shadowCasterData.cachedLocalMatrix.m11 + localToWorldMatrix.m02 * shadowCasterData.cachedLocalMatrix.m21 + localToWorldMatrix.m03 * shadowCasterData.cachedLocalMatrix.m31;
				shadowCasterData.cachedGlobalMatrix.m02 = localToWorldMatrix.m00 * shadowCasterData.cachedLocalMatrix.m02 + localToWorldMatrix.m01 * shadowCasterData.cachedLocalMatrix.m12 + localToWorldMatrix.m02 * shadowCasterData.cachedLocalMatrix.m22 + localToWorldMatrix.m03 * shadowCasterData.cachedLocalMatrix.m32;
				shadowCasterData.cachedGlobalMatrix.m03 = localToWorldMatrix.m00 * shadowCasterData.cachedLocalMatrix.m03 + localToWorldMatrix.m01 * shadowCasterData.cachedLocalMatrix.m13 + localToWorldMatrix.m02 * shadowCasterData.cachedLocalMatrix.m23 + localToWorldMatrix.m03 * shadowCasterData.cachedLocalMatrix.m33;
				shadowCasterData.cachedGlobalMatrix.m10 = localToWorldMatrix.m10 * shadowCasterData.cachedLocalMatrix.m00 + localToWorldMatrix.m11 * shadowCasterData.cachedLocalMatrix.m10 + localToWorldMatrix.m12 * shadowCasterData.cachedLocalMatrix.m20 + localToWorldMatrix.m13 * shadowCasterData.cachedLocalMatrix.m30;
				shadowCasterData.cachedGlobalMatrix.m11 = localToWorldMatrix.m10 * shadowCasterData.cachedLocalMatrix.m01 + localToWorldMatrix.m11 * shadowCasterData.cachedLocalMatrix.m11 + localToWorldMatrix.m12 * shadowCasterData.cachedLocalMatrix.m21 + localToWorldMatrix.m13 * shadowCasterData.cachedLocalMatrix.m31;
				shadowCasterData.cachedGlobalMatrix.m12 = localToWorldMatrix.m10 * shadowCasterData.cachedLocalMatrix.m02 + localToWorldMatrix.m11 * shadowCasterData.cachedLocalMatrix.m12 + localToWorldMatrix.m12 * shadowCasterData.cachedLocalMatrix.m22 + localToWorldMatrix.m13 * shadowCasterData.cachedLocalMatrix.m32;
				shadowCasterData.cachedGlobalMatrix.m13 = localToWorldMatrix.m10 * shadowCasterData.cachedLocalMatrix.m03 + localToWorldMatrix.m11 * shadowCasterData.cachedLocalMatrix.m13 + localToWorldMatrix.m12 * shadowCasterData.cachedLocalMatrix.m23 + localToWorldMatrix.m13 * shadowCasterData.cachedLocalMatrix.m33;
				shadowCasterData.cachedGlobalMatrix.m20 = localToWorldMatrix.m20 * shadowCasterData.cachedLocalMatrix.m00 + localToWorldMatrix.m21 * shadowCasterData.cachedLocalMatrix.m10 + localToWorldMatrix.m22 * shadowCasterData.cachedLocalMatrix.m20 + localToWorldMatrix.m23 * shadowCasterData.cachedLocalMatrix.m30;
				shadowCasterData.cachedGlobalMatrix.m21 = localToWorldMatrix.m20 * shadowCasterData.cachedLocalMatrix.m01 + localToWorldMatrix.m21 * shadowCasterData.cachedLocalMatrix.m11 + localToWorldMatrix.m22 * shadowCasterData.cachedLocalMatrix.m21 + localToWorldMatrix.m23 * shadowCasterData.cachedLocalMatrix.m31;
				shadowCasterData.cachedGlobalMatrix.m22 = localToWorldMatrix.m20 * shadowCasterData.cachedLocalMatrix.m02 + localToWorldMatrix.m21 * shadowCasterData.cachedLocalMatrix.m12 + localToWorldMatrix.m22 * shadowCasterData.cachedLocalMatrix.m22 + localToWorldMatrix.m23 * shadowCasterData.cachedLocalMatrix.m32;
				shadowCasterData.cachedGlobalMatrix.m23 = localToWorldMatrix.m20 * shadowCasterData.cachedLocalMatrix.m03 + localToWorldMatrix.m21 * shadowCasterData.cachedLocalMatrix.m13 + localToWorldMatrix.m22 * shadowCasterData.cachedLocalMatrix.m23 + localToWorldMatrix.m23 * shadowCasterData.cachedLocalMatrix.m33;
			}
			_lastUpdatedFrame = Time.frameCount;
		}
	}

	private static List<ProxyShadowCasterSuperGroup> s_groups = new List<ProxyShadowCasterSuperGroup>(32);

	[SerializeField]
	private SphereBounds _bounds = new SphereBounds(Vector3.zero, 500f);

	private CascadeGroup _nearCascadeGroup = new CascadeGroup();

	private CascadeGroup _farCascadeGroup = new CascadeGroup();

	private void Awake()
	{
		_nearCascadeGroup.superGroup = this;
		_farCascadeGroup.superGroup = this;
		s_groups.Add(this);
	}

	private void OnDestroy()
	{
		s_groups.QuickRemove(this);
	}

	public static List<ProxyShadowCasterSuperGroup> GetGroupList()
	{
		return s_groups;
	}

	public CascadeGroup GetNearCascade()
	{
		return _nearCascadeGroup;
	}

	public CascadeGroup GetFarCascade()
	{
		return _farCascadeGroup;
	}

	public void AddShadowCaster(ProxyShadowCaster shadowCaster)
	{
		if (shadowCaster.near)
		{
			_nearCascadeGroup.AddShadowCaster(shadowCaster);
		}
		if (shadowCaster.far)
		{
			_farCascadeGroup.AddShadowCaster(shadowCaster);
		}
	}

	public void RemoveShadowCaster(ProxyShadowCaster shadowCaster)
	{
		if (shadowCaster.near)
		{
			_nearCascadeGroup.RemoveShadowCaster(shadowCaster);
		}
		if (shadowCaster.far)
		{
			_farCascadeGroup.RemoveShadowCaster(shadowCaster);
		}
	}

	public void UpdateDynamicFlag(ProxyShadowCaster shadowCaster)
	{
		if (shadowCaster.near)
		{
			_nearCascadeGroup.UpdateDynamicFlag(shadowCaster);
		}
		if (shadowCaster.far)
		{
			_farCascadeGroup.UpdateDynamicFlag(shadowCaster);
		}
	}

	public SphereBounds CalcWorldBounds()
	{
		return new SphereBounds(base.transform.TransformPoint(_bounds.center), _bounds.radius);
	}

	public bool FrustumCheck(Plane[] frustumPlanes)
	{
		SphereBounds sphereBounds = CalcWorldBounds();
		for (int i = 0; i < frustumPlanes.Length; i++)
		{
			if (Vector3.Dot(sphereBounds.center, frustumPlanes[i].normal) + frustumPlanes[i].distance < 0f - sphereBounds.radius)
			{
				return false;
			}
		}
		return true;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawSphere(_bounds.center, _bounds.radius);
		}
	}
}
