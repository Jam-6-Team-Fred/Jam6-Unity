using UnityEngine;

[ExecuteInEditMode]
public class ReplaceWithPrefab : MonoBehaviour
{
	public string prefabName;

	private void Awake()
	{
	}

	private void Start()
	{
		Object.DestroyImmediate(base.gameObject);
	}
}
