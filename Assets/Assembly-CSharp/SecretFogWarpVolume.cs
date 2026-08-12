using UnityEngine;

public class SecretFogWarpVolume : OuterFogWarpVolume
{
	[Space]
	[SerializeField]
	protected Transform _probeSocket;

	protected override void RepositionWarpedBody(OWRigidbody body, Vector3 localRelVelocity, Vector3 localPos, Quaternion localRot)
	{
		if (body.CompareTag("Probe"))
		{
			body.GetComponent<SurveyorProbe>().GetAnchor().AnchorToSocket(_probeSocket);
		}
	}
}
