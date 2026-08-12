using UnityEngine;

public class SignalDishRingController : SectoredMonoBehaviour
{
	[SerializeField]
	private float _direction = 1f;

	[SerializeField]
	private float _maxSpeed = 60f;

	[SerializeField]
	private OWAudioSource _rotationAudio;

	private Transform _target;

	private bool _searchingForTheEye;

	private float _degreesToTarget;

	private float _localTargetDegrees;

	private float _speed;

	private float _origDirection;

	private void Start()
	{
		base.enabled = false;
		_searchingForTheEye = false;
		_localTargetDegrees = base.transform.localEulerAngles.y;
		_degreesToTarget = 0f;
		_speed = 0f;
		_origDirection = _direction;
	}

	public bool IsAlignedWithTarget()
	{
		if (_target != null)
		{
			return _degreesToTarget < 1f;
		}
		return false;
	}

	public void SetAstroObjectTarget(AstroObject.Name astroName)
	{
		if (astroName == AstroObject.Name.Eye)
		{
			_searchingForTheEye = true;
			return;
		}
		AstroObject astroObject = Locator.GetAstroObject(astroName);
		if (astroObject != null)
		{
			_direction = _origDirection;
			_target = astroObject.transform;
			_degreesToTarget = GetDegreesToTargetPosition(_target.position);
		}
	}

	public void RemoveTarget()
	{
		_target = null;
		_searchingForTheEye = false;
	}

	private void FixedUpdate()
	{
		float num = 0f;
		float num2 = 20f;
		if (_searchingForTheEye)
		{
			if (Mathf.Abs(_degreesToTarget) < 1f && _speed < 1f)
			{
				_direction *= -1f;
				Vector2 insideUnitCircle = Random.insideUnitCircle;
				Vector3 worldPos = base.transform.position + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y);
				_localTargetDegrees = GetLocalDegrees(worldPos);
				_degreesToTarget = GetDegreesToTargetPosition(worldPos);
			}
		}
		else if (_target != null)
		{
			_localTargetDegrees = GetLocalDegrees(_target.position);
		}
		if (_searchingForTheEye || _target != null)
		{
			float num3 = Mathf.InverseLerp(20f, 0f, Mathf.Abs(_degreesToTarget));
			if (num3 > 0f)
			{
				num = Mathf.Lerp(_maxSpeed, 0f, num3);
				if (num < _speed)
				{
					num2 = 100f;
				}
			}
			else
			{
				num = _maxSpeed;
			}
		}
		_speed = Mathf.MoveTowards(_speed, num, num2 * Time.deltaTime);
		_degreesToTarget = Mathf.MoveTowards(_degreesToTarget, 0f, _speed * Time.deltaTime);
		base.transform.localEulerAngles = Vector3.up * (_localTargetDegrees - _degreesToTarget);
		if (_rotationAudio != null)
		{
			if (!_rotationAudio.isPlaying)
			{
				_rotationAudio.Play();
				_rotationAudio.RandomizePlayhead();
			}
			_rotationAudio.SetLocalVolume(Mathf.InverseLerp(0f, _maxSpeed, _speed));
		}
	}

	private float GetLocalDegrees(Vector3 worldPos)
	{
		Vector3 to = Vector3.ProjectOnPlane(worldPos - base.transform.position, base.transform.up);
		return OWMath.Angle(base.transform.parent.forward, to, base.transform.up);
	}

	private float GetDegreesToTargetPosition(Vector3 worldPos)
	{
		Vector3 to = Vector3.ProjectOnPlane(worldPos - base.transform.position, base.transform.up);
		float num = OWMath.Angle(base.transform.forward, to, base.transform.up);
		if (num < 0f && _direction > 0f)
		{
			num += 360f;
		}
		else if (num > 0f && _direction < 0f)
		{
			num -= 360f;
		}
		return num;
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
			if (_rotationAudio != null)
			{
				_rotationAudio.FadeOut(1f);
			}
		}
	}
}
