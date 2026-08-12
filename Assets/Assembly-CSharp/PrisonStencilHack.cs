using UnityEngine;

public class PrisonStencilHack : MonoBehaviour
{
	[SerializeField]
	private OWRingRiverCollider _riverCollider;

	[SerializeField]
	private OWRenderer _topStencil;

	[SerializeField]
	private OWRenderer _bottomStencil;

	[SerializeField]
	private float _floodLerpSwapTime = 0.62f;

	private bool _swapped;

	private void Awake()
	{
		SetSwappped(swapped: false);
	}

	private void Update()
	{
		float floodLerp = _riverCollider.GetFloodLerp();
		if (floodLerp >= _floodLerpSwapTime && !_swapped)
		{
			SetSwappped(swapped: true);
		}
		else if (floodLerp < _floodLerpSwapTime && _swapped)
		{
			SetSwappped(swapped: false);
		}
	}

	private void SetSwappped(bool swapped)
	{
		_topStencil.SetActivation(!swapped);
		_bottomStencil.SetActivation(swapped);
		_swapped = swapped;
	}
}
