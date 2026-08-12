using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnglerfishAnimController : MonoBehaviour
{
	private Animator _animator;

	[SerializeField]
	private AnglerfishController _anglerfishController;

	[Space]
	[SerializeField]
	private float _moveChangeSpeed = 1f;

	[Space]
	[SerializeField]
	private float _jawOpenSpeed = 1f;

	[SerializeField]
	private float _jawCloseSpeed = 1f;

	[Space]
	[SerializeField]
	private float _spinesFlareSpeed = 1f;

	[SerializeField]
	private float _spinesFlareVariation = 1f;

	[SerializeField]
	private Transform[] _spines = new Transform[0];

	[Space]
	[SerializeField]
	private float _lookSpeed = 1f;

	private float _lastStateChangeTime;

	private float _moveCurrent;

	private float _moveTarget;

	private float _moveStart;

	private float _jawCurrent;

	private float _jawTarget;

	private float _jawStart;

	private float[] _spinesCurrent;

	private float _spinesTarget;

	private float[] _spinesStart;

	private Quaternion[] _spineRotations;

	private float[] _spineOffsets;

	private Vector2 _lookCurrent;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_lastStateChangeTime = 0f;
		_moveCurrent = (_moveTarget = (_moveStart = 0f));
		_jawCurrent = (_jawTarget = (_jawStart = 0.33f));
		_spinesTarget = 0f;
		_spinesCurrent = new float[_spines.Length];
		_spinesStart = new float[_spines.Length];
		_spineRotations = new Quaternion[_spines.Length];
		_spineOffsets = new float[_spines.Length];
		for (int i = 0; i < _spines.Length; i++)
		{
			_spinesCurrent[i] = (_spinesStart[i] = 0f);
			_spineRotations[i] = _spines[i].localRotation;
			_spineOffsets[i] = Random.value * _spinesFlareVariation;
		}
		_animator.SetFloat("MoveSpeed", _moveCurrent);
		_anglerfishController.OnChangeAnglerState += OnChangeAnglerState;
		_anglerfishController.OnAnglerTurn += OnAnglerTurn;
		_anglerfishController.OnAnglerSuspended += OnAnglerSuspended;
		_anglerfishController.OnAnglerUnsuspended += OnAnglerUnsuspended;
	}

	private void OnDestroy()
	{
		_anglerfishController.OnChangeAnglerState -= OnChangeAnglerState;
		_anglerfishController.OnAnglerTurn -= OnAnglerTurn;
		_anglerfishController.OnAnglerSuspended -= OnAnglerSuspended;
		_anglerfishController.OnAnglerUnsuspended -= OnAnglerUnsuspended;
	}

	private void LateUpdate()
	{
		float num = Time.time - _lastStateChangeTime;
		float num2 = Mathf.Clamp01(num / _moveChangeSpeed);
		num2 = (num2 - 2f) * (0f - num2);
		_moveCurrent = Mathf.SmoothStep(_moveStart, _moveTarget, num2);
		float t = Mathf.Clamp01(num / ((_jawTarget > _jawStart) ? _jawOpenSpeed : _jawCloseSpeed));
		_jawCurrent = Mathf.SmoothStep(_jawStart, _jawTarget, t);
		for (int i = 0; i < _spines.Length; i++)
		{
			float num3 = Mathf.Clamp01((num - _spineOffsets[i]) / _spinesFlareSpeed);
			num3 = (num3 - 2f) * (0f - num3);
			_spinesCurrent[i] = Mathf.SmoothStep(_spinesStart[i], _spinesTarget, num3);
		}
		Vector2 target = Vector2.zero;
		if (_anglerfishController.GetAnglerState() == AnglerfishController.AnglerState.Investigating || _anglerfishController.GetAnglerState() == AnglerfishController.AnglerState.Chasing)
		{
			Vector3 vector = _anglerfishController.GetTargetPosition() - _anglerfishController.transform.position;
			Vector3 vector2 = _anglerfishController.transform.InverseTransformVector(vector);
			Vector2 vector3 = default(Vector2);
			vector3.x = OWMath.Angle(Vector3.ProjectOnPlane(vector2, Vector3.up), Vector3.forward, Vector3.up);
			vector3.y = OWMath.Angle(Vector3.ProjectOnPlane(vector2, Vector3.right), Vector3.forward, Vector3.right);
			target = vector3 / 30f;
			target.x = 0f - target.x;
		}
		_lookCurrent = Vector2.MoveTowards(_lookCurrent, target, _lookSpeed * Time.deltaTime);
		_animator.SetFloat("MoveSpeed", _moveCurrent);
		_animator.SetFloat("Jaw", _jawCurrent);
		for (int j = 0; j < _spines.Length; j++)
		{
			_spines[j].localRotation = _spineRotations[j] * Quaternion.AngleAxis(_spinesCurrent[j] * -60f, Vector3.forward);
		}
		_animator.SetFloat("LookX", _lookCurrent.x);
		_animator.SetFloat("LookY", _lookCurrent.y);
	}

	private void OnChangeAnglerState(AnglerfishController.AnglerState newState)
	{
		switch (newState)
		{
		case AnglerfishController.AnglerState.Chasing:
			_moveTarget = 1f;
			break;
		case AnglerfishController.AnglerState.Investigating:
			_moveTarget = 0.5f;
			break;
		default:
			_moveTarget = 0f;
			break;
		}
		switch (newState)
		{
		case AnglerfishController.AnglerState.Chasing:
			_jawTarget = 1f;
			break;
		case AnglerfishController.AnglerState.Consuming:
			_jawTarget = 0f;
			break;
		default:
			_jawTarget = 0.33f;
			break;
		}
		if (newState == AnglerfishController.AnglerState.Chasing)
		{
			_spinesTarget = 1f;
		}
		else
		{
			_spinesTarget = 0f;
		}
		_moveStart = _moveCurrent;
		_jawStart = _jawCurrent;
		for (int i = 0; i < _spines.Length; i++)
		{
			_spinesStart[i] = _spinesCurrent[i];
		}
		_lastStateChangeTime = Time.time;
	}

	private void OnAnglerSuspended(AnglerfishController.AnglerState state)
	{
		base.enabled = false;
		_animator.enabled = false;
	}

	private void OnAnglerUnsuspended(AnglerfishController.AnglerState state)
	{
		base.enabled = true;
		_animator.enabled = true;
		_animator.SetFloat("MoveSpeed", _moveCurrent);
	}

	private void OnAnglerTurn()
	{
		_animator.SetTrigger("Impulse");
	}
}
