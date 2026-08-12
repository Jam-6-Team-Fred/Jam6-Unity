public abstract class BaseEntitlementRetriever
{
	public virtual void Initialize()
	{
	}

	public virtual void OnDestroy()
	{
	}

	public abstract EntitlementsManager.AsyncOwnershipStatus GetOwnershipStatus();
}
