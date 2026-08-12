using Tessellation;
using UnityEngine;

[ExecuteInEditMode]
public class TessellatedSphereRenderer : TessellatedRenderer
{
	private Sphere _tessellatedSphere;

	private TessellatedSphereLOD _tessellatedSphereLOD;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_tessellatedSphere == null)
		{
			_tessellatedSphere = new Sphere();
		}
		_tessellatedSphereLOD = GetComponent<TessellatedSphereLOD>();
	}

	protected override void ReconfigureMeshBounds()
	{
		for (int i = 0; i < _tessellationMeshGroup.variants.Length; i++)
		{
			_tessellationMeshGroup.variants[i].bounds = new Bounds(new Vector3(0f, 0.75f, 0f), new Vector3(2f, 0.5f, 2f));
		}
	}

	protected override void Rebuild(OWCamera owCamera)
	{
		if (!ShouldRenderInCamera(owCamera) || _tessellatedSphere == null || _tessellationMeshGroup == null)
		{
			return;
		}
		Vector3 localPos = base.transform.InverseTransformPoint(owCamera.transform.position);
		float num = Mathf.Abs(localPos.magnitude - 1f);
		int num2 = 0;
		float num3 = 1 << _LODBias;
		for (int i = 0; i < _maxLOD; i++)
		{
			if (num > num3)
			{
				break;
			}
			num2++;
			num3 *= 0.5f;
		}
		Material[] materials = _materials;
		if (_tessellatedSphereLOD != null)
		{
			float x = base.transform.lossyScale.x;
			float num4 = Vector3.Distance(owCamera.transform.position, base.transform.position) - x;
			if (num4 > _tessellatedSphereLOD._highAltitude)
			{
				materials = _tessellatedSphereLOD._highAltitudeMaterials;
			}
			else if (num4 < _tessellatedSphereLOD._lowAltitude)
			{
				materials = _tessellatedSphereLOD._lowAltitudeMaterials;
				num2 = _tessellatedSphereLOD._lowAltitudeMaxLOD;
			}
		}
		_tessellatedSphere.Init();
		_tessellatedSphere.Tessellate(num2, localPos, _LODRadius);
		if (owCamera.useFarCamera && owCamera.farCamera != null)
		{
			Camera[] cameras = new Camera[2] { owCamera.mainCamera, owCamera.farCamera };
			_tessellatedSphere.Draw(_tessellationMeshGroup, base.transform.localToWorldMatrix, materials, cameras, _cullingMode, base.gameObject.layer);
		}
		else
		{
			_tessellatedSphere.Draw(_tessellationMeshGroup, base.transform.localToWorldMatrix, materials, owCamera.mainCamera, _cullingMode, base.gameObject.layer);
		}
	}

	protected override void Clear(OWCamera owCamera)
	{
		if (_tessellatedSphere != null)
		{
			_tessellatedSphere.Clear();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.clear;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawSphere(Vector3.zero, 1f);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = (OWGizmos.IsDirectlySelected(base.gameObject) ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.25f));
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireSphere(Vector3.zero, 1.002f);
	}
}
