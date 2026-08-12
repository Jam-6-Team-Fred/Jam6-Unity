using UnityEngine;

public class QuantumFoamTrigger : MonoBehaviour
{
	[SerializeField]
	private Transform _playerWarpPoint;

	[SerializeField]
	private OWTriggerVolume _observatoryVolume;

	private OWRigidbody _playerBody;

	private OWTriggerVolume _triggerVolume;

	private void Awake()
	{
		_triggerVolume = GetComponent<OWTriggerVolume>();
		_triggerVolume.OnEntry += OnEntry;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEntry;
	}

	private void OnEntry(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			base.enabled = true;
			_playerBody = hitObject.GetAttachedOWRigidbody();
			if (Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe() != null)
			{
				Object.Destroy(Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe()
					.gameObject);
					Debug.Log("PROBE DESTROYED (LEFT BEHIND)");
				}
			}
		}

		private void FixedUpdate()
		{
			if (_playerBody != null)
			{
				OWRigidbody attachedOWRigidbody = _playerWarpPoint.GetAttachedOWRigidbody();
				_playerBody.WarpToPositionRotation(_playerWarpPoint.position, _playerWarpPoint.rotation);
				_playerBody.SetVelocity(attachedOWRigidbody.GetPointVelocity(_playerWarpPoint.position) - 5f * _playerWarpPoint.up);
				Locator.GetFlashlight().TurnOff(playAudio: false);
				_observatoryVolume.AddObjectToVolume(Locator.GetPlayerDetector());
				_observatoryVolume.AddObjectToVolume(Locator.GetPlayerCameraDetector());
				_playerBody = null;
				RumbleManager.PulseLightImpact();
				if (Locator.GetShipLogManager().IsFactRevealed("IP_RING_WORLD_X1"))
				{
					DialogueConditionManager.SharedInstance.SetConditionState("EnteredIP", conditionState: true);
				}
				Locator.GetEyeStateManager().SetState(EyeState.Observatory);
			}
			base.enabled = false;
		}
	}
