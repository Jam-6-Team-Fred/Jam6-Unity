using System.Collections.Generic;
using UnityEngine;

public class KinematicCollisionManager : MonoBehaviour
{
	private class IntegrationData
	{
		public KinematicRigidbody body;

		public Vector3 startPosition;

		public Quaternion startRotation;

		public Vector3 deltaPosition;

		public Quaternion deltaRotation;

		public Vector3 finalPosition;

		public Quaternion finalRotation;

		public IntegrationData(KinematicRigidbody kinematicRigidbody)
		{
			body = kinematicRigidbody;
			startPosition = Vector3.zero;
			startRotation = Quaternion.identity;
			deltaPosition = Vector3.zero;
			deltaRotation = Quaternion.identity;
			finalPosition = Vector3.zero;
			finalRotation = Quaternion.identity;
		}
	}

	private static bool _active;

	private static List<IntegrationData> _kinematicRigidbodies;

	public static bool active => _active;

	private void Awake()
	{
		if (_kinematicRigidbodies == null)
		{
			_kinematicRigidbodies = new List<IntegrationData>(256);
		}
	}

	private void OnDestroy()
	{
		_kinematicRigidbodies = null;
	}

	private void OnEnable()
	{
		_active = true;
	}

	private void OnDisable()
	{
		_active = false;
	}

	public static void RegisterKinematicRigidbody(KinematicRigidbody kinematicRigidbody)
	{
		if (kinematicRigidbody == null)
		{
			return;
		}
		if (_kinematicRigidbodies == null)
		{
			_kinematicRigidbodies = new List<IntegrationData>(256);
		}
		for (int i = 0; i < _kinematicRigidbodies.Count; i++)
		{
			if (_kinematicRigidbodies[i].body == kinematicRigidbody)
			{
				return;
			}
		}
		_kinematicRigidbodies.Add(new IntegrationData(kinematicRigidbody));
	}

	public static void UnregisterKinematicRigidbody(KinematicRigidbody kinematicRigidbody)
	{
		if (kinematicRigidbody == null || _kinematicRigidbodies == null)
		{
			return;
		}
		for (int i = 0; i < _kinematicRigidbodies.Count; i++)
		{
			if (_kinematicRigidbodies[i].body == kinematicRigidbody)
			{
				_kinematicRigidbodies.QuickRemoveAt(i);
				break;
			}
		}
	}

	private void FixedUpdate()
	{
		for (int num = _kinematicRigidbodies.Count - 1; num >= 0; num--)
		{
			if (_kinematicRigidbodies[num].body == null)
			{
				_kinematicRigidbodies.QuickRemoveAt(num);
			}
			else
			{
				IntegrationData integrationData = _kinematicRigidbodies[num];
				integrationData.startPosition = integrationData.body.owRigidbody.GetPosition();
				integrationData.startRotation = integrationData.body.owRigidbody.GetRotation();
				integrationData.deltaPosition = integrationData.body.IntegratePosition();
				integrationData.deltaRotation = integrationData.body.IntegrateRotation();
				integrationData.finalPosition = integrationData.startPosition + integrationData.deltaPosition;
				integrationData.finalRotation = integrationData.deltaRotation * integrationData.startRotation;
			}
		}
		for (int i = 0; i < _kinematicRigidbodies.Count; i++)
		{
			IntegrationData integrationData2 = _kinematicRigidbodies[i];
			KinematicRigidbody body = integrationData2.body;
			KinematicCollider[] kinematicColliders = body.kinematicColliders;
			if (kinematicColliders.Length == 0)
			{
				continue;
			}
			float num2 = 1f / body.owRigidbody.GetMass();
			for (int j = i + 1; j < _kinematicRigidbodies.Count; j++)
			{
				IntegrationData integrationData3 = _kinematicRigidbodies[j];
				KinematicRigidbody body2 = integrationData3.body;
				KinematicCollider[] kinematicColliders2 = body2.kinematicColliders;
				if (kinematicColliders2.Length == 0)
				{
					continue;
				}
				float num3 = 1f / body2.owRigidbody.GetMass();
				bool flag = false;
				Vector3 vector = Vector3.zero;
				Vector3 vector2 = Vector3.zero;
				float num4 = 0f;
				KinematicCollider kinematicCollider = null;
				KinematicCollider kinematicCollider2 = null;
				foreach (KinematicCollider kinematicCollider3 in kinematicColliders)
				{
					foreach (KinematicCollider kinematicCollider4 in kinematicColliders2)
					{
						if (kinematicCollider3.gameObject.activeInHierarchy && kinematicCollider3.collider.enabled && kinematicCollider4.gameObject.activeInHierarchy && kinematicCollider4.collider.enabled && Physics.ComputePenetration(kinematicCollider3.collider, integrationData2.finalPosition, integrationData2.finalRotation, kinematicCollider4.collider, integrationData3.finalPosition, integrationData3.finalRotation, out var direction, out var distance))
						{
							float num5 = num2 / (num2 + num3);
							integrationData2.finalPosition += direction * distance * num5;
							integrationData3.finalPosition -= direction * distance * (1f - num5);
							if (!flag || distance > num4)
							{
								flag = true;
								Vector3 vector3 = Physics.ClosestPoint(kinematicCollider4.collider.bounds.center, kinematicCollider3.collider, integrationData2.finalPosition, integrationData2.finalRotation);
								Vector3 vector4 = Physics.ClosestPoint(kinematicCollider3.collider.bounds.center, kinematicCollider4.collider, integrationData3.finalPosition, integrationData3.finalRotation);
								vector = (vector3 + vector4) * 0.5f;
								vector2 = direction;
								num4 = distance;
								kinematicCollider = kinematicCollider3;
								kinematicCollider2 = kinematicCollider4;
							}
						}
					}
				}
				if (flag)
				{
					float a = ((kinematicCollider.collider.sharedMaterial != null) ? kinematicCollider.collider.sharedMaterial.bounciness : 0f);
					float b = ((kinematicCollider2.collider.sharedMaterial != null) ? kinematicCollider2.collider.sharedMaterial.bounciness : 0f);
					float num6 = Mathf.Min(a, b);
					Vector3 vector5 = -vector2;
					Vector3 vector6 = body2.GetPointVelocity(vector) - body.GetPointVelocity(vector);
					Vector3 lhs = vector - body.worldCenterOfMass;
					Vector3 lhs2 = vector - body2.worldCenterOfMass;
					Vector3 vector7 = Vector3.Cross(lhs, vector5);
					Vector3 vector8 = Vector3.Cross(lhs2, vector5);
					float num7 = (0f - (1f + num6)) * Mathf.Min(Vector3.Dot(vector6, vector5), 0f);
					num7 /= num2 + num3;
					body.velocity -= num7 * num2 * vector5;
					body2.velocity += num7 * num3 * vector5;
					body.angularVelocity += num7 * num2 * vector7 / 1000f;
					body2.angularVelocity += num7 * num3 * vector8 / 1000f;
					KinematicCollision kinematicCollision = default(KinematicCollision);
					kinematicCollision.impulse = num7 * (num2 + num3) * vector5;
					kinematicCollision.relativeVelocity = vector6;
					kinematicCollision.kinematicRigidbody = body2;
					kinematicCollision.point = vector;
					kinematicCollision.normal = vector5;
					kinematicCollision.thisKinematicCollider = kinematicCollider;
					kinematicCollision.otherKinematicCollider = kinematicCollider2;
					KinematicCollision collision = kinematicCollision;
					body.FireCollisionEvent(collision);
					collision.impulse = -collision.impulse;
					collision.kinematicRigidbody = body;
					collision.normal = -collision.normal;
					collision.thisKinematicCollider = kinematicCollider2;
					collision.otherKinematicCollider = kinematicCollider;
					body2.FireCollisionEvent(collision);
				}
			}
		}
		for (int m = 0; m < _kinematicRigidbodies.Count; m++)
		{
			_kinematicRigidbodies[m].body.Move(_kinematicRigidbodies[m].finalPosition, _kinematicRigidbodies[m].finalRotation);
			_kinematicRigidbodies[m].body.ResetAccumulators();
		}
	}
}
