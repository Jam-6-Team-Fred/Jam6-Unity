using UnityEngine;

public class StatueEyeAnimator : MonoBehaviour
{
	[SerializeField]
	private TransformAnimator[] _upperLidAnimators;

	[SerializeField]
	private TransformAnimator[] _lowerLidAnimators;

	private void Start()
	{
		for (int i = 0; i < _lowerLidAnimators.Length; i++)
		{
			_lowerLidAnimators[i].transform.Rotate(Vector3.up, -33f);
		}
		for (int j = 0; j < _upperLidAnimators.Length; j++)
		{
			_upperLidAnimators[j].transform.Rotate(Vector3.up, 35f);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.J))
		{
			OpenEyes();
		}
	}

	private void OpenEyes()
	{
		for (int i = 0; i < _lowerLidAnimators.Length; i++)
		{
			_lowerLidAnimators[i].RotateToOriginalLocalRotation(1f);
		}
		for (int j = 0; j < _upperLidAnimators.Length; j++)
		{
			_upperLidAnimators[j].RotateToOriginalLocalRotation(1f);
		}
	}
}
