using UnityEngine;

public class MapSatelliteOrbitLine : OrbitLine
{
	[Space]
	[SerializeField]
	private ReferenceFrameVolume _rfVolume;

	[SerializeField]
	private float _lockOnFadeLength = 1f;

	[SerializeField]
	private float _minAlpha;

	private bool _isLockedOn;

	private float _lockOnFade;

	protected override void Awake()
	{
		base.Awake();
		GlobalMessenger<ReferenceFrame>.AddListener("TargetReferenceFrame", OnTargetReferenceFrame);
		GlobalMessenger.AddListener("UntargetReferenceFrame", OnUntargetReferenceFrame);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger<ReferenceFrame>.RemoveListener("TargetReferenceFrame", OnTargetReferenceFrame);
		GlobalMessenger.RemoveListener("UntargetReferenceFrame", OnUntargetReferenceFrame);
	}

	protected override void OnEnterMapView()
	{
		base.OnEnterMapView();
		_lockOnFade = (_isLockedOn ? 1f : _minAlpha);
	}

	protected override void Update()
	{
		_lockOnFade = Mathf.MoveTowards(_lockOnFade, _isLockedOn ? 1f : _minAlpha, Time.deltaTime / _lockOnFadeLength);
		AstroObject astroObject = ((_astroObject != null) ? _astroObject.GetPrimaryBody() : null);
		if (astroObject == null)
		{
			base.enabled = false;
			return;
		}
		Vector3 vector = _astroObject.transform.position - astroObject.transform.position;
		Vector3 normalized = Vector3.Cross(astroObject.GetAttachedOWRigidbody().GetRelativeVelocity(_astroObject.GetAttachedOWRigidbody()), vector).normalized;
		float magnitude = vector.magnitude;
		base.transform.position = astroObject.transform.position;
		base.transform.rotation = Quaternion.LookRotation(vector, normalized);
		base.transform.localScale = Vector3.one * magnitude;
		float num = CalcFadeDistance();
		float widthMultiplier = Mathf.Min(num * (_lineWidth / 1000f), _maxLineWidth);
		float num2 = CalcFade(num);
		_lineRenderer.widthMultiplier = widthMultiplier;
		_lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, _color.a * num2 * num2);
	}

	private float CalcFadeDistance()
	{
		OWCamera activeCamera = Locator.GetActiveCamera();
		if (activeCamera.CompareTag("MapCamera"))
		{
			Vector3 position = activeCamera.transform.position;
			Vector3 position2 = Locator.GetAstroObject(AstroObject.Name.Sun).GetOWRigidbody().transform.position;
			return Vector3.Distance(position, position2);
		}
		return 0f;
	}

	private void OnTargetReferenceFrame(ReferenceFrame referenceFrame)
	{
		_isLockedOn = referenceFrame == _rfVolume.GetReferenceFrame();
	}

	private void OnUntargetReferenceFrame()
	{
		_isLockedOn = false;
	}
}
