using UnityEngine;

public class AlarmTotem : SectoredMonoBehaviour
{
	public delegate void AlarmTotemObjectEvent();

	[SerializeField]
	private Transform _sightOrigin;

	[SerializeField]
	private OWLightController _pulseLightController;

	[Space]
	[SerializeField]
	private float _sightDistance = 50f;

	[SerializeField]
	private float _sightAngle = 90f;

	[Space]
	[SerializeField]
	private Transform _leftFaceCover;

	[SerializeField]
	private Transform _rightFaceCover;

	[Header("Simulation")]
	[SerializeField]
	private Renderer _simTotemRenderer;

	[SerializeField]
	private Material _simAlarmMaterial;

	[SerializeField]
	private OWRenderer _simVisionConeRenderer;

	[SerializeField]
	[ColorUsage(false, true)]
	private Color _simAlarmColor;

	private bool _isPlayerVisible;

	private bool _isFaceOpen = true;

	private float _faceDegrees;

	private bool _hasConcealedFromAlarm;

	private float _secondsConcealed;

	private Material _origSimEyeMaterial;

	private Material[] _simTotemMaterials;

	private static RaycastHit[] s_raycastHitBuffer = new RaycastHit[32];

	public event AlarmTotemObjectEvent OnRinging;

	private void Start()
	{
		_pulseLightController.SetIntensity(0f);
		_faceDegrees = _rightFaceCover.localEulerAngles.y;
		_simTotemMaterials = _simTotemRenderer.sharedMaterials;
		_origSimEyeMaterial = _simTotemMaterials[0];
		base.enabled = false;
	}

	public void SetFaceOpen(bool open)
	{
		if (_isFaceOpen != open)
		{
			_rightFaceCover.localEulerAngles = Vector3.up * (_isFaceOpen ? 0f : (-90f));
			_leftFaceCover.localEulerAngles = Vector3.up * (_isFaceOpen ? 0f : 90f);
		}
		_isFaceOpen = open;
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			base.enabled = true;
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			base.enabled = false;
			_pulseLightController.SetIntensity(0f);
			_simTotemMaterials[0] = _origSimEyeMaterial;
			_simTotemRenderer.sharedMaterials = _simTotemMaterials;
			_simVisionConeRenderer.SetColor(_simVisionConeRenderer.GetOriginalColor());
			if (_isPlayerVisible)
			{
				_isPlayerVisible = false;
				_secondsConcealed = 0f;
				Locator.GetAlarmSequenceController().DecreaseAlarmCounter();
			}
		}
	}

	private void FixedUpdate()
	{
		bool isPlayerVisible = _isPlayerVisible;
		_isPlayerVisible = CheckPlayerVisible();
		if (_isPlayerVisible && !isPlayerVisible)
		{
			Locator.GetAlarmSequenceController().IncreaseAlarmCounter();
			_secondsConcealed = 0f;
			_simTotemMaterials[0] = _simAlarmMaterial;
			_simTotemRenderer.sharedMaterials = _simTotemMaterials;
			_simVisionConeRenderer.SetColor(_simAlarmColor);
			GlobalMessenger.FireEvent("AlarmTotemTriggered");
		}
		else if (isPlayerVisible && !_isPlayerVisible)
		{
			Locator.GetAlarmSequenceController().DecreaseAlarmCounter();
			_secondsConcealed = 0f;
			_simTotemMaterials[0] = _origSimEyeMaterial;
			_simTotemRenderer.sharedMaterials = _simTotemMaterials;
			_simVisionConeRenderer.SetColor(_simVisionConeRenderer.GetOriginalColor());
			_pulseLightController.FadeTo(0f, 0.5f);
		}
	}

	private void Update()
	{
		if (_isPlayerVisible)
		{
			float pulseIntensity = Locator.GetAlarmSequenceController().GetPulseIntensity();
			_pulseLightController.SetIntensity(pulseIntensity);
			if (this.OnRinging != null)
			{
				this.OnRinging();
			}
		}
	}

	private bool CheckPlayerVisible()
	{
		if (!_isFaceOpen)
		{
			return false;
		}
		Vector3 position = Locator.GetPlayerCamera().transform.position;
		if (CheckPointInVisionCone(position) && !CheckLineOccluded(_sightOrigin.position, position))
		{
			if (Locator.GetPlayerLightSensor().IsIlluminated())
			{
				return true;
			}
			DreamLanternController lanternController = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController();
			if (lanternController.IsHeldByPlayer())
			{
				if (lanternController.IsConcealed())
				{
					if (!_hasConcealedFromAlarm)
					{
						_secondsConcealed += Time.deltaTime;
						if (_secondsConcealed > 1f)
						{
							_hasConcealedFromAlarm = true;
							GlobalMessenger.FireEvent("ConcealFromAlarmTotem");
						}
					}
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private bool CheckPointInVisionCone(Vector3 worldPosition)
	{
		Vector3 vector = worldPosition - _sightOrigin.position;
		if (vector.sqrMagnitude < _sightDistance * _sightDistance && Vector3.Angle(Vector3.ProjectOnPlane(vector, base.transform.up), base.transform.forward) < _sightAngle * 0.5f)
		{
			return true;
		}
		return false;
	}

	private bool CheckLineOccluded(Vector3 startPos, Vector3 endPos)
	{
		Vector3 direction = endPos - startPos;
		int num = Physics.RaycastNonAlloc(startPos, direction, s_raycastHitBuffer, direction.magnitude, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			if (!s_raycastHitBuffer[i].collider.CompareTag("Player"))
			{
				return true;
			}
		}
		return false;
	}

	private void OnDrawGizmosSelected()
	{
		if (_sightOrigin == null) return; // CHANGED
		Quaternion quaternion = Quaternion.AngleAxis(_sightAngle * 0.5f, base.transform.up);
		Vector3 vector = quaternion * (base.transform.forward * _sightDistance);
		Vector3 vector2 = Quaternion.Inverse(quaternion) * (base.transform.forward * _sightDistance);
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(_sightOrigin.position, _sightOrigin.position + vector);
		Gizmos.DrawLine(_sightOrigin.position, _sightOrigin.position + vector2);
		Gizmos.color = Color.blue;
		OWGizmos.DrawWireCircle(_sightOrigin.position, base.transform.up, _sightDistance);
	}
}
