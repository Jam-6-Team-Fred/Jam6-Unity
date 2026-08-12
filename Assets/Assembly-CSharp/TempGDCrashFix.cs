using UnityEngine;

public class TempGDCrashFix : MonoBehaviour
{
	[SerializeField]
	private float _radius = 1000f;

	[SerializeField]
	private int _frameDelay = 5;

	private OWCamera _landingCamera;

	private Transform _playerTransform;

	private ProxyShadowLight _proxyShadowLight;

	private bool _fixActive;

	private int _numFrames;

	private void Start()
	{
		_landingCamera = base.gameObject.FindWithRequiredTag("LandingCamera").GetComponent<OWCamera>();
		_playerTransform = Locator.GetPlayerTransform();
		_proxyShadowLight = Locator.GetSunTransform().GetComponentInChildren<ProxyShadowLight>();
		if (_landingCamera == null || _playerTransform == null || _proxyShadowLight == null)
		{
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if ((_playerTransform.position - base.transform.position).sqrMagnitude <= _radius * _radius && _landingCamera.enabled)
		{
			if (!_fixActive)
			{
				_proxyShadowLight.enabled = false;
				_fixActive = true;
			}
			_numFrames = 0;
		}
		else
		{
			if (_fixActive && _numFrames >= _frameDelay)
			{
				_proxyShadowLight.enabled = true;
				_fixActive = false;
			}
			_numFrames++;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, _radius);
		}
	}
}
