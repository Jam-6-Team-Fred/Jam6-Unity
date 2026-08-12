using UnityEngine;

[RequireComponent(typeof(SphereShape))]
[RequireComponent(typeof(OWTriggerVolume))]
public class QuantumDarkTrigger : MonoBehaviour
{
	private SphereShape _sphereShape;

	private OWTriggerVolume _trigger;

	private bool _isPlayerInside;

	private void Awake()
	{
		_sphereShape = GetComponent<SphereShape>();
		_trigger = GetComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	public bool IsPlayerInDarkness()
	{
		bool flag = false;
		if (Locator.GetProbe() != null && Locator.GetProbe().IsLaunched())
		{
			flag = Locator.GetProbe().CheckIlluminationAtPoint(base.transform.position, _sphereShape.radius);
		}
		if (_isPlayerInside && !PlayerState.IsFlashlightOn())
		{
			return !flag;
		}
		return false;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_isPlayerInside = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_isPlayerInside = false;
		}
	}
}
