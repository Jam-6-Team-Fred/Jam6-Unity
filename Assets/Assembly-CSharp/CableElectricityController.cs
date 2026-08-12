using UnityEngine;

[RequireComponent(typeof(OWRenderer))]
public class CableElectricityController : MonoBehaviour
{
	private static int _propID_RandomOffset = Shader.PropertyToID("_RandomOffset");

	private OWRenderer _owRenderer;

	private void Awake()
	{
		_owRenderer = GetComponent<OWRenderer>();
		_owRenderer.SetMaterialProperty(_propID_RandomOffset, Random.value);
	}
}
