using UnityEngine;

[ExecuteInEditMode] // CHANGED
public class BRDFManager : MonoBehaviour
{
	[SerializeField]
	public BRDFRegistry _brdfRegistryAsset; // CHANGED

	private void Awake()
	{
		if (_brdfRegistryAsset == null)
		{
			Debug.LogError("No BRDF Registry found!");
		}
		else
		{
			_brdfRegistryAsset.UpdateBRDFs();
		}
	}
}
