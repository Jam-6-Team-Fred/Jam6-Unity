using UnityEngine;

public class PrintNormal : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		Debug.Log(base.transform.forward.normalized);
	}
}
