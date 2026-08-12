using UnityEngine;

public class MatchTransform : MonoBehaviour
{
	[SerializeField]
	private Transform _targetTransform;

	[SerializeField]
	private bool _matchPosition = true;

	[SerializeField]
	private bool _matchRotation;

	[SerializeField]
	private bool _matchLocal;

	[SerializeField]
	private bool _doNotReparent;

	private void Awake()
	{
		if (!_doNotReparent)
		{
			base.transform.parent = base.transform.root;
		}
	}

	private void Update()
	{
		if (_matchLocal)
		{
			if (_matchPosition)
			{
				base.transform.localPosition = _targetTransform.localPosition;
			}
			if (_matchRotation)
			{
				base.transform.localRotation = _targetTransform.localRotation;
			}
		}
		else
		{
			if (_matchPosition)
			{
				base.transform.position = _targetTransform.position;
			}
			if (_matchRotation)
			{
				base.transform.rotation = _targetTransform.rotation;
			}
		}
	}
}
