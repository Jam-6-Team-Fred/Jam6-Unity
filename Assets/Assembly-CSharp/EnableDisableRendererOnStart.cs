using UnityEngine;

public class EnableDisableRendererOnStart : MonoBehaviour
{
	[SerializeField]
	private bool _enabled = true;

	private void Start()
	{
		GetComponent<Renderer>().enabled = _enabled;
	}
}
