using UnityEngine;

public class DayNightTunnel : MonoBehaviour
{
	private enum State
	{
		Standbye = 0,
		ClosingShell = 1,
		Traveling = 2,
		OpeningShell = 3
	}

	private const float _degreesPerSecond = 60f;

	[SerializeField]
	private Transform _vehicleRoot;

	[SerializeField]
	private Transform _platform;

	[SerializeField]
	private Transform _dayShell;

	[SerializeField]
	private Transform _nightShell;

	[SerializeField]
	private PlayerAttachPoint _attachPoint;

	[SerializeField]
	private LightCodeInterpreter _lightCodeInterpreter;

	[SerializeField]
	private float _height = 40f;

	private DayNightPlanetController _dayNightController;

	private bool _heads;

	private Transform _firstShell;

	private Transform _secondShell;

	private State _state;

	private void Start()
	{
		base.enabled = false;
		_dayNightController = GetComponentInParent<DayNightPlanetController>();
		_heads = _dayNightController.IsPointOnHeads(base.transform.position);
		_lightCodeInterpreter.OnEnterCode += OnEnterCode;
	}

	private void OnDestroy()
	{
		_lightCodeInterpreter.OnEnterCode -= OnEnterCode;
	}

	private void OnEnterCode(LightCodeName codeName)
	{
		bool flag = _dayNightController.IsDay(_heads);
		if ((codeName == LightCodeName.DAY && !flag) || (codeName == LightCodeName.NIGHT && flag))
		{
			Travel();
		}
	}

	private void Travel()
	{
		if (_state == State.Standbye)
		{
			_firstShell = (_dayNightController.IsDay(_heads) ? _dayShell : _nightShell);
			_secondShell = (_dayNightController.IsDay(_heads) ? _nightShell : _dayShell);
			_state = State.ClosingShell;
			_attachPoint.transform.position = Locator.GetPlayerTransform().position;
			_attachPoint.transform.rotation = Locator.GetPlayerTransform().rotation;
			_attachPoint.AttachPlayer();
			base.enabled = true;
		}
	}

	private void FixedUpdate()
	{
		switch (_state)
		{
		case State.ClosingShell:
		{
			float num7 = (_heads ? 180f : 0f);
			float num8 = Mathf.MoveTowards(_firstShell.localEulerAngles.z, num7, Time.deltaTime * 60f);
			if (OWMath.ApproxEquals(num8, num7))
			{
				num8 = num7;
				_state = State.Traveling;
			}
			_firstShell.localEulerAngles = new Vector3(0f, 0f, num8);
			break;
		}
		case State.Traveling:
		{
			float num3 = (_heads ? (-180f) : 0f);
			float num4 = _platform.localEulerAngles.z;
			if (num4 > 0f)
			{
				num4 -= 360f;
			}
			num4 = Mathf.MoveTowards(num4, num3, Time.deltaTime * 60f);
			float num5 = Mathf.InverseLerp(0f, num3, num4);
			if (OWMath.ApproxEquals(num4, num3))
			{
				num4 = num3;
				num5 = 1f;
				_state = State.OpeningShell;
			}
			_platform.localEulerAngles = new Vector3(0f, 0f, num4);
			float num6 = (_heads ? (0f - _height) : 0f);
			_vehicleRoot.localPosition = new Vector3(0f, num5 * num6, 0f);
			break;
		}
		case State.OpeningShell:
		{
			float num = (_heads ? 180f : 0f);
			float num2 = Mathf.MoveTowards(_secondShell.localEulerAngles.z, num, Time.deltaTime * 60f);
			if (OWMath.ApproxEquals(num2, num))
			{
				num2 = num;
				_state = State.Standbye;
				_heads = !_heads;
				base.enabled = false;
			}
			_secondShell.localEulerAngles = new Vector3(0f, 0f, num2);
			if (PlayerState.IsAttached())
			{
				_attachPoint.DetachPlayer();
			}
			break;
		}
		}
	}
}
