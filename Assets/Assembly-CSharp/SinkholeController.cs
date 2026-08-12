using UnityEngine;

[RequireComponent(typeof(SinkholeFluidVolume))]
public class SinkholeController : MonoBehaviour
{
	private SinkholeFluidVolume _sinkholeVolume;

	private ParticleSystem _particleSystem;

	[SerializeField]
	private bool _startActivated;

	[SerializeField]
	private MeshRenderer _sinkholeRenderer;

	[SerializeField]
	private int _sinkholeMaterialIndex;

	[Space(10f)]
	[SerializeField]
	private float _revealLength = 1f;

	[SerializeField]
	private Vector2 _uvScrollSpeed = new Vector2(0.1f, -0.1f);

	[Space(10f)]
	[SerializeField]
	private SandLevelController _sandSphere;

	[SerializeField]
	private float _sandsphereDeactivateHeight = -5f;

	private bool _sinkholeActivated;

	private float _revealTimer;

	private float _deactivateRadius;

	private void Awake()
	{
		_sinkholeVolume = GetComponent<SinkholeFluidVolume>();
		_sinkholeVolume.OnSinkholeActivated += Activate;
		_particleSystem = GetComponentInChildren<ParticleSystem>();
		_sinkholeActivated = false;
		_revealTimer = 0f;
		_deactivateRadius = Vector3.Distance(base.transform.position, this.GetAttachedOWRigidbody().transform.position) + _sandsphereDeactivateHeight;
		if (_sinkholeRenderer != null)
		{
			_sinkholeRenderer.bounds.Expand(1f);
		}
		base.enabled = false;
		if (_startActivated)
		{
			Activate();
		}
	}

	private void OnDestroy()
	{
		_sinkholeVolume.OnSinkholeActivated -= Activate;
	}

	public void Activate()
	{
		_sinkholeActivated = true;
		_sinkholeVolume.SetVolumeActivation(active: true);
		if (_particleSystem != null)
		{
			_particleSystem.Play();
		}
		base.enabled = true;
	}

	public void Deactivate()
	{
		_sinkholeActivated = false;
		_sinkholeVolume.SetVolumeActivation(active: false);
		if (_particleSystem != null)
		{
			_particleSystem.Stop();
		}
	}

	private void Update()
	{
		_revealTimer = Mathf.Clamp01(_sinkholeActivated ? (_revealTimer + Time.deltaTime / _revealLength) : (_revealTimer - Time.deltaTime / _revealLength));
		float num = (2f - _revealTimer) * _revealTimer;
		if (_sandSphere != null && _sandSphere.GetRadius() > _deactivateRadius)
		{
			Deactivate();
		}
		if (_sinkholeRenderer != null)
		{
			_sinkholeRenderer.materials[_sinkholeMaterialIndex].SetFloat("_Activity", num);
			Vector2 textureOffset = _sinkholeRenderer.materials[_sinkholeMaterialIndex].GetTextureOffset("_VortexTex");
			_sinkholeRenderer.materials[_sinkholeMaterialIndex].SetTextureOffset("_VortexTex", textureOffset + _uvScrollSpeed * num * Time.deltaTime);
		}
		if (!_sinkholeActivated && _revealTimer == 0f)
		{
			base.enabled = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 normalized = (((this.GetAttachedOWRigidbody() != null) ? this.GetAttachedOWRigidbody().transform.position : Vector3.zero) - base.transform.position).normalized;
		Gizmos.color = Color.red;
		Gizmos.DrawRay(base.transform.position, normalized * (0f - _sandsphereDeactivateHeight));
		OWGizmos.DrawWireCircle(base.transform.position + normalized * (0f - _sandsphereDeactivateHeight), normalized, 1f);
	}
}
