using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ShipGroanAudioController : MonoBehaviour
{
	[SerializeField]
	private float _jerkMagnitudeGroanThreshold = 30f;

	[SerializeField]
	private float _jerkChargeTimeThreshold = 5f;

	[SerializeField]
	private float _secondsPreventGroanAfterThrusterFire = 5f;

	private float _groanChargeFraction;

	private OWAudioSource _audioSource;

	private OWRigidbody _shipBody;

	private ShipThrusterModel _shipThrusters;

	private float _timeLastThrustersFired = float.MinValue;

	private Queue<Vector3> _cachedJerks;

	private const int c_cachedJerkSize = 30;

	private void Start()
	{
		_cachedJerks = new Queue<Vector3>(32);
		_audioSource = this.GetRequiredComponent<OWAudioSource>();
		_shipBody = Locator.GetShipBody();
		_shipThrusters = _shipBody.GetComponentInChildren<ShipThrusterModel>();
	}

	private void Update()
	{
		Vector3 zero = Vector3.zero;
		if (Time.timeSinceLevelLoad > 1f)
		{
			zero = _shipBody.GetJerk();
			_cachedJerks.Enqueue(zero);
			if (_cachedJerks.Count > 30)
			{
				_cachedJerks.Dequeue();
			}
			Vector3 zero2 = Vector3.zero;
			Queue<Vector3>.Enumerator enumerator = _cachedJerks.GetEnumerator();
			while (enumerator.MoveNext())
			{
				zero2 += enumerator.Current;
			}
			if ((zero2 / 30f).magnitude > _jerkMagnitudeGroanThreshold)
			{
				_groanChargeFraction += Time.deltaTime / _jerkChargeTimeThreshold;
			}
		}
		if (_shipThrusters.IsTranslationalThrusterFiring())
		{
			_timeLastThrustersFired = Time.time;
		}
		if (_groanChargeFraction >= 1f && Time.time - _timeLastThrustersFired > _secondsPreventGroanAfterThrusterFire)
		{
			_audioSource.PlayOneShot(AudioType.ShipHullGroan);
			_groanChargeFraction = 0f;
		}
	}
}
