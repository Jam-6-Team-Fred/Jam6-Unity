using UnityEngine;

[ExecuteInEditMode] // CHANGED
public class VolumeOcclusionRenderer : MonoBehaviour
{
	[SerializeField]
	private Mesh _mesh;

	[SerializeField]
	[Range(0f, 1f)]
	private float _occlusionStrength = 1f;

	private Transform _transform;

	private Vector4 _localBounds;

	public Mesh mesh
	{
		get
		{
			return _mesh;
		}
		set
		{
			_mesh = value;
			RecalculateLocalBounds();
		}
	}

	public float occlusionStrength
	{
		get
		{
			return _occlusionStrength;
		}
		set
		{
			_occlusionStrength = Mathf.Clamp01(value);
		}
	}

	public Matrix4x4 localToWorldMatrix => _transform.localToWorldMatrix;

	public Vector4 localBounds => _localBounds;

	public Vector4 CalcWorldBounds()
	{
		Vector3 vector = _transform.TransformPoint(new Vector3(_localBounds.x, _localBounds.y, _localBounds.z));
		Vector3 lossyScale = _transform.lossyScale;
		if (lossyScale.x < 0f)
		{
			lossyScale.x = 0f - lossyScale.x;
		}
		if (lossyScale.y < 0f)
		{
			lossyScale.y = 0f - lossyScale.y;
		}
		if (lossyScale.z < 0f)
		{
			lossyScale.z = 0f - lossyScale.z;
		}
		float num = lossyScale.x;
		if (num < lossyScale.y)
		{
			num = lossyScale.y;
		}
		if (num < lossyScale.z)
		{
			num = lossyScale.z;
		}
		float w = localBounds.w * num;
		return new Vector4(vector.x, vector.y, vector.z, w);
	}

	private void Awake()
	{
		_transform = base.transform;
		RecalculateLocalBounds();
	}

	private void OnEnable()
	{
		VolumeOcclusionManager.RegisterVolumeOcclusionRenderer(this);
	}

	private void OnDisable()
	{
		VolumeOcclusionManager.UnregisterVolumeOcclusionRenderer(this);
	}

	private void RecalculateLocalBounds()
	{
		if (_mesh == null)
		{
			_localBounds = Vector4.zero;
			return;
		}
		Bounds bounds = _mesh.bounds;
		_localBounds = new Vector4(bounds.center.x, bounds.center.y, bounds.center.z, bounds.extents.magnitude);
	}

	private void OnDrawGizmosSelected()
	{
		if (_mesh != null) // CHANGED
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
			Gizmos.matrix = localToWorldMatrix;
			Gizmos.DrawWireMesh(_mesh);
		}
	}
}
