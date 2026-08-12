using UnityEngine;

public class DialogueAttentionPointSwapper : MonoBehaviour
{
	[SerializeField]
	private CharacterDialogueTree _dialogueTree;

	[SerializeField]
	private string _nodeName = "";

	[SerializeField]
	private int _dialoguePage;

	[Space]
	[SerializeField]
	private Transform _attentionPoint;

	[SerializeField]
	private Vector3 _attentionPointOffset = Vector3.zero;

	[SerializeField]
	private float _lookEasing = 1f;

	private void Awake()
	{
		_dialogueTree.OnAdvancePage += OnAdvancePage;
	}

	private void OnDestroy()
	{
		_dialogueTree.OnAdvancePage -= OnAdvancePage;
	}

	private void OnAdvancePage(string nodeName, int pageNum)
	{
		if ((string.IsNullOrEmpty(_nodeName) || nodeName == _nodeName) && pageNum == _dialoguePage)
		{
			Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(_attentionPoint, _attentionPointOffset, _lookEasing);
		}
	}
}
