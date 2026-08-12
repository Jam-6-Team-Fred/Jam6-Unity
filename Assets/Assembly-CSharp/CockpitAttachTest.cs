using UnityEngine;

public class CockpitAttachTest : MonoBehaviour
{
	private PlayerAttachPoint _playerAttachPoint;

	private SingleInteractionVolume _interactVolume;

	private bool _attached;

	private void Awake()
	{
		_interactVolume = this.GetRequiredComponent<SingleInteractionVolume>();
		_playerAttachPoint = this.GetRequiredComponent<PlayerAttachPoint>();
		_interactVolume.OnPressInteract += OnPressInteract;
	}

	private void OnDestroy()
	{
		_interactVolume.OnPressInteract -= OnPressInteract;
	}

	private void OnPressInteract()
	{
		if (_attached)
		{
			_playerAttachPoint.DetachPlayer();
		}
		else
		{
			_playerAttachPoint.AttachPlayer();
		}
		_attached = !_attached;
		_interactVolume.ResetInteraction();
	}
}
