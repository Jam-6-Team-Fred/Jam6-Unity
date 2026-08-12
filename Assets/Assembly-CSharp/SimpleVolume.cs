using UnityEngine;

public abstract class SimpleVolume : MonoBehaviour
{
	public enum Shape
	{
		Sphere = 0,
		Capsule = 1,
		Box = 2
	}

	[SerializeField]
	protected Shape _shape;

	[SerializeField]
	protected float _radius = 1f;

	[SerializeField]
	protected float _height = 2f;

	[SerializeField]
	protected Vector3 _size = Vector3.one;

	public Shape shape
	{
		get
		{
			return _shape;
		}
		set
		{
			_shape = value;
		}
	}

	public float radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = Mathf.Max(value, 0f);
		}
	}

	public float height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = Mathf.Max(value, 0f);
		}
	}

	public Vector3 size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = Vector3.Max(value, Vector3.zero);
		}
	}

	public float CalcRealSphereRadius()
	{
		return Mathf.Max(_radius * Mathf.Max(Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.y), base.transform.lossyScale.z), 0f);
	}

	public float CalcRealCapsuleRadius()
	{
		return Mathf.Max(_radius * Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.z), 0f);
	}

	public float CalcRealCapsuleHeight()
	{
		return Mathf.Max(Mathf.Max(_height * base.transform.lossyScale.y, _radius * Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.z) * 2f), 0f);
	}

	protected virtual void OnValidate()
	{
		if (_radius < 0f)
		{
			_radius = 0f;
		}
		if (_height < 0f)
		{
			_height = 0f;
		}
		if (_size.x < 0f || _size.y < 0f)
		{
			_size = Vector3.Max(_size, Vector3.zero);
		}
	}

	public virtual bool Contains(Vector3 point)
	{
		switch (_shape)
		{
		case Shape.Sphere:
		{
			float num2 = CalcRealSphereRadius();
			return Vector3.SqrMagnitude(point - base.transform.position) < num2 * num2;
		}
		case Shape.Capsule:
		{
			float num = CalcRealCapsuleRadius();
			Vector3 vector = base.transform.up * (_height * 0.5f * base.transform.lossyScale.y - num);
			Vector3 segmentStart = base.transform.position + vector;
			Vector3 segmentEnd = base.transform.position - vector;
			return OWMath.PointSegmentDistance(point, segmentStart, segmentEnd) < num;
		}
		case Shape.Box:
			return OWMath.PointInBox(point, base.transform.position, Vector3.Scale(_size, base.transform.lossyScale) * 0.5f, base.transform.rotation);
		default:
			return false;
		}
	}

	public virtual float GetPenetrationDist(Vector3 point)
	{
		switch (_shape)
		{
		case Shape.Sphere:
		{
			float num2 = CalcRealSphereRadius();
			return Vector3.Distance(point, base.transform.position) - num2;
		}
		case Shape.Capsule:
		{
			float num = CalcRealCapsuleRadius();
			Vector3 vector = base.transform.up * (_height * 0.5f * base.transform.lossyScale.y - num);
			Vector3 segmentStart = base.transform.position + vector;
			Vector3 segmentEnd = base.transform.position - vector;
			return OWMath.PointSegmentDistance(point, segmentStart, segmentEnd) - num;
		}
		case Shape.Box:
			return OWMath.PointBoxDistance(point, base.transform.position, Vector3.Scale(_size, base.transform.lossyScale) * 0.5f, base.transform.rotation);
		default:
			return 0f;
		}
	}
}
