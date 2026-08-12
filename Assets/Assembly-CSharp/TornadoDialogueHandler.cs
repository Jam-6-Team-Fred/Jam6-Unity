using UnityEngine;

[RequireComponent(typeof(CharacterDialogueTree))]
public class TornadoDialogueHandler : MonoBehaviour
{
	private CharacterDialogueTree _dialogueTree;

	private IslandController _islandController;

	private void Start()
	{
		_dialogueTree = GetComponent<CharacterDialogueTree>();
		_islandController = base.gameObject.GetAttachedOWRigidbody().GetComponent<IslandController>();
		if (_islandController != null)
		{
			_islandController.OnIslandEnteredTornadoEvent += OnEnterTornado;
			_islandController.OnIslandSplashEvent += OnSplashDown;
		}
	}

	private void OnDestroy()
	{
		if (_islandController != null)
		{
			_islandController.OnIslandEnteredTornadoEvent -= OnEnterTornado;
			_islandController.OnIslandSplashEvent -= OnSplashDown;
		}
	}

	private void OnEnterTornado()
	{
		_dialogueTree.EndConversation();
		_dialogueTree.GetComponent<InteractVolume>().DisableInteraction();
	}

	private void OnSplashDown()
	{
		_dialogueTree.GetComponent<InteractVolume>().EnableInteraction();
	}
}
