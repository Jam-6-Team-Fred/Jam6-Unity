using UnityEngine;

public class AlarmBridgeController : MonoBehaviour
{
	[SerializeField]
	private AbstractGhostDoorInterface _codeInterface;

	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private OWRendererFadeController _lightBeamController;

	private void Awake()
	{
		_codeInterface.OnOpen += OnOpen;
		_codeInterface.OnClose += OnClose;
	}

	private void OnDestroy()
	{
		_codeInterface.OnOpen -= OnOpen;
		_codeInterface.OnClose -= OnClose;
	}

	private void OnOpen()
	{
		_lightController.FadeTo(0f, 1f);
		_lightBeamController.FadeTo(0f, 1f);
	}

	private void OnClose()
	{
		_lightController.FadeTo(1f, 1f);
		_lightBeamController.FadeTo(1f, 1f);
	}
}
