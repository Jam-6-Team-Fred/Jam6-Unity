using UnityEngine;

public class FacePlayerWhenTalking : MonoBehaviour
{
	private CharacterDialogueTree _dialogueTree;

	private Quaternion _origLocalRotation;

	private Quaternion _targetLocalRotation;

	private void Awake()
	{
		_dialogueTree = GetComponentInChildren<CharacterDialogueTree>();
		if (_dialogueTree != null)
		{
			_dialogueTree.OnStartConversation += OnStartConversation;
			_dialogueTree.OnEndConversation += OnEndConversation;
		}
		_origLocalRotation = base.transform.localRotation;
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_dialogueTree != null)
		{
			_dialogueTree.OnStartConversation -= OnStartConversation;
			_dialogueTree.OnEndConversation -= OnEndConversation;
		}
	}

	private void OnStartConversation()
	{
		Vector3 vector = Locator.GetPlayerTransform().position - base.transform.position;
		Vector3 vector2 = vector - Vector3.Project(vector, base.transform.up);
		float angle = Vector3.Angle(base.transform.forward, vector2) * Mathf.Sign(Vector3.Dot(vector2, base.transform.right));
		Vector3 axis = base.transform.parent.InverseTransformDirection(base.transform.up);
		Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
		FaceLocalRotation(quaternion * base.transform.localRotation);
	}

	private void OnEndConversation()
	{
		FaceLocalRotation(_origLocalRotation);
	}

	private void FaceLocalRotation(Quaternion targetLocalRotation)
	{
		base.enabled = true;
		_targetLocalRotation = targetLocalRotation;
	}

	private void Update()
	{
		base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, _targetLocalRotation, 0.1f);
		if (Mathf.Abs(Quaternion.Angle(base.transform.localRotation, _targetLocalRotation)) < 1f)
		{
			base.enabled = false;
		}
	}
}
