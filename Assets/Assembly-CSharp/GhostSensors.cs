using UnityEngine;

public class GhostSensors : MonoBehaviour
{
	public const float GRAB_WINDOW = 0.5f;

	[SerializeField]
	private Transform _sightOrigin;

	[SerializeField]
	private DreamLanternController _lantern;

	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private OWTriggerVolume _contactTrigger;

	[SerializeField]
	private BoxShape _contactEdgeCatcherShape;

	[SerializeField]
	private Animator _animator;

	[Header("Grab Buffs")]
	[SerializeField]
	private float _grabDistanceBuff;

	[SerializeField]
	private float _grabAngleBuff;

	private GhostData _data;

	private OWTriggerVolume _guardVolume;

	private Vector3 _origEdgeCatcherSize;

	private static RaycastHit[] s_raycastHitBuffer = new RaycastHit[32];

	public void Initialize(GhostData data, OWTriggerVolume guardVolume = null)
	{
		_data = data;
		_contactTrigger.OnEntry += OnEnterContactTrigger;
		_contactTrigger.OnExit += OnExitContactTrigger;
		_origEdgeCatcherSize = _contactEdgeCatcherShape.size;
		_guardVolume = guardVolume;
		if (_guardVolume != null)
		{
			_guardVolume.OnEntry += OnEnterGuardVolume;
			_guardVolume.OnExit += OnExitGuardVolume;
		}
	}

	public void RemoveEventListeners()
	{
		_contactTrigger.OnEntry -= OnEnterContactTrigger;
		_contactTrigger.OnExit -= OnExitContactTrigger;
		if (_guardVolume != null)
		{
			_guardVolume.OnEntry -= OnEnterGuardVolume;
			_guardVolume.OnExit -= OnExitGuardVolume;
		}
	}

	public bool CanGrabPlayer()
	{
		if (!PlayerState.IsAttached() && _data.playerLocation.distanceXZ < 2f + _grabDistanceBuff && _data.playerLocation.toPosition.y > -2f && _data.playerLocation.toPosition.y < 3f && _data.playerLocation.degreesToPositionXZ < 20f + _grabAngleBuff)
		{
			return _animator.GetFloat("GrabWindow") > 0.5f;
		}
		return false;
	}

	public void SetContactEdgeCatcherWidth(float width)
	{
		_contactEdgeCatcherShape.size = new Vector3(width, _origEdgeCatcherSize.y, _origEdgeCatcherSize.z);
	}

	public void ResetContactEdgeCatcherWidth()
	{
		_contactEdgeCatcherShape.size = _origEdgeCatcherSize;
	}

	public bool CheckPositionOccluded(Vector3 worldPosition)
	{
		return CheckLineOccluded(_sightOrigin.position, worldPosition);
	}

	public void Update_Sensors()
	{
	}

	public void FixedUpdate_Sensors()
	{
		DreamLanternController lanternController = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController();
		LightSensor playerLightSensor = Locator.GetPlayerLightSensor();
		_data.sensor.isPlayerHoldingLantern = lanternController.IsHeldByPlayer();
		_data.sensor.isIlluminated = _lightSensor.IsIlluminated();
		_data.sensor.isIlluminatedByPlayer = lanternController.IsHeldByPlayer() && _lightSensor.IsIlluminatedByLantern(lanternController);
		_data.sensor.isPlayerIlluminatedByUs = playerLightSensor.IsIlluminatedByLantern(_lantern);
		_data.sensor.isPlayerIlluminated = playerLightSensor.IsIlluminated();
		_data.sensor.isPlayerVisible = false;
		_data.sensor.isPlayerHeldLanternVisible = false;
		_data.sensor.isPlayerDroppedLanternVisible = false;
		_data.sensor.isPlayerOccluded = false;
		if ((lanternController.IsHeldByPlayer() && !lanternController.IsConcealed()) || playerLightSensor.IsIlluminated())
		{
			Vector3 position = Locator.GetPlayerCamera().transform.position;
			if (CheckPointInVisionCone(position))
			{
				if (CheckLineOccluded(_sightOrigin.position, position))
				{
					_data.sensor.isPlayerOccluded = true;
				}
				else
				{
					_data.sensor.isPlayerVisible = playerLightSensor.IsIlluminated();
					_data.sensor.isPlayerHeldLanternVisible = lanternController.IsHeldByPlayer() && !lanternController.IsConcealed();
				}
			}
		}
		if (!lanternController.IsHeldByPlayer() && CheckPointInVisionCone(lanternController.GetLightPosition()) && !CheckLineOccluded(_sightOrigin.position, lanternController.GetLightPosition()))
		{
			_data.sensor.isPlayerDroppedLanternVisible = true;
		}
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

	private bool CheckPointInVisionCone(Vector3 worldPosition)
	{
		Vector3 vector = worldPosition - _sightOrigin.position;
		if (vector.sqrMagnitude < 2500f)
		{
			Vector3 vector2 = _sightOrigin.InverseTransformPoint(worldPosition);
			if (vector2.z < 1f && vector2.z > 0f && Mathf.Abs(vector2.x) < 0.5f)
			{
				return true;
			}
			if (Vector3.Angle(Vector3.ProjectOnPlane(vector, base.transform.up), base.transform.forward) < 20f)
			{
				return true;
			}
		}
		return false;
	}

	private void OnEnterContactTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_data.sensor.inContactWithPlayer = true;
		}
	}

	private void OnExitContactTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_data.sensor.inContactWithPlayer = false;
		}
	}

	private void OnEnterGuardVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_data.sensor.isPlayerInGuardVolume = true;
		}
	}

	private void OnExitGuardVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_data.sensor.isPlayerInGuardVolume = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_sightOrigin == null) return; // CHANGED
		Gizmos.color = Color.blue;
		Quaternion quaternion = Quaternion.AngleAxis(20f, base.transform.up);
		Vector3 vector = quaternion * (base.transform.forward * 50f);
		Vector3 vector2 = Quaternion.Inverse(quaternion) * (base.transform.forward * 50f);
		Gizmos.DrawLine(_sightOrigin.position, _sightOrigin.position + vector);
		Gizmos.DrawLine(_sightOrigin.position, _sightOrigin.position + vector2);
		Gizmos.DrawLine(_sightOrigin.position + base.transform.right * 0.5f, _sightOrigin.position + base.transform.forward + base.transform.right * 0.5f);
		Gizmos.DrawLine(_sightOrigin.position - base.transform.right * 0.5f, _sightOrigin.position + base.transform.forward - base.transform.right * 0.5f);
		Gizmos.DrawLine(_sightOrigin.position + base.transform.forward + base.transform.right * 0.5f, _sightOrigin.position + base.transform.forward - base.transform.right * 0.5f);
	}
}
