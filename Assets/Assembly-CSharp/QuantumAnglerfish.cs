using UnityEngine;

public class QuantumAnglerfish : MonoBehaviour
{
	[SerializeField]
	private VisibilityObject _anglerfishObject;

	[SerializeField]
	private Renderer _anglerfishRenderer;

	[SerializeField]
	private OWTriggerVolume _detectionTrigger;

	[SerializeField]
	private OWAudioSource _audioSource;

	private float _lureGlow;

	private bool _flickeringOut;

	private float _flickerOutTime;

	private void Awake()
	{
		_detectionTrigger.OnEntry += OnDetectPlayer;
		base.enabled = false;
		_anglerfishObject.gameObject.SetActive(value: false);
		_anglerfishRenderer.material.SetColor("_EmissionColor", new Color(_lureGlow, _lureGlow, _lureGlow));
	}

	private void OnDestroy()
	{
		_detectionTrigger.OnEntry -= OnDetectPlayer;
	}

	private void Update()
	{
		if (_flickeringOut && Time.time >= _flickerOutTime)
		{
			_anglerfishObject.gameObject.SetActive(value: false);
			base.enabled = false;
		}
		if (_anglerfishObject.IsNewlyObscured())
		{
			_anglerfishObject.gameObject.SetActive(value: false);
			base.enabled = false;
		}
	}

	private void FixedUpdate()
	{
		base.transform.LookAt(Locator.GetPlayerTransform());
		base.transform.position = Vector3.MoveTowards(base.transform.position, Locator.GetPlayerTransform().position, Time.deltaTime * 5f);
		_lureGlow = Mathf.MoveTowards(_lureGlow, 2f, Time.deltaTime);
		_anglerfishRenderer.material.SetColor("_EmissionColor", new Color(_lureGlow, _lureGlow, _lureGlow));
		if (!_flickeringOut && Vector3.Distance(Locator.GetPlayerCameraDetector().transform.position, _anglerfishObject.transform.position) < 15f)
		{
			_flickerOutTime = Time.time + 0.2f;
			_flickeringOut = true;
			GlobalMessenger<float, float>.FireEvent("FlickerOffAndOn", 0.2f, 2f);
		}
	}

	private void OnDetectPlayer(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = true;
			if (_audioSource != null)
			{
				_audioSource.PlayOneShot(AudioType.DBAnglerfishDetectDisturbance);
			}
			if (Locator.GetProbe() != null && Locator.GetProbe().IsLaunched())
			{
				Locator.GetProbe().FlickerOffAndOn(0f, 2f);
			}
			_anglerfishObject.gameObject.SetActive(value: true);
		}
	}
}
