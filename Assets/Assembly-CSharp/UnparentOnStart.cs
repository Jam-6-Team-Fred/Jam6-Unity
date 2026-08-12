using UnityEngine;

public class UnparentOnStart : MonoBehaviour
{
	[SerializeField]
	private Vector3 _worldPos = Vector3.zero;

	[SerializeField]
	private Vector3 _worldRot = Vector3.zero;

	[SerializeField]
	private Vector3 _worldScale = Vector3.one;

	private void Start()
	{
		base.transform.SetParent(null);
		base.transform.position = _worldPos;
		base.transform.eulerAngles = _worldRot;
		base.transform.localScale = _worldScale;
		Object.Destroy(this);
	}
}
