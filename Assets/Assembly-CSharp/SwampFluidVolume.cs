using UnityEngine;

public class SwampFluidVolume : FlatFluidVolume_Old
{
	[Space]
	[SerializeField]
	private float _depth;

	[SerializeField]
	private float _heightChange;

	[SerializeField]
	private float _changeOverDegrees;

	[SerializeField]
	private bool _previewNightOnHeads;

	[Space]
	[SerializeField]
	private Transform _surfaceHeads;

	[SerializeField]
	private Transform _surfaceTails;

	private DayNightPlanetController _dayNightController;

	private void OnValidate()
	{
		if (_boxShape == null)
		{
			_boxShape = GetComponent<BoxShape>();
		}
		if (!OWMath.ApproxEquals(_boxShape.size.y, _depth))
		{
			_boxShape.size = new Vector3(_boxShape.size.x, _depth, _boxShape.size.z);
		}
		float num = _heightChange * 0.5f * (float)(_previewNightOnHeads ? 1 : (-1));
		if (!OWMath.ApproxEquals(_boxShape.center.y, num))
		{
			_boxShape.center = new Vector3(0f, num, 0f);
		}
		if (_surfaceHeads != null && !OWMath.ApproxEquals(_surfaceHeads.localPosition.y, GetLocalSurfaceYPos(heads: true)))
		{
			_surfaceHeads.localPosition = new Vector3(0f, GetLocalSurfaceYPos(heads: true), 0f);
		}
		if (_surfaceTails != null && !OWMath.ApproxEquals(_surfaceTails.localPosition.y, GetLocalSurfaceYPos(heads: false)))
		{
			_surfaceTails.localPosition = new Vector3(0f, GetLocalSurfaceYPos(heads: false), 0f);
		}
	}

	protected override void Start()
	{
		base.Start();
		_dayNightController = Locator.GetAstroObject(AstroObject.Name.RingWorld).GetComponent<DayNightPlanetController>();
		float y = _heightChange * 0.5f * (float)(_dayNightController.IsDay(heads: false) ? 1 : (-1));
		_boxShape.center = new Vector3(0f, y, 0f);
		UpdateWaterSurfacePosition();
	}

	private void FixedUpdate()
	{
		float sunAngle = _dayNightController.GetSunAngle();
		float num = _changeOverDegrees * 0.5f;
		float a = 90f - num;
		float b = 90f + num;
		float a2 = -90f - num;
		float b2 = -90f + num;
		float num2 = ((!(sunAngle > 0f)) ? (1f - 2f * Mathf.InverseLerp(a2, b2, sunAngle)) : (-1f + 2f * Mathf.InverseLerp(a, b, sunAngle)));
		float y = _heightChange * 0.5f * num2;
		_boxShape.center = new Vector3(0f, y, 0f);
		UpdateWaterSurfacePosition();
	}

	private void UpdateWaterSurfacePosition()
	{
		_surfaceHeads.localPosition = new Vector3(0f, GetLocalSurfaceYPos(heads: true), 0f);
		_surfaceTails.localPosition = new Vector3(0f, GetLocalSurfaceYPos(heads: false), 0f);
	}
}
