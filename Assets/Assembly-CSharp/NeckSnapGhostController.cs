using UnityEngine;

public class NeckSnapGhostController : MonoBehaviour
{
	private enum State
	{
		Wandering = 0,
		Hunting = 1,
		Killing = 2
	}

	[SerializeField]
	private Transform _solidHead;

	[SerializeField]
	private GameObject _solidRoot;

	[SerializeField]
	private GameObject _transparentRoot;

	[SerializeField]
	private OWLight _faceLight;

	[SerializeField]
	private float _wanderRadius;

	private OWRigidbody _planetBody;

	private MeshRenderer[] _transparentRenderers;

	private Material _transparentMaterial;

	private State _state;

	private float _stateChangeTime;

	private bool _snappingNeck;

	private Quaternion _startSnapCamRotation;

	private Vector3 _origLocalPosition;

	private Vector3 _targetLocalPosition;

	private void Awake()
	{
		_transparentRenderers = _transparentRoot.GetComponentsInChildren<MeshRenderer>();
		_transparentMaterial = new Material(_transparentRenderers[0].sharedMaterial);
		for (int i = 0; i < _transparentRenderers.Length; i++)
		{
			_transparentRenderers[i].sharedMaterial = _transparentMaterial;
		}
		_planetBody = base.gameObject.GetAttachedOWRigidbody();
		_origLocalPosition = base.transform.localPosition;
	}

	private void Start()
	{
		_faceLight.SetIntensity(0f);
		_solidRoot.SetActive(value: false);
		_transparentRoot.SetActive(value: true);
		_origLocalPosition = base.transform.localPosition;
		_targetLocalPosition = GetRandomWanderPosition();
		ChangeState(State.Wandering);
	}

	private void OnDestroy()
	{
	}

	private Vector3 GetRandomWanderPosition()
	{
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		return _origLocalPosition + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * _wanderRadius;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.K))
		{
			KillPlayer();
		}
	}

	private void FixedUpdate()
	{
		if (_state == State.Wandering)
		{
			Vector3 to = _targetLocalPosition - base.transform.localPosition;
			to.y = 0f;
			Quaternion quaternion = Quaternion.AngleAxis(OWMath.Angle(base.transform.parent.InverseTransformDirection(base.transform.forward), to, Vector3.up), Vector3.up);
			base.transform.localRotation = quaternion * base.transform.localRotation;
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, _targetLocalPosition, Time.deltaTime);
			if (to.magnitude < 1f)
			{
				_targetLocalPosition = GetRandomWanderPosition();
			}
		}
		else
		{
			if (_state != State.Killing)
			{
				return;
			}
			float num = 0.1f;
			float num2 = 1.2f;
			float num3 = Mathf.InverseLerp(_stateChangeTime + num2, _stateChangeTime + num2 + num, Time.time);
			if (!_snappingNeck && num3 > 0f && num3 < 1f)
			{
				_snappingNeck = true;
				Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().enabled = false;
				_startSnapCamRotation = Locator.GetPlayerCamera().transform.rotation;
			}
			else if (_snappingNeck)
			{
				Vector3 axis = _solidHead.position - Locator.GetPlayerCamera().transform.position;
				Quaternion quaternion2 = Quaternion.AngleAxis(45f * num3, axis);
				Locator.GetPlayerCamera().transform.rotation = Quaternion.Slerp(_startSnapCamRotation, quaternion2 * _startSnapCamRotation, num3);
				if (num3 >= 1f)
				{
					_snappingNeck = false;
					Locator.GetDeathManager().KillPlayer(DeathType.Default);
				}
			}
			_solidHead.position = Vector3.MoveTowards(_solidHead.position, Locator.GetPlayerCamera().transform.position, Time.deltaTime * 0.03f);
		}
	}

	private void ChangeState(State newState)
	{
		_state = newState;
		_stateChangeTime = Time.time;
	}

	private void KillPlayer()
	{
		if (_state != State.Killing)
		{
			ChangeState(State.Killing);
			GlobalMessenger.FireEvent("GhostKillPlayer");
			_transparentRoot.SetActive(value: false);
			_solidRoot.SetActive(value: true);
			Transform playerTransform = Locator.GetPlayerTransform();
			Vector3 worldPosition = playerTransform.position - playerTransform.up;
			OWInput.ChangeInputMode(InputMode.None);
			Locator.GetPlayerBody().SetVelocity(_planetBody.GetPointVelocity(playerTransform.position));
			Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(_solidHead, 2f);
			base.transform.position = playerTransform.position - playerTransform.up + playerTransform.forward * 1.5f;
			base.transform.LookAt(worldPosition, playerTransform.up);
			base.transform.parent = playerTransform;
			_faceLight.FadeTo(1f, 0.2f);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		OWGizmos.DrawWireCircle(base.transform.position, base.transform.up, _wanderRadius);
	}
}
