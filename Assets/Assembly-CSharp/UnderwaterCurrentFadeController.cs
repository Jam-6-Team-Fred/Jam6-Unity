using UnityEngine;

public class UnderwaterCurrentFadeController : SectoredMonoBehaviour
{
	[SerializeField]
	private float _minAlpha = 0.5f;

	[SerializeField]
	private float _minDistance;

	[SerializeField]
	private float _maxDistance;

	[SerializeField]
	private OWRenderer[] _currentRenderers;

	private int _overlayColorID;

	private bool _active;

	private void OnValidate()
	{
		if (_minDistance > _maxDistance)
		{
			_minDistance = _maxDistance;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_overlayColorID = Shader.PropertyToID("_DetailColor");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		OWCamera.onAnyPreRender -= new OWEvent<OWCamera>.OWCallback(OnAnyPrerender);
		OWCamera.onAnyPostRender -= new OWEvent<OWCamera>.OWCallback(OnAnyPostrender);
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool active = _active;
		_active = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (!active && _active)
		{
			OWCamera.onAnyPreRender += new OWEvent<OWCamera>.OWCallback(OnAnyPrerender);
			OWCamera.onAnyPostRender += new OWEvent<OWCamera>.OWCallback(OnAnyPostrender);
		}
		else if (active && !_active)
		{
			OWCamera.onAnyPreRender -= new OWEvent<OWCamera>.OWCallback(OnAnyPrerender);
			OWCamera.onAnyPostRender -= new OWEvent<OWCamera>.OWCallback(OnAnyPostrender);
		}
	}

	private void OnAnyPrerender(OWCamera owCamera)
	{
		float value = Vector3.Distance(owCamera.transform.position, base.transform.position);
		float t = Mathf.InverseLerp(_maxDistance, _minDistance, value);
		SetAlpha(Mathf.Lerp(_minAlpha, 1f, t));
	}

	private void OnAnyPostrender(OWCamera owCamera)
	{
		SetAlpha(_minAlpha);
	}

	private void SetAlpha(float alpha)
	{
		for (int i = 0; i < _currentRenderers.Length; i++)
		{
			_currentRenderers[i].SetColor(_currentRenderers[i].GetOriginalColor() * alpha);
			_currentRenderers[i].SetMaterialProperty(_overlayColorID, _currentRenderers[i].sharedMaterial.GetColor(_overlayColorID) * alpha);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(base.transform.position, _minDistance);
		Gizmos.DrawWireSphere(base.transform.position, _maxDistance);
	}
}
