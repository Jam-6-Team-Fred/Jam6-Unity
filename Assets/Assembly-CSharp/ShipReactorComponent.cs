using UnityEngine;

public class ShipReactorComponent : ShipComponent
{
	[Space(10f)]
	[SerializeField]
	private float _minCountdown = 25f;

	[SerializeField]
	private float _maxCountdown = 35f;

	[Space]
	[SerializeField]
	private Transform _timerArrow;

	[SerializeField]
	private float _startArrowRotation = 30f;

	[SerializeField]
	private float _endArrowRotation = -90f;

	private ShipDamageController _shipDamageController;

	private float _criticalCountdown;

	private float _criticalTimer;

	protected override void Awake()
	{
		base.Awake();
		_shipDamageController = GetComponentInParent<ShipDamageController>();
	}

	private void Update()
	{
		if (_damaged)
		{
			_criticalTimer -= Time.deltaTime;
			if (_criticalTimer <= 0f)
			{
				_shipDamageController.Explode();
				base.enabled = false;
				return;
			}
			float t = 1f - _criticalTimer / _criticalCountdown;
			float num = Mathf.LerpAngle(_startArrowRotation, _endArrowRotation, t);
			float num2 = Mathf.PerlinNoise(Time.time * 10f, Time.time * 10f) * 2f - 1f;
			num += num2 * 5f;
			_timerArrow.localEulerAngles = new Vector3(num, 0f, 0f);
		}
		else
		{
			float num3 = Mathf.MoveTowardsAngle(_timerArrow.localEulerAngles.x, _startArrowRotation, 90f * Time.deltaTime);
			_timerArrow.localEulerAngles = new Vector3(num3, 0f, 0f);
			if (OWMath.ApproxEquals(num3, _startArrowRotation))
			{
				base.enabled = false;
			}
		}
	}

	protected override void OnComponentDamaged()
	{
		_criticalCountdown = Random.Range(_minCountdown, _maxCountdown);
		_criticalTimer = _criticalCountdown;
		base.enabled = true;
	}
}
