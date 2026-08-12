using UnityEngine;

public class ShipTractorBeamSwitch : MonoBehaviour
{
	[SerializeField]
	private FluidVolume _beamFluid;

	[SerializeField]
	private ParticleSystem[] _particleSystems = new ParticleSystem[0];

	[SerializeField]
	private Light[] _lights = new Light[0];

	private bool _isPlayerInShip;

	private bool _functional = true;

	private void Awake()
	{
		GetComponent<Collider>().isTrigger = true;
		base.gameObject.layer = LayerMask.NameToLayer("AdvancedEffectVolume");
		GlobalMessenger.AddListener("EnterShip", OnEnterShip);
		GlobalMessenger.AddListener("ExitShip", OnExitShip);
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipSystemFailure);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterShip", OnEnterShip);
		GlobalMessenger.RemoveListener("ExitShip", OnExitShip);
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipSystemFailure);
	}

	public void ActivateTractorBeam()
	{
		_beamFluid.SetVolumeActivation(active: true);
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			_particleSystems[i].Play();
		}
		for (int j = 0; j < _lights.Length; j++)
		{
			_lights[j].enabled = true;
		}
	}

	public void DeactivateTractorBeam()
	{
		_beamFluid.SetVolumeActivation(active: false);
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			_particleSystems[i].Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		for (int j = 0; j < _lights.Length; j++)
		{
			_lights[j].enabled = false;
		}
	}

	private void OnEnterShip()
	{
		_isPlayerInShip = true;
		DeactivateTractorBeam();
	}

	private void OnExitShip()
	{
		_isPlayerInShip = false;
	}

	private void OnShipSystemFailure()
	{
		_functional = false;
		DeactivateTractorBeam();
	}

	private void OnTriggerExit(Collider hitCollider)
	{
		if (!_isPlayerInShip && _functional && hitCollider.CompareTag("PlayerDetector"))
		{
			ActivateTractorBeam();
		}
	}
}
