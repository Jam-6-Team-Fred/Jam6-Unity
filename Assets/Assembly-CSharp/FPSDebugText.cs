using UnityEngine;

public class FPSDebugText : MonoBehaviour
{
	private float deltaTime;

	private void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}

	private void OnGUI()
	{
		float num = deltaTime * 1000f;
		float num2 = 1f / deltaTime;
		DebugText.SetText($"{num:0.0} ms ({num2:0.} fps)");
	}
}
