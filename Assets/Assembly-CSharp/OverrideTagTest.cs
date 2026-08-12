using UnityEngine;

public class OverrideTagTest : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Renderer>().material.SetOverrideTag("ProxyShadow", "On");
	}

	private void Update()
	{
	}

	private void OnDisable()
	{
		GetComponent<Renderer>().material.SetOverrideTag("ProxyShadow", "");
	}
}
