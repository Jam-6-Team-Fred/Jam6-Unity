using UnityEngine;

public class ShadowcasterFloodToggle : MonoBehaviour
{
	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private Transform[] _casterTransforms;

	private int _enableFrameCount;

	private ProxyShadowCaster[] _casters;

	private void Awake()
	{
		_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
	}

	private void Start()
	{
		_casters = new ProxyShadowCaster[_casterTransforms.Length];
		for (int i = 0; i < _casters.Length; i++)
		{
			_casters[i] = _casterTransforms[i].GetComponent<ProxyShadowCaster>();
		}
		base.enabled = false;
	}

	private void Update()
	{
		if (_enableFrameCount >= 1)
		{
			for (int i = 0; i < _casters.Length; i++)
			{
				_casters[i].SetDynamic(dynamic: false);
			}
			base.enabled = false;
			_enableFrameCount = 0;
		}
		_enableFrameCount++;
	}

	private void OnFloodImpact()
	{
		_enableFrameCount = 0;
		base.enabled = true;
		for (int i = 0; i < _casters.Length; i++)
		{
			_casters[i].SetDynamic(dynamic: true);
		}
	}
}
