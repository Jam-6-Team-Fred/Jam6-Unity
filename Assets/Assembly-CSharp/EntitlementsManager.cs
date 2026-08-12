using UnityEngine;

public class EntitlementsManager : MonoBehaviour, IPermanentManagerWorker
{
	public enum AsyncOwnershipStatus
	{
		NotReady = 0,
		Owned = 1,
		NotOwned = 2
	}

	[SerializeField]
	private PopupMenu _popupMenu;

	[Space(10f)]
	[Header("Test values (editor only)")]
	[Tooltip("Enable this to use test value")]
	[SerializeField]
	private bool _useTestValue;

	[SerializeField]
	private AsyncOwnershipStatus _testValue;

	private BaseEntitlementRetriever _entitlementRetriever;

	private static EntitlementsManager _instance;

	public static EntitlementsManager instance => _instance;

	public static event DlcInstallEvent OnDlcInstallEvent;

	private void Start()
	{
		_entitlementRetriever?.Initialize();
		Debug.Log("DLC installed: " + IsDlcOwned());
	}

	private void OnDestroy()
	{
		_entitlementRetriever?.OnDestroy();
	}

	public static AsyncOwnershipStatus IsDlcOwned()
	{
		if (_instance == null)
		{
			Debug.LogError("[EntitlementsManager] EntitlementsManager component on PermanentManager doesn't exist. Either it hasn't initialized yet or something has gone really wrong.");
			return AsyncOwnershipStatus.NotReady;
		}
		if (_instance._entitlementRetriever != null)
		{
			return _instance._entitlementRetriever.GetOwnershipStatus();
		}
		Debug.LogError("[EntitlementsManager] No entitlement retriever has been created. This is likely because no platform or plugin has been defined");
		return AsyncOwnershipStatus.NotReady;
	}

	public void InitializeOnAwake()
	{
		_instance = this;
		_entitlementRetriever = null;
		_entitlementRetriever = new SteamEntitlementRetriever();
	}

	public static void RaiseDLCInstalledEvent()
	{
		if (EntitlementsManager.OnDlcInstallEvent != null)
		{
			EntitlementsManager.OnDlcInstallEvent();
		}
	}
}
