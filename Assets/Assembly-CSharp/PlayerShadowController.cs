using UnityEngine;

public class PlayerShadowController : MonoBehaviour
{
	[SerializeField]
	private Renderer _shadowProjector;

	private void Awake()
	{
		GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
	}

	private void OnDestroy()
	{
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
	}

	private void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		_shadowProjector.enabled = false;
	}

	private void OnExitFlightConsole()
	{
		_shadowProjector.enabled = true;
	}
}
