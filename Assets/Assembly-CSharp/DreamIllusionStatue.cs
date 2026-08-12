using UnityEngine;

public class DreamIllusionStatue : MonoBehaviour
{
	[SerializeField]
	private DitheringAnimator _cloakAnimator;

	[SerializeField]
	private LensFlare _lensFlare;

	[SerializeField]
	private Transform _sightOrigin;

	[SerializeField]
	private OWLightController _lightController;

	private bool _isPlayerVisible;

	private void Awake()
	{
	}

	private void Start()
	{
		_lensFlare.enabled = false;
		_lensFlare.brightness = 0f;
		_lightController.SetIntensity(0f);
		_cloakAnimator.SetVisibleImmediate(visible: false);
	}

	private void OnDestroy()
	{
	}

	private void FixedUpdate()
	{
		bool isPlayerVisible = _isPlayerVisible;
		Vector3 direction = Locator.GetPlayerCamera().transform.position - _sightOrigin.position;
		float magnitude = direction.magnitude;
		RaycastHit hitInfo;
		if (magnitude > 30f)
		{
			_isPlayerVisible = false;
		}
		else if (Physics.Raycast(_sightOrigin.position + direction.normalized * 0.5f, direction, out hitInfo, magnitude, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore))
		{
			_isPlayerVisible = hitInfo.collider.CompareTag("Player");
		}
		else
		{
			_isPlayerVisible = true;
		}
		if (_isPlayerVisible && !isPlayerVisible)
		{
			_lightController.FadeTo(1f, 0.5f);
			_cloakAnimator.SetVisible(visible: false, 4f);
		}
		else if (isPlayerVisible && !_isPlayerVisible)
		{
			_lightController.FadeTo(0f, 0.5f);
			_cloakAnimator.SetVisible(visible: true, 4f);
		}
	}

	private void Update()
	{
		float target = (_isPlayerVisible ? 1f : 0f);
		_lensFlare.brightness = Mathf.MoveTowards(_lensFlare.brightness, target, Time.deltaTime * 4f);
	}
}
