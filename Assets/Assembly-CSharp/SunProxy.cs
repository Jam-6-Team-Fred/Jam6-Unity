using UnityEngine;

public class SunProxy : MonoBehaviour
{
	private const float SUN_DIAMETER = 2000f;

	[SerializeField]
	private SunProxyEffectController _proxySunController;

	private Transform _sunTransform;

	private Transform _playerCameraTransform;

	private float _proxyAtan;

	private float _logSpaceLength;

	private SunController _realSunController;

	private bool _sunOutOfRange;

	private void Start()
	{
		_sunTransform = Locator.GetSunTransform();
		_playerCameraTransform = Locator.GetPlayerCamera().transform;
		_proxyAtan = Mathf.Atan(1f / 21f);
		_logSpaceLength = 4000f;
		_realSunController = _sunTransform.GetComponent<SunController>();
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		_proxySunController.SetRenderingEnabled(renderingEnabled: false);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
	}

	private void Update()
	{
		Vector3 position = _playerCameraTransform.position;
		Vector3 realVector = _sunTransform.position - position;
		bool sunOutOfRange = _sunOutOfRange;
		float sqrMagnitude = realVector.sqrMagnitude;
		_sunOutOfRange = sqrMagnitude > 1.764E+09f;
		if (_sunOutOfRange != sunOutOfRange)
		{
			_realSunController.SetRenderingEnabled(!_sunOutOfRange);
			_proxySunController.SetRenderingEnabled(_sunOutOfRange);
		}
		sqrMagnitude = Mathf.Sqrt(sqrMagnitude);
		base.transform.position = ProxyBody.GetProxyPosition(position, realVector, sqrMagnitude, 42000f, _logSpaceLength, out var resultDistance);
		float num = Mathf.Atan(2000f / sqrMagnitude);
		_proxyAtan = Mathf.Atan(2000f / resultDistance);
		float num2 = Mathf.Clamp01(num / _proxyAtan);
		_proxySunController.UpdateRayleighConstant(sqrMagnitude);
		base.transform.localScale = Vector3.one * num2;
	}

	private void OnEnterMapView()
	{
		if (_sunOutOfRange)
		{
			_realSunController.SetRenderingEnabled(renderingEnabled: true);
			_proxySunController.SetRenderingEnabled(renderingEnabled: false);
		}
		base.enabled = false;
	}

	private void OnExitMapView()
	{
		if (_sunOutOfRange)
		{
			_realSunController.SetRenderingEnabled(renderingEnabled: false);
			_proxySunController.SetRenderingEnabled(renderingEnabled: true);
		}
		base.enabled = true;
	}
}
