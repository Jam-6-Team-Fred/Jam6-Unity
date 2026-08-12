using UnityEngine;

public class DestroyOnEvent : MonoBehaviour
{
	[SerializeField]
	private bool _startOfFirstTimeLoop;

	[SerializeField]
	private bool _resumeSimulation;

	[SerializeField]
	private bool _nomaiStatueActivated;

	[SerializeField]
	private bool _onLoopGO3;

	private void Awake()
	{
		GlobalMessenger<int>.AddListener("StartOfTimeLoop", OnStartOfTimeLoop);
		GlobalMessenger.AddListener("ResumeSimulation", OnResumeSimulation);
		GlobalMessenger.AddListener("NomaiStatueActivated", OnNomaiStatueActivated);
	}

	private void OnDestroy()
	{
		GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", OnStartOfTimeLoop);
		GlobalMessenger.RemoveListener("ResumeSimulation", OnResumeSimulation);
		GlobalMessenger.RemoveListener("NomaiStatueActivated", OnNomaiStatueActivated);
	}

	private void OnStartOfTimeLoop(int loopCount)
	{
		if (_startOfFirstTimeLoop && loopCount < 2)
		{
			Object.Destroy(base.gameObject);
		}
		if (_onLoopGO3 && loopCount > 1)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnResumeSimulation()
	{
		if (_resumeSimulation)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnNomaiStatueActivated()
	{
		if (_nomaiStatueActivated)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
