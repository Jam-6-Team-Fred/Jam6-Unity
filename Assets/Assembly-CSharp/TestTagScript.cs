using UnityEngine;

public class TestTagScript : MonoBehaviour
{
	private void Awake()
	{
		MonoBehaviour.print(base.gameObject.FindWithRequiredTag("Player").name);
	}
}
