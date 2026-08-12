using UnityEngine;

public class SlideReelSocket : OWItemSocket
{
	[SerializeField]
	private Vector3 _unsocketDir = new Vector3(-0.3f, 0f, 1f);

	[SerializeField]
	private bool _reversableUnsocketDir = true;

	protected override void Awake()
	{
		base.Awake();
		_acceptableType = ItemType.SlideReel;
	}

	private Vector3 CalcCorrectUnsocketDir(Transform itemTransform)
	{
		if (!_reversableUnsocketDir)
		{
			return _unsocketDir;
		}
		Vector3 rhs = base.transform.TransformDirection(_unsocketDir);
		if (Vector3.Dot(itemTransform.position - base.transform.position, rhs) < 0f)
		{
			return new Vector3(_unsocketDir.x, _unsocketDir.y, 0f - _unsocketDir.z);
		}
		return _unsocketDir;
	}

	public override bool PlaceIntoSocket(OWItem item)
	{
		(item as SlideReelItem).SetSocketLocalDir(CalcCorrectUnsocketDir(item.transform));
		return base.PlaceIntoSocket(item);
	}

	public override OWItem RemoveFromSocket()
	{
		(_socketedItem as SlideReelItem).SetSocketLocalDir(CalcCorrectUnsocketDir(Locator.GetPlayerCamera().transform));
		return base.RemoveFromSocket();
	}
}
