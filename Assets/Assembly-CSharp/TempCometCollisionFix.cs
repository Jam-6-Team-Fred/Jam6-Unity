using UnityEngine;

public class TempCometCollisionFix : MonoBehaviour
{
	public OWEvent onCometDestroyed;

	private void Start()
	{
		if (TimeLoop.IsTimeFlowing())
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (Time.timeSinceLevelLoad > 1200f)
		{
			onCometDestroyed.Invoke();
			base.gameObject.SetActive(value: false);
		}
	}
}
