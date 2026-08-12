using UnityEngine;

public class DestroyOnDLC : MonoBehaviour
{
	[SerializeField]
	private bool _destroyOnDLCOwned;

	[SerializeField]
	private bool _destroyOnDLCNotOwned;

	private bool _hasChecked;

	public bool destroyOnDlcOwned => _destroyOnDLCOwned;

	public bool destroyOnDlcNotOwned => _destroyOnDLCNotOwned;

	private void Awake()
	{
		if (EntitlementsManager.instance != null)
		{
			CheckIfDestroy();
		}
	}

	private void Start()
	{
		if (!_hasChecked)
		{
			CheckIfDestroy();
		}
	}

	private void CheckIfDestroy()
	{
		bool flag = EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned;
		if ((_destroyOnDLCOwned && flag) || (_destroyOnDLCNotOwned && !flag))
		{
			Object.Destroy(base.gameObject);
		}
		_hasChecked = true;
	}
}
