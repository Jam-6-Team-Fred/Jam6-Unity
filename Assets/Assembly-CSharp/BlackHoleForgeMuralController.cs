using UnityEngine;

public class BlackHoleForgeMuralController : SectoredMonoBehaviour
{
	[SerializeField]
	private Transform _rotatingDisk;

	[SerializeField]
	private float _rotationSpeed;

	[SerializeField]
	private float _angleOfTrigerring;

	[SerializeField]
	private Renderer _cableRenderer;

	private float _currentRotation;

	private bool _towerOn;

	private float _targetGlow;

	private Color _glowColor;

	private void Start()
	{
		_towerOn = true;
		_glowColor = _cableRenderer.material.GetColor("_Glow");
	}

	private void Update()
	{
		_glowColor.a = Mathf.MoveTowards(_glowColor.a, _targetGlow, Time.deltaTime / 0.2f);
		_cableRenderer.material.SetColor("_Glow", _glowColor);
	}

	private void FixedUpdate()
	{
		_currentRotation += _rotationSpeed;
		_rotatingDisk.localEulerAngles = new Vector3(0f, 0f, _currentRotation);
		if (_towerOn && _currentRotation % 360f > _angleOfTrigerring)
		{
			_towerOn = false;
			_targetGlow = 0f;
		}
		if (!_towerOn && _currentRotation % 360f > 360f - _angleOfTrigerring)
		{
			_towerOn = true;
			_targetGlow = 1f;
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}
}
