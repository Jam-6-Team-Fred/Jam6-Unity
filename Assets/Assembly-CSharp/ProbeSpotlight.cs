using UnityEngine;

[RequireComponent(typeof(OWLight2))]
public class ProbeSpotlight : MonoBehaviour
{
	private SurveyorProbe _probe;

	private OWLight2 _light;

	[SerializeField]
	private ProbeCamera.ID _id;

	[SerializeField]
	private float _fadeInLength = 1f;

	private bool _inFlight;

	private float _intensity;

	private float _timer;

	private void Awake()
	{
		_probe = this.GetAttachedOWRigidbody().GetRequiredComponent<SurveyorProbe>();
		_light = GetComponent<OWLight2>();
		_intensity = _light.GetLight().intensity;
		_light.SetActivation(active: false);
		base.enabled = false;
		_probe.OnLaunchProbe += OnLaunch;
		_probe.OnAnchorProbe += OnAnchorOrRetrieve;
		_probe.OnRetrieveProbe += OnAnchorOrRetrieve;
		GlobalMessenger<ProbeCamera>.AddListener("ProbeSnapshot", OnTakeSnapshot);
	}

	private void OnDestroy()
	{
		_probe.OnLaunchProbe -= OnLaunch;
		_probe.OnAnchorProbe -= OnAnchorOrRetrieve;
		_probe.OnRetrieveProbe -= OnAnchorOrRetrieve;
		GlobalMessenger<ProbeCamera>.RemoveListener("ProbeSnapshot", OnTakeSnapshot);
	}

	private void Update()
	{
		_timer += Time.deltaTime;
		float num = Mathf.Clamp01(_timer / _fadeInLength);
		float intensityScale = (2f - num) * num * _intensity;
		_light.SetIntensityScale(intensityScale);
	}

	private void StartFadeIn()
	{
		if (!base.enabled)
		{
			_light.SetActivation(active: true);
			_light.SetIntensityScale(0f);
			_timer = 0f;
			base.enabled = true;
		}
	}

	private void OnLaunch()
	{
		if (_id == ProbeCamera.ID.Forward)
		{
			StartFadeIn();
		}
		_inFlight = true;
	}

	private void OnAnchorOrRetrieve()
	{
		_light.SetActivation(active: false);
		base.enabled = false;
		_inFlight = false;
	}

	private void OnTakeSnapshot(ProbeCamera probeCamera)
	{
		if (_inFlight)
		{
			if (probeCamera.GetID() == _id)
			{
				StartFadeIn();
				return;
			}
			_light.SetActivation(active: false);
			base.enabled = false;
		}
	}
}
