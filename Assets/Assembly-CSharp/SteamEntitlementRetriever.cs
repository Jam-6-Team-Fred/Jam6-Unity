using Steamworks;

public class SteamEntitlementRetriever : BaseEntitlementRetriever
{
	public const long STEAM_DLC_APP_ID = 1622100L;

	public override EntitlementsManager.AsyncOwnershipStatus GetOwnershipStatus()
	{
		if (!SteamApps.BIsDlcInstalled((AppId_t)1622100u))
		{
			return EntitlementsManager.AsyncOwnershipStatus.NotOwned;
		}
		return EntitlementsManager.AsyncOwnershipStatus.Owned;
	}
}
