using UnityEngine;

public class FrameTracker : MonoBehaviour
{
	[SerializeField]
	private bool _logUpdate;

	[SerializeField]
	private bool _logFixedUpdate;

	private void Update()
	{
		if (_logUpdate)
		{
			MonoBehaviour.print("------------------------------------------------------------------- BEGIN UPDATE");
		}
	}

	private void FixedUpdate()
	{
		if (_logFixedUpdate)
		{
			MonoBehaviour.print("------------------------------------------------------------------- BEGIN FIXED UPDATE");
		}
	}
}
