using UnityEngine;

public class TestSector : MonoBehaviour
{
	[SerializeField]
	private Transform[] _detectors;

	[SerializeField]
	private float _radius;

	private float _radiusSqr;

	private Transform _transform;

	private void Awake()
	{
		_transform = base.transform;
		_radiusSqr = _radius * _radius;
		InvokeRepeating("DistanceCheck", 0f, 0.25f);
	}

	private void DistanceCheck()
	{
		for (int i = 0; i < _detectors.Length; i++)
		{
			_ = (_transform.position - _detectors[i].position).sqrMagnitude;
			_ = _radiusSqr;
		}
	}
}
