using UnityEngine;

public class LightSwitch : MonoBehaviour
{
	[SerializeField]
	protected Light[] _lights;

	protected virtual void Awake()
	{
		if (_lights == null || _lights.Length == 0)
		{
			_lights = GetComponentsInChildren<Light>();
		}
	}

	public virtual void TurnOn()
	{
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].enabled = true;
		}
	}

	public virtual void TurnOff()
	{
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].enabled = false;
		}
	}
}
