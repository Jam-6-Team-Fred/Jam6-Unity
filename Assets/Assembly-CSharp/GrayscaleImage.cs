using UnityEngine;
using UnityEngine.UI;

public class GrayscaleImage : Image
{
	private int _propID_Greyscale;

	[SerializeField]
	private float _grayscalePercentage = 1f;

	protected override void Awake()
	{
		base.Awake();
		_propID_Greyscale = Shader.PropertyToID("_Greyscale");
		material = new Material(material);
	}

	public void SetImageAsGreyscale(bool value)
	{
		if (value)
		{
			_grayscalePercentage = 1f;
		}
		else
		{
			_grayscalePercentage = 0f;
		}
	}

	public override Material GetModifiedMaterial(Material baseMaterial)
	{
		Material modifiedMaterial = base.GetModifiedMaterial(baseMaterial);
		modifiedMaterial.SetFloat(_propID_Greyscale, _grayscalePercentage);
		return modifiedMaterial;
	}
}
