using UnityEngine;

public class GravityFlipperController : MonoBehaviour
{
	[SerializeField]
	private float _extendedScale = 10f;

	[SerializeField]
	private float _defaultScale = 2f;

	[SerializeField]
	private OWTriggerVolume _gravityVolume;

	private int _occupantCount;

	private void Awake()
	{
		_gravityVolume.OnEntry += OnEntry;
		_gravityVolume.OnExit += OnExit;
	}

	private void OnDestroy()
	{
		_gravityVolume.OnEntry -= OnEntry;
		_gravityVolume.OnExit -= OnExit;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") || hitObj.CompareTag("ProbeDetector") || hitObj.CompareTag("DynamicPropDetector"))
		{
			_occupantCount++;
			if (_occupantCount == 1)
			{
				_gravityVolume.transform.localScale = new Vector3(1f, _extendedScale, 1f);
			}
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") || hitObj.CompareTag("ProbeDetector") || hitObj.CompareTag("DynamicPropDetector"))
		{
			_occupantCount--;
			if (_occupantCount == 0)
			{
				_gravityVolume.transform.localScale = new Vector3(1f, _defaultScale, 1f);
			}
		}
	}
}
