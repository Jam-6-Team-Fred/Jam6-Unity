using UnityEngine;

public class EchoesOverController : MonoBehaviour
{
	[SerializeField]
	private Canvas _echoesTitleCanvas;

	[SerializeField]
	private CanvasGroupAnimator _echoesTitleAnimator;

	[SerializeField]
	private AnimationCurve _fadeCurve;

	private OWCamera _flashbackCamera;

	private AudioListener _audioListener;

	private bool _fadeInStarted;

	private bool _fadeOutStarted;

	private float _fadeInTime = float.PositiveInfinity;

	private float _fadeOutTime = float.PositiveInfinity;

	private float _flashbackTime = float.PositiveInfinity;

	private void Awake()
	{
		_flashbackCamera = GetComponent<OWCamera>();
		_audioListener = GetComponentInChildren<AudioListener>();
		GlobalMessenger.AddListener("TriggerEndOfDLC", OnTriggerEndOfDLC);
	}

	private void Start()
	{
		base.enabled = false;
		_echoesTitleCanvas.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("TriggerEndOfDLC", OnTriggerEndOfDLC);
	}

	private void Update()
	{
		if (!_fadeInStarted && Time.time >= _fadeInTime)
		{
			SetupCamera();
			_fadeInStarted = true;
			_fadeOutTime = Time.time + 2f + 2f;
			_echoesTitleAnimator.AnimateTo(1f, Vector3.one, 2f, _fadeCurve);
		}
		else if (!_fadeOutStarted && Time.time > _fadeOutTime)
		{
			_fadeOutStarted = true;
			_echoesTitleAnimator.AnimateTo(0f, Vector3.one, 2f, _fadeCurve, invertCurve: true);
			_flashbackTime = Time.time + 2f;
		}
		else if (Time.time >= _flashbackTime)
		{
			base.enabled = false;
			GlobalMessenger.FireEvent("TriggerFlashback");
		}
	}

	private void OnTriggerEndOfDLC()
	{
		_echoesTitleCanvas.gameObject.SetActive(value: true);
		_echoesTitleAnimator.SetImmediate(0f, Vector3.one);
		_fadeInTime = Time.time + 2f;
		base.enabled = true;
	}

	private void SetupCamera()
	{
		CenterOfTheUniverse.DeactivateUniverse();
		base.transform.position = Vector3.zero;
		Locator.GetActiveCamera().enabled = false;
		_flashbackCamera.clearFlags = CameraClearFlags.Color;
		_flashbackCamera.enabled = true;
		_flashbackCamera.postProcessing.enabled = false;
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _flashbackCamera);
		_audioListener.enabled = true;
	}
}
