using System;
using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class HighSpeedImpactSensor : MonoBehaviour
{
	[SerializeField]
	private SectorDetector _sectorDetector;

	[Space]
	[SerializeField]
	private float _checkSpeedThreshold = 300f;

	[SerializeField]
	private Vector3 _localOffset = Vector3.zero;

	[SerializeField]
	private float _radius;

	[Header("New Method")]
	[SerializeField]
	private bool _verbose;

	[SerializeField]
	private ImpactSensor _impactSensor;

	[SerializeField]
	private Collider _collider;

	private RaycastHit[] _raycastHits = new RaycastHit[32];

	private bool _isPlayer;

	private bool _dieNextUpdate;

	private bool _dead;

	private float _impactSpeed;

	private OWRigidbody _body;

	private float _sqrCheckSpeedThreshold;

	private void Awake()
	{
		_isPlayer = base.gameObject.CompareTag("Player");
		_body = GetComponent<OWRigidbody>();
		_sqrCheckSpeedThreshold = _checkSpeedThreshold * _checkSpeedThreshold;
		_verbose = false;
	}

	private void FixedUpdate()
	{
		if (_isPlayer && (PlayerState.IsAttached() || PlayerState.IsInsideShuttle() || PlayerState.UsingNomaiRemoteCamera()))
		{
			return;
		}
		if (_dieNextUpdate && !_dead)
		{
			_dead = true;
			_dieNextUpdate = false;
			if (base.gameObject.CompareTag("Player"))
			{
				Locator.GetDeathManager().SetImpactDeathSpeed(_impactSpeed);
				Locator.GetDeathManager().KillPlayer(DeathType.Impact);
			}
			else if (base.gameObject.CompareTag("Ship"))
			{
				GetComponent<ShipDamageController>().Explode();
			}
		}
		if (_isPlayer && PlayerState.IsInsideShip())
		{
			HandlePlayerInsideShip();
			return;
		}
		ReferenceFrame passiveReferenceFrame = _sectorDetector.GetPassiveReferenceFrame();
		if (!_dead && passiveReferenceFrame != null)
		{
			if (_isPlayer)
			{
				CheckForImpacts_New(passiveReferenceFrame);
			}
			else
			{
				CheckForImpacts_Old(passiveReferenceFrame);
			}
		}
	}

	private void HandlePlayerInsideShip()
	{
		Vector3 vector = Locator.GetShipTransform().position + Locator.GetShipTransform().up * 2f;
		float num = Vector3.Distance(_body.GetPosition(), vector);
		if (num > 8f)
		{
			_body.SetPosition(vector);
			MonoBehaviour.print("MOVE PLAYER BACK TO SHIP CENTER");
		}
		if (!_dead)
		{
			Vector3 vector2 = _body.GetVelocity() - Locator.GetShipBody().GetPointVelocity(_body.GetPosition());
			if (vector2.sqrMagnitude > _sqrCheckSpeedThreshold)
			{
				_impactSpeed = vector2.magnitude;
				_body.AddVelocityChange(-vector2);
				_dieNextUpdate = true;
				MonoBehaviour.print("HIGH SPEED IMPACT: " + base.name + " hit the Ship at " + _impactSpeed + "m/s   Dist from ship: " + num);
			}
		}
	}

	private void CheckForImpacts_Old(ReferenceFrame referenceFrame)
	{
		Vector3 direction = _body.GetVelocity() - referenceFrame.GetOWRigidBody().GetPointVelocity(_body.GetPosition());
		if (!(direction.sqrMagnitude > _sqrCheckSpeedThreshold))
		{
			return;
		}
		int num = Physics.RaycastNonAlloc(base.transform.TransformPoint(_localOffset), direction, _raycastHits, direction.magnitude * Time.deltaTime + _radius, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			if (!(_raycastHits[i].rigidbody.mass > 10f) || _raycastHits[i].rigidbody.Equals(_body.GetRigidbody()))
			{
				continue;
			}
			OWRigidbody component = _raycastHits[i].rigidbody.GetComponent<OWRigidbody>();
			if (component == null)
			{
				Debug.LogError("Rigidbody does not have attached OWRigidbody!!!");
				Debug.Break();
				continue;
			}
			direction = _body.GetVelocity() - component.GetPointVelocity(_body.GetPosition());
			Vector3 vector = Vector3.Project(direction, _raycastHits[i].normal);
			if (vector.sqrMagnitude > _sqrCheckSpeedThreshold)
			{
				_body.AddVelocityChange(-vector);
				_impactSpeed = vector.magnitude;
				bool flag = false;
				if (!PlayerState.IsInsideTheEye() && !flag)
				{
					_dieNextUpdate = true;
				}
				MonoBehaviour.print("HIGH SPEED IMPACT: " + base.name + " hit " + _raycastHits[i].rigidbody.name + " at " + _impactSpeed + "m/s   RF: " + referenceFrame.GetOWRigidBody().name);
				break;
			}
		}
	}

	private void CheckForImpacts_New(ReferenceFrame referenceFrame)
	{
		Vector3 vector = _body.GetVelocity() - referenceFrame.GetOWRigidBody().GetPointVelocity(_body.GetPosition());
		float num = vector.magnitude * Time.deltaTime;
		if (!(num > _radius))
		{
			return;
		}
		float maxDistance = num * 1.5f;
		int num2 = Physics.RaycastNonAlloc(base.transform.TransformPoint(_localOffset), vector, _raycastHits, maxDistance, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num2; i++)
		{
			if (_raycastHits[i].rigidbody.mass <= 10f || _raycastHits[i].rigidbody.Equals(_body.GetRigidbody()))
			{
				continue;
			}
			OWRigidbody component = _raycastHits[i].rigidbody.GetComponent<OWRigidbody>();
			if (component == null)
			{
				Debug.LogError("Rigidbody does not have attached OWRigidbody!!!");
				Debug.Break();
				continue;
			}
			float num3 = _radius;
			if (_isPlayer)
			{
				float num4 = Vector3.Angle(vector, base.transform.up);
				if (num4 > 90f)
				{
					num4 = Mathf.Abs(num4 - 180f);
				}
				num3 = ((!(num4 >= 45f)) ? Mathf.Cos(num4 * ((float)Math.PI / 180f)) : (0.5f / Mathf.Cos((90f - num4) * ((float)Math.PI / 180f))));
			}
			if (component.GetReferenceFrame() != referenceFrame)
			{
				vector = _body.GetVelocity() - component.GetPointVelocity(_body.GetPosition());
				num = vector.magnitude * Time.deltaTime;
				if (_verbose)
				{
					MonoBehaviour.print("recalculating values relative to " + component.gameObject.name);
				}
			}
			if (_raycastHits[i].distance > num)
			{
				if (_verbose)
				{
					MonoBehaviour.print("overlap next frame should be safe: " + _raycastHits[i].distance + "   meters per frame: " + num + "   time: " + Time.time);
				}
				continue;
			}
			Vector3 vector2 = Vector3.Project(vector, _raycastHits[i].normal);
			float magnitude = vector2.magnitude;
			ImpactData impact = new ImpactData(_body, _collider, component, vector2, magnitude, _raycastHits[i]);
			_impactSensor.FireHighSpeedImpactEvent(impact);
			if (_raycastHits[i].distance < num3)
			{
				if (_verbose)
				{
					MonoBehaviour.print("safe overlap, let physX handle it: " + _raycastHits[i].distance + "   speed: " + magnitude + "   radius: " + num3 + "   time: " + Time.time);
				}
				continue;
			}
			_body.AddVelocityChange(-vector2);
			break;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(base.transform.TransformPoint(_localOffset), _radius);
	}
}
