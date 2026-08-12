using UnityEngine;

public class StrobeRenderer : MonoBehaviour
{
	private float random;

	private void Start()
	{
		random = Random.value;
	}

	private void Update()
	{
		GetComponent<Renderer>().enabled = Mathf.Sin(Time.time * random * 100f) > 0f;
	}
}
