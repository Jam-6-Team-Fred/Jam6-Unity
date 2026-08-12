using System;
using UnityEngine;

public class OWFlameController : OWLightController
{
	[SerializeField]
	private OWRenderer[] _flameRenderers;

	private static int s_propID_MainTex_ST = Shader.PropertyToID("_MainTex_ST");

	public OWRenderer[] flameRenderers => _flameRenderers;

	protected override void UpdateVisuals()
	{
		base.UpdateVisuals();
		if (_flameRenderers.Length == 0)
		{
			return;
		}
		try
		{
			Vector4 value = new Vector4(1f, 1f, 0f, 1f - _intensity);
			for (int i = 0; i < _flameRenderers.Length; i++)
			{
				_flameRenderers[i].SetMaterialProperty(s_propID_MainTex_ST, value);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
