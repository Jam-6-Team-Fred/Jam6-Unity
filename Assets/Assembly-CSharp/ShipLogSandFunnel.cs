using UnityEngine;
using UnityEngine.UI;

public class ShipLogSandFunnel : MonoBehaviour
{
	[SerializeField]
	private ShipLogAstroObject _caveTwin;

	[SerializeField]
	private ShipLogAstroObject _towerTwin;

	[SerializeField]
	private Image _image;

	private Material _greyscaleMaterial;

	private void Awake()
	{
		_greyscaleMaterial = _image.material;
	}

	public void UpdateState()
	{
		_image.gameObject.SetActive(value: false);
		if (_caveTwin.GetState() == ShipLogEntry.State.Explored && _towerTwin.GetState() == ShipLogEntry.State.Explored)
		{
			_image.gameObject.SetActive(value: true);
			_image.material = null;
		}
		else if (_caveTwin.GetState() != ShipLogEntry.State.Hidden && _towerTwin.GetState() != ShipLogEntry.State.Hidden)
		{
			_image.gameObject.SetActive(value: true);
			_image.material = _greyscaleMaterial;
		}
	}
}
