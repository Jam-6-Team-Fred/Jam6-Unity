using UnityEngine;

public class ImpactData
{
	public readonly Vector3 velocity;

	public readonly float speed;

	public readonly SurfaceType[] contactSurfaceTypes;

	public readonly OWRigidbody thisBody;

	public readonly OWRigidbody otherBody;

	public readonly Collider thisCollider;

	public readonly Collider otherCollider;

	public readonly Vector3 point;

	public readonly Vector3 normal;

	public ImpactData(ImpactData other, float velocityScale = 1f)
	{
		velocity = other.velocity * velocityScale;
		speed = other.speed * velocityScale;
		contactSurfaceTypes = other.contactSurfaceTypes;
		thisBody = other.thisBody;
		otherBody = other.otherBody;
		thisCollider = other.thisCollider;
		otherCollider = other.otherCollider;
		point = other.point;
		normal = other.normal;
	}

	public ImpactData(OWRigidbody thisOWRigidbody, OWRigidbody collidedOWRigidbody, Collision collision)
	{
		thisBody = thisOWRigidbody;
		otherBody = collidedOWRigidbody;
		thisCollider = collision.contacts[0].thisCollider;
		otherCollider = collision.contacts[0].otherCollider;
		point = collision.contacts[0].point;
		normal = collision.contacts[0].normal;
		Vector3 vector = collidedOWRigidbody.GetPointVelocity(point) - thisOWRigidbody.GetPointVelocity(point);
		velocity = Vector3.Project(vector, normal);
		speed = velocity.magnitude;
		SurfaceManager surfaceManager = Locator.GetSurfaceManager();
		contactSurfaceTypes = new SurfaceType[collision.contacts.Length];
		for (int i = 0; i < collision.contacts.Length; i++)
		{
			contactSurfaceTypes[i] = SurfaceType.None;
			Vector3 origin = collision.contacts[i].point + collision.contacts[i].normal;
			if (collision.contacts[i].otherCollider.Raycast(new Ray(origin, -collision.contacts[i].normal), out var hitInfo, 2f))
			{
				contactSurfaceTypes[i] = surfaceManager.GetHitSurfaceType(hitInfo);
			}
		}
	}

	public ImpactData(OWRigidbody thisBody, Collider thisCollider, OWRigidbody otherBody, Vector3 velocity, float speed, RaycastHit hitInfo)
	{
		this.thisBody = thisBody;
		this.otherBody = otherBody;
		this.thisCollider = thisCollider;
		otherCollider = hitInfo.collider;
		this.velocity = velocity;
		this.speed = speed;
		point = hitInfo.point;
		normal = hitInfo.normal;
		contactSurfaceTypes = new SurfaceType[1];
		contactSurfaceTypes[0] = Locator.GetSurfaceManager().GetHitSurfaceType(hitInfo);
	}
}
