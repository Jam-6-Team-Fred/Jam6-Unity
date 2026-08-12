public struct LightPulse
{
	public bool illuminated;

	public bool isLong;

	public LightPulse(bool illuminated, float duration)
	{
		this.illuminated = illuminated;
		isLong = duration > 0.7f && duration <= 3f;
	}

	public LightPulse(bool illuminated, bool isLong)
	{
		this.illuminated = illuminated;
		this.isLong = isLong;
	}

	public bool Equals(LightPulse pulse)
	{
		if (pulse.illuminated == illuminated)
		{
			return pulse.isLong == isLong;
		}
		return false;
	}
}
