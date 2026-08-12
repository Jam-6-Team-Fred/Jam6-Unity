using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class ImpactSensor : MonoBehaviour
{
	public delegate void ImpactEvent(ImpactData impact);

	private OWRigidbody _owRigidbody;

	private bool _blockSameFrameImpacts;

	private float _lastHighSpeedImpactTime;

	public event ImpactEvent OnImpact;

	private void Awake()
	{
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
	}

	public void FireHighSpeedImpactEvent(ImpactData impact)
	{
		_blockSameFrameImpacts = true;
		_lastHighSpeedImpactTime = Time.time;
		if (this.OnImpact != null)
		{
			this.OnImpact(impact);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.contacts[0].thisCollider.gameObject.layer == OWLayerMask.playerSafetyCollider || collision.contacts[0].otherCollider.gameObject.layer == OWLayerMask.playerSafetyCollider)
		{
			return;
		}
		if (_blockSameFrameImpacts)
		{
			if (OWMath.ApproxEquals(Time.time, _lastHighSpeedImpactTime))
			{
				MonoBehaviour.print("blocked impact same frame as high speed impact   time: " + Time.time);
				return;
			}
			_blockSameFrameImpacts = false;
		}
		if (collision.rigidbody != null)
		{
			OWRigidbody requiredComponent = collision.rigidbody.GetRequiredComponent<OWRigidbody>();
			ImpactData impactData = new ImpactData(_owRigidbody, requiredComponent, collision);
			FragmentIntegrity componentInParent = collision.collider.GetComponentInParent<FragmentIntegrity>();
			if (componentInParent != null)
			{
				componentInParent.HandleImpact(_owRigidbody.GetMass(), impactData.speed);
			}
			if (this.OnImpact != null)
			{
				this.OnImpact(impactData);
			}
		}
	}
}
