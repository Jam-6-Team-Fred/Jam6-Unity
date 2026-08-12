using UnityEngine;

public class QuantumSocketCollapseTrigger : MonoBehaviour
{
	[SerializeField]
	private QuantumSocket _quantumSocket;

	[SerializeField]
	private bool _forceCollapse;

	private void Awake()
	{
		GetComponent<Collider>().Assert(OWLayerMask.effectVolumeMask, isTrigger: true);
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("PlayerDetector"))
		{
			_quantumSocket.CollapseOccupant(_forceCollapse);
		}
	}
}
