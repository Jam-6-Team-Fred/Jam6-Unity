using UnityEngine;

public class DayNightTracker : MonoBehaviour
{
	public delegate void SunriseEvent();

	public delegate void SunsetEvent();

	[SerializeField]
	private float _daySideConeAngle = 180f;

	[SerializeField]
	private GameObject[] _nightLightRoots;

	private PlanetoidRuleset _planetoidRuleset;

	private OWRigidbody _parentBody;

	private bool _wasDay;

	public event SunriseEvent OnSunrise;

	public event SunsetEvent OnSunset;

	private void Awake()
	{
		_parentBody = this.GetAttachedOWRigidbody();
		for (int i = 0; i < _nightLightRoots.Length; i++)
		{
			NightLight[] componentsInChildren = _nightLightRoots[i].GetComponentsInChildren<NightLight>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].SetDayNightTracker(this);
			}
		}
		InvokeRepeating("CheckDayNightTransition", 0f, 1f);
	}

	private void CheckDayNightTransition()
	{
		Vector3 position = _parentBody.GetPosition();
		Vector3 from = Locator.GetSunTransform().position - position;
		Vector3 to = base.transform.position - position;
		bool flag = Vector3.Angle(from, to) < _daySideConeAngle * 0.5f;
		if (flag && !_wasDay && this.OnSunrise != null)
		{
			this.OnSunrise();
		}
		else if (!flag && _wasDay && this.OnSunset != null)
		{
			this.OnSunset();
		}
		_wasDay = flag;
	}
}
