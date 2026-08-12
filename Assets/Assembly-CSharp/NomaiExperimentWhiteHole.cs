using UnityEngine;

public class NomaiExperimentWhiteHole : MonoBehaviour
{
	private SingularityController _singularityController;

	private WhiteHoleFluidVolume _whiteHoleFluidVolume;

	private void Awake()
	{
		_singularityController = GetComponentInChildren<SingularityController>();
		_singularityController.OnCollapse += OnSingularityCollapse;
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		_singularityController.OnCollapse -= OnSingularityCollapse;
	}

	public void OpenSingularity()
	{
		_singularityController.Create();
	}

	public void CloseSingularity()
	{
		_singularityController.Collapse();
	}

	public void PlayExitAudio(bool isPlayer = false)
	{
		_singularityController.PlayExitAudio(isPlayer);
	}

	private void OnSingularityCollapse()
	{
	}

	private bool IsSingularityOpen()
	{
		return _singularityController.GetState() != SingularityController.State.Collapsed;
	}
}
