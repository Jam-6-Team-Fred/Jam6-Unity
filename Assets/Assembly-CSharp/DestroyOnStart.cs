using UnityEngine;

public class DestroyOnStart : MonoBehaviour
{
	private void Start()
	{
		Object.Destroy(base.gameObject);
	}
}
