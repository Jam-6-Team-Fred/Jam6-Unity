using UnityEngine;

public class SetActiveOnDLC : MonoBehaviour
{
	[SerializeField]
	private bool _activeIfDLCOwned;

	private bool _hasChecked;

	public bool activeIfDlcOwned => _activeIfDLCOwned;

	private void Awake()
	{
		if (EntitlementsManager.instance != null)
		{
			CheckActivation();
		}
	}

	private void Start()
	{
		if (!_hasChecked)
		{
			CheckActivation();
		}
	}

	private void CheckActivation()
	{
		bool flag = EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned;
		bool active = (_activeIfDLCOwned && flag) || (!_activeIfDLCOwned && !flag);
		base.gameObject.SetActive(active);
		_hasChecked = true;
	}
}
