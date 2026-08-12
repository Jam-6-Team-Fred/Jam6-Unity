using UnityEngine;

public class LightLock : MonoBehaviour
{
	[SerializeField]
	private Material _glowMaterial;

	[SerializeField]
	private Material _origMaterial;

	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private Transform[] _lockTransforms;

	[SerializeField]
	private MeshRenderer[] _lockRenderers;

	private int _index;

	private void Awake()
	{
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void Update()
	{
		_lockTransforms[_index].Rotate(Vector3.up, 45f * Time.deltaTime, Space.Self);
	}

	private void OnDetectLight()
	{
		base.enabled = true;
		_lockRenderers[_index].material = _glowMaterial;
	}

	private void OnDetectDarkness()
	{
		base.enabled = false;
		_index++;
		if (_index > _lockTransforms.Length - 1)
		{
			_index = 0;
		}
		for (int i = 0; i < _lockTransforms.Length; i++)
		{
			_lockRenderers[i].material = _origMaterial;
		}
	}
}
