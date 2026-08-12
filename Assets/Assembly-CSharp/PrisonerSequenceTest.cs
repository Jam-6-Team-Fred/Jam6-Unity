using UnityEngine;

public class PrisonerSequenceTest : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _sequenceTrigger;

	[SerializeField]
	private Animation _prisonerAnimation;

	private bool _darknessAwoken;

	private void Awake()
	{
		_sequenceTrigger.OnEntry += OnEnterSequenceTrigger;
	}

	private void Start()
	{
		_prisonerAnimation.Play("PrisonerTestSitting");
	}

	private void OnDestroy()
	{
		_sequenceTrigger.OnEntry -= OnEnterSequenceTrigger;
	}

	private void OnEnterSequenceTrigger(GameObject hitObj)
	{
		if (!_darknessAwoken && hitObj.CompareTag("PlayerDetector"))
		{
			_darknessAwoken = true;
			_prisonerAnimation.Play("PrisonerTestGrabLantern");
		}
	}
}
