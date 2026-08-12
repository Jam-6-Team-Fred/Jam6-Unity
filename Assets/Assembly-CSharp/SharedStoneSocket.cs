using UnityEngine;

public class SharedStoneSocket : OWItemSocket
{
	[SerializeField]
	private PedestalAnimator _pedestalAnimator;

	protected override void Awake()
	{
		base.Awake();
		_acceptableType = ItemType.SharedStone;
		_pedestalAnimator = base.transform.GetComponentInChildren<PedestalAnimator>();
		if (_pedestalAnimator == null)
		{
			Debug.LogError("Pedestal animator is NULL", this);
			Debug.Break();
		}
	}

	protected override void Start()
	{
		base.Start();
		if (_socketedItem != null && _socketedItem as SharedStone == null)
		{
			Debug.LogError("WRONG SOCKETED ITEM: Expected Shared Stone", this);
		}
		if (_socketedItem != null)
		{
			_pedestalAnimator.SetClosed();
		}
		else
		{
			_pedestalAnimator.SetOpen();
		}
	}

	public PedestalAnimator GetPedestalAnimator()
	{
		return _pedestalAnimator;
	}
}
