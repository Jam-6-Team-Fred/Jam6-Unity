using System;

[Serializable]
public struct CreditsTemplate
{
	public CreditsEntry contentTemplate;

	public CreditsEntry headerTemplate;

	public CreditsEntry titleTemplate;

	public override bool Equals(object obj)
	{
		if (obj.GetType() != typeof(CreditsTemplate))
		{
			return false;
		}
		CreditsTemplate creditsTemplate = (CreditsTemplate)obj;
		if (contentTemplate == creditsTemplate.contentTemplate && headerTemplate == creditsTemplate.headerTemplate)
		{
			return titleTemplate == creditsTemplate.titleTemplate;
		}
		return false;
	}

	public static bool operator ==(CreditsTemplate lhs, CreditsTemplate rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(CreditsTemplate lhs, CreditsTemplate rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override int GetHashCode()
	{
		if (contentTemplate == null && headerTemplate == null && titleTemplate == null)
		{
			return base.GetHashCode();
		}
		int num = 0;
		if (contentTemplate != null)
		{
			num ^= contentTemplate.GetHashCode();
		}
		if (headerTemplate != null)
		{
			num ^= headerTemplate.GetHashCode();
		}
		if (titleTemplate != null)
		{
			num ^= titleTemplate.GetHashCode();
		}
		return num;
	}
}
