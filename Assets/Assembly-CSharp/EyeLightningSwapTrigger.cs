using UnityEngine;

public class EyeLightningSwapTrigger : MonoBehaviour
{
	[SerializeField]
	private GameObject _quantumLightningObjectRoot;

	[SerializeField]
	private CloudLightningGenerator[] _lightningGenerators;

	[SerializeField]
	private OWLight _ambientLight;

	[SerializeField]
	private OWLight _cloudEdgeLight;

	private QuantumLightningObject[] _quantumLightningObjects;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		_quantumLightningObjects = _quantumLightningObjectRoot.GetComponentsInChildren<QuantumLightningObject>();
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			for (int i = 0; i < _lightningGenerators.Length; i++)
			{
				_lightningGenerators[i].enabled = false;
			}
			for (int j = 0; j < _quantumLightningObjects.Length; j++)
			{
				_quantumLightningObjects[j].SetActivation(active: true);
			}
			_ambientLight.FadeTo(0f, 5f);
			_cloudEdgeLight.FadeTo(0f, 5f);
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			for (int i = 0; i < _lightningGenerators.Length; i++)
			{
				_lightningGenerators[i].enabled = true;
			}
			for (int j = 0; j < _quantumLightningObjects.Length; j++)
			{
				_quantumLightningObjects[j].SetActivation(active: false);
			}
			_ambientLight.FadeTo(1f, 5f);
			_cloudEdgeLight.FadeTo(1f, 5f);
		}
	}
}
