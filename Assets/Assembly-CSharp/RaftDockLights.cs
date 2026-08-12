using UnityEngine;

public class RaftDockLights : MonoBehaviour
{
	private const float FADE_DURATION = 0.1f;

	[SerializeField]
	private OWRenderer _upBeam;

	[SerializeField]
	private OWRenderer _upBulb;

	[SerializeField]
	private OWRenderer _downBeam;

	[SerializeField]
	private OWRenderer _downBulb;

	private float _upLightStatus;

	private float _downLightStatus;

	private float _upLightStatusTarget;

	private float _downLightStatusTarget;

	private float _transitionStartTime;

	private Color _origBeamColor;

	private Color _origBulbColor;

	private Color _origBulbEmissionColor;

	private bool _isFlood;

	private void Awake()
	{
		_origBeamColor = _upBeam.GetOriginalColor();
		_origBulbColor = _upBulb.GetOriginalColor();
		_origBulbEmissionColor = _upBulb.GetOriginalEmissionColor();
		base.enabled = false;
	}

	private void Update()
	{
		if (OWMath.ApproxEquals(_upLightStatus, _upLightStatusTarget) && OWMath.ApproxEquals(_downLightStatus, _downLightStatusTarget))
		{
			base.enabled = false;
			return;
		}
		float t = Mathf.Clamp01((Time.time - _transitionStartTime) / 0.1f);
		_upLightStatus = Mathf.Lerp(_upLightStatus, _upLightStatusTarget, t);
		_downLightStatus = Mathf.Lerp(_downLightStatus, _downLightStatusTarget, t);
		SetMaterialColors();
	}

	private void SetMaterialColors()
	{
		Color origBeamColor = _origBeamColor;
		origBeamColor.a = _origBeamColor.a * _upLightStatus;
		_upBeam.SetColor(origBeamColor);
		Color color = Color.Lerp(Color.black, _origBulbColor, _upLightStatus);
		Color emissionColor = Color.Lerp(Color.black, _origBulbEmissionColor, _upLightStatus);
		_upBulb.SetColor(color);
		_upBulb.SetEmissionColor(emissionColor);
		Color origBeamColor2 = _origBeamColor;
		origBeamColor2.a = _origBeamColor.a * _downLightStatus;
		_downBeam.SetColor(origBeamColor2);
		Color color2 = Color.Lerp(Color.black, _origBulbColor, _downLightStatus);
		Color emissionColor2 = Color.Lerp(Color.black, _origBulbEmissionColor, _downLightStatus);
		_downBulb.SetColor(color2);
		_downBulb.SetEmissionColor(emissionColor2);
	}

	public void SetLightsActivation(bool occupied, bool instant = false)
	{
		_upLightStatusTarget = (occupied ? 1 : 0);
		if (!_isFlood)
		{
			_downLightStatusTarget = ((!occupied) ? 1 : 0);
		}
		else
		{
			_downLightStatusTarget = 0f;
		}
		_transitionStartTime = Time.time;
		if (!instant)
		{
			base.enabled = true;
			return;
		}
		_upLightStatus = _upLightStatusTarget;
		_downLightStatus = _downLightStatusTarget;
		SetMaterialColors();
	}

	public void SetFlood(bool flood)
	{
		_isFlood = flood;
	}
}
