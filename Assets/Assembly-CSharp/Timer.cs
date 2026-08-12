using UnityEngine;

public struct Timer
{
	public float startTime;

	public float endTime;

	private float _elapsed;

	private float _lastElapsed;

	public Timer(float duration)
	{
		startTime = Time.time;
		endTime = startTime + duration;
		_lastElapsed = (_elapsed = -1f);
	}

	public void Reset()
	{
		startTime = Time.time;
	}

	public float GetDuration()
	{
		return endTime - startTime;
	}

	public static float FractionOfTimespan(float startTime, float endTime, bool clamped = true)
	{
		float num = (Time.time - startTime) / (endTime - startTime);
		if (!clamped)
		{
			return num;
		}
		return Mathf.Clamp01(num);
	}

	public float Fraction(bool clamped = true)
	{
		float num = (Time.time - startTime) / (endTime - startTime);
		if (!clamped)
		{
			return num;
		}
		return Mathf.Clamp01(num);
	}

	public bool IsFinished()
	{
		return Fraction() >= 1f;
	}

	public float FractionOfSubspan(float t1, float t2, bool clamped = true)
	{
		return FractionOfTimespan(startTime + t1, startTime + t2, clamped);
	}

	public float FractionOfFraction(float startFraction, float endFraction, bool clamped = true)
	{
		float num = (Fraction(clamped: false) - startFraction) / (endFraction - startFraction);
		if (!clamped)
		{
			return num;
		}
		return Mathf.Clamp01(num);
	}

	public bool IsWithinSubspan(float t1, float t2)
	{
		float num = SecondsElapsed();
		if (num >= t1)
		{
			return num < t2;
		}
		return false;
	}

	public float SecondsElapsed()
	{
		return Time.time - startTime;
	}

	public void Update()
	{
		_lastElapsed = _elapsed;
		_elapsed = SecondsElapsed();
	}

	public bool SecondsNewlyElapsed(float threshold)
	{
		if (_lastElapsed < threshold)
		{
			return SecondsElapsed() >= threshold;
		}
		return false;
	}
}
