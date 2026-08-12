using UnityEngine;

public class ShipAltimeter : MonoBehaviour
{
	[SerializeField]
	private Transform _raycastOrigin;

	[SerializeField]
	private float _shipRadius;

	[Space(10f)]
	[SerializeField]
	private float _barLength = 1f;

	[SerializeField]
	private Transform _shipIcon;

	[SerializeField]
	private Transform _groundBar;

	[SerializeField]
	private DampedSpring _groundBarSpring = new DampedSpring();

	private RulesetDetector _shipRulesetDetector;

	private const int RAYCAST_BUFFER_SIZE = 32;

	private RaycastHit[] _raycastHits;

	private bool _altimeterActive;

	private float _terrainAltitude;

	private float _shipAltitude;

	private void Awake()
	{
		_raycastHits = new RaycastHit[32];
	}

	private void Start()
	{
		_shipRulesetDetector = Locator.GetShipDetector().GetComponent<RulesetDetector>();
	}

	private void Update()
	{
		_altimeterActive = PlayerState.IsInsideShip() && _shipRulesetDetector.GetUseAltimeter(_raycastOrigin.position);
		if (!_altimeterActive)
		{
			return;
		}
		PlanetoidRuleset planetoidRuleset = _shipRulesetDetector.GetPlanetoidRuleset();
		OWRigidbody attachedOWRigidbody = planetoidRuleset.GetAttachedOWRigidbody();
		Vector3 direction = attachedOWRigidbody.GetWorldCenterOfMass() - _raycastOrigin.position;
		float magnitude = direction.magnitude;
		int num = -1;
		int num2 = Physics.RaycastNonAlloc(_raycastOrigin.position, direction, _raycastHits, magnitude, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
		if (num2 > 0)
		{
			float num3 = float.PositiveInfinity;
			for (int i = 0; i < num2; i++)
			{
				if (_raycastHits[i].rigidbody == attachedOWRigidbody.GetRigidbody() && _raycastHits[i].distance < num3)
				{
					num = i;
					num3 = _raycastHits[i].distance;
				}
			}
		}
		_terrainAltitude = ((num > -1) ? planetoidRuleset.GetAltitude(magnitude - _raycastHits[num].distance) : planetoidRuleset.GetAltitudeFloor());
		_shipAltitude = _shipRulesetDetector.GetPlanetoidRuleset().GetAltitude(magnitude - _shipRadius);
	}

	private void FixedUpdate()
	{
		_altimeterActive = PlayerState.IsInsideShip() && _shipRulesetDetector.GetUseAltimeter(_raycastOrigin.position);
		float num = (_altimeterActive ? _shipRulesetDetector.GetPlanetoidRuleset().AltitudeToNormalizedAltitude(_shipAltitude) : 1f);
		float targetValue = (_altimeterActive ? _shipRulesetDetector.GetPlanetoidRuleset().AltitudeToNormalizedAltitude(_terrainAltitude) : 0f);
		if ((bool)_shipIcon)
		{
			_shipIcon.localPosition = new Vector3(num * _barLength * -1f - 0.015f, 0f, 0f);
		}
		if ((bool)_groundBar)
		{
			float num2 = _groundBarSpring.Update(_groundBar.localScale.x, targetValue, Time.deltaTime);
			if (num2 < 0f || num2 > 1f)
			{
				_groundBarSpring.velocity *= -0.5f;
				num2 = Mathf.Clamp01(num2);
			}
			_groundBar.localScale = new Vector3(num2, 1f, 1f);
		}
	}

	public bool AltimeterIsActive()
	{
		return _altimeterActive;
	}

	public float GetTerrainAltitude()
	{
		return _terrainAltitude;
	}

	public float GetShipAltitude()
	{
		return _shipAltitude;
	}

	public float GetShipAltitudeAboveTerrain()
	{
		return _shipAltitude - _terrainAltitude;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject) && _raycastOrigin != null)
		{
			Gizmos.color = new Color(0.5f, 0.5f, 1f, 1f);
			Gizmos.DrawWireSphere(_raycastOrigin.position, _shipRadius);
		}
	}
}
