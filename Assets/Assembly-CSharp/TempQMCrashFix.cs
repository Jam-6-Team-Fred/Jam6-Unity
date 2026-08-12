using UnityEngine;

public class TempQMCrashFix : MonoBehaviour
{
	[SerializeField]
	private float _radius = 100f;

	[SerializeField]
	private int _frameDelay = 5;

	private Transform _playerTransform;

	private ProxyShadowLight _proxyShadowLight;

	private bool _fixActive;

	private int _numFrames;

	private void Start()
	{
		_playerTransform = Locator.GetPlayerTransform();
		_proxyShadowLight = Locator.GetSunTransform().GetComponentInChildren<ProxyShadowLight>();
		if (_playerTransform == null || _proxyShadowLight == null)
		{
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		bool flag = (_playerTransform.position - base.transform.position).sqrMagnitude <= _radius * _radius;
		if (flag)
		{
			if (!_fixActive)
			{
				_proxyShadowLight.enabled = false;
				_fixActive = true;
			}
			_numFrames = 0;
		}
		else if (!flag)
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
