using System.Collections.Generic;
using ShapeCollision;
using UnityEngine;

[AddComponentMenu("Shapes/Shape Manager", -100)]
public class ShapeManager : MonoBehaviour
{
	private class Layer
	{
		private int _count;

		private ShapeData[] _array;

		public int Count => _count;

		public ShapeData this[int index] => _array[index];

		public Layer(int maxCount)
		{
			_count = 0;
			_array = new ShapeData[maxCount];
			for (int i = 0; i < maxCount; i++)
			{
				_array[i] = new ShapeData();
				_array[i].box.worldAxes = new Vector3[3];
				_array[i].box.worldVertices = new Vector3[8];
			}
		}

		public void Add(Shape shape)
		{
			if (_count >= _array.Length)
			{
				Debug.LogWarning("ShapeManager: Trying to add a shape to an already full layer!");
			}
			_array[_count].Set(shape);
			_count++;
		}

		public void RemoveAt(int index)
		{
			_array[index].shape = null;
			if (_array[index].contacts != null)
			{
				_array[index].contacts.Clear();
			}
			ShapeData shapeData = _array[index];
			_array[index] = _array[_count - 1];
			_array[_count - 1] = shapeData;
			_count--;
		}

		public void Remove(Shape shape)
		{
			for (int i = 0; i < _count; i++)
			{
				if (_array[i].shape == shape)
				{
					RemoveAt(i);
					break;
				}
			}
		}

		public int IndexOf(Shape shape)
		{
			for (int i = 0; i < _count; i++)
			{
				if (_array[i].shape == shape)
				{
					return i;
				}
			}
			return -1;
		}
	}

	private struct ContactData
	{
		public bool newFlag;

		public bool frameFlag;

		public Shape shape;

		public ContactData(Shape shape, bool newFlag, bool frameFlag)
		{
			this.newFlag = newFlag;
			this.frameFlag = frameFlag;
			this.shape = shape;
		}
	}

	private class ShapeData
	{
		public enum Type
		{
			Sphere = 0,
			Hemisphere = 1,
			Capsule = 2,
			Hemicapsule = 3,
			Cylinder = 4,
			Box = 5,
			Cone = 6
		}

		public struct SphereShapeData
		{
			public SphereShape sphereShape;

			public Vector3 worldCenter;

			public float worldRadius;
		}

		public struct HemisphereShapeData
		{
			public HemisphereShape hemisphereShape;

			public Vector3 worldAxis;
		}

		public struct CapsuleShapeData
		{
			public CapsuleShape capsuleShape;

			public Vector3 worldStartPoint;

			public Vector3 worldEndPoint;

			public float worldRadius;
		}

		public struct HemicapsuleShapeData
		{
			public HemicapsuleShape hemicapsuleShape;
		}

		public struct CylinderShapeData
		{
			public CylinderShape cylinderShape;
		}

		public struct ConeShapeData
		{
			public ConeShape coneShape;

			public Vector3 worldStartPoint;

			public Vector3 worldEndPoint;

			public float worldStartRadius;

			public float worldEndRadius;
		}

		public struct BoxShapeData
		{
			public BoxShape boxShape;

			public Vector3 worldCenter;

			public Vector3 worldSize;

			public Vector3[] worldAxes;

			public Vector3[] worldVertices;
		}

		public Shape shape;

		public Type type;

		public List<ContactData> contacts;

		public Vector3 worldBoundsCenter;

		public float worldBoundsRadius;

		public bool shapeDataDirty;

		public SphereShapeData sphere;

		public HemisphereShapeData hemisphere;

		public CapsuleShapeData capsule;

		public HemicapsuleShapeData hemicapsule;

		public CylinderShapeData cylinder;

		public ConeShapeData cone;

		public BoxShapeData box;

		public void Set(Shape shape)
		{
			this.shape = shape;
			if (shape is SphereShape)
			{
				type = Type.Sphere;
				sphere.sphereShape = shape as SphereShape;
				if (shape is HemisphereShape)
				{
					type = Type.Hemisphere;
					hemisphere.hemisphereShape = shape as HemisphereShape;
				}
			}
			else if (shape is CapsuleShape)
			{
				type = Type.Capsule;
				capsule.capsuleShape = shape as CapsuleShape;
				if (shape is HemicapsuleShape)
				{
					type = Type.Hemicapsule;
					hemicapsule.hemicapsuleShape = shape as HemicapsuleShape;
				}
				else if (shape is CylinderShape)
				{
					type = Type.Cylinder;
					cylinder.cylinderShape = shape as CylinderShape;
				}
			}
			else if (shape is ConeShape)
			{
				type = Type.Cone;
				cone.coneShape = shape as ConeShape;
			}
			else if (shape is BoxShape)
			{
				type = Type.Box;
				box.boxShape = shape as BoxShape;
			}
		}
	}

	private const int kMaxDetectors = 256;

	private const int kMaxVolumes = 1024;

	private static bool _exists = false;

	private static Layer _detectors = null;

	private static Layer[] _volumes = null;

	private static bool _locked = false;

	private static List<Shape> _pendingShapeAdditions = new List<Shape>(128);

	private static Queue<Shape> _pendingShapeRemovals = new Queue<Shape>(128);

	private static bool _frameFlag = false;

	private static Queue<QuantumObject> s_quantumRetryList = new Queue<QuantumObject>(128);

	public static bool exists => _exists;

	private static void Initialize()
	{
		_exists = true;
		_detectors = new Layer(256);
		for (int i = 0; i < 256; i++)
		{
			_detectors[i].contacts = new List<ContactData>(64);
		}
		_volumes = new Layer[4];
		for (int j = 0; j < 4; j++)
		{
			_volumes[j] = new Layer(1024);
		}
		_locked = false;
		_frameFlag = false;
	}

	private void Awake()
	{
		if (!_exists)
		{
			Initialize();
		}
	}

	private void OnDestroy()
	{
		_detectors = null;
		_volumes = null;
		_pendingShapeAdditions.Clear();
		_pendingShapeRemovals.Clear();
		s_quantumRetryList.Clear();
		_exists = false;
	}

	public static void RegisterShape(Shape shape)
	{
		if (!_exists)
		{
			Initialize();
		}
		_pendingShapeAdditions.Add(shape);
	}

	public static void UnregisterShape(Shape shape)
	{
		if (!_exists)
		{
			return;
		}
		for (int i = 0; i < _pendingShapeAdditions.Count; i++)
		{
			if (_pendingShapeAdditions[i] == shape)
			{
				_pendingShapeAdditions.QuickRemoveAt(i);
				return;
			}
		}
		if (_locked)
		{
			_pendingShapeRemovals.Enqueue(shape);
			return;
		}
		_locked = true;
		RemoveShapeAndContacts(shape);
		while (_pendingShapeRemovals.Count > 0)
		{
			RemoveShapeAndContacts(_pendingShapeRemovals.Dequeue());
		}
		_locked = false;
	}

	public static void AddToRetryQueue(QuantumObject obj)
	{
		if (s_quantumRetryList == null)
		{
			s_quantumRetryList = new Queue<QuantumObject>(128);
		}
		if (!s_quantumRetryList.Contains(obj))
		{
			s_quantumRetryList.Enqueue(obj);
		}
	}

	private static void AddShape(Shape shape)
	{
		if (shape.collisionMode == Shape.CollisionMode.Detector)
		{
			_detectors.Add(shape);
		}
		else if (shape.collisionMode == Shape.CollisionMode.Volume)
		{
			int num = ShapeUtil.LayerToIndex(shape.layer);
			_volumes[num].Add(shape);
		}
	}

	private static void RemoveShapeAndContacts(Shape shape)
	{
		if (shape.collisionMode == Shape.CollisionMode.Detector)
		{
			int index = _detectors.IndexOf(shape);
			List<ContactData> contacts = _detectors[index].contacts;
			for (int num = contacts.Count - 1; num >= 0; num--)
			{
				shape.FireCollisionExitEvent(contacts[num].shape);
				contacts[num].shape.FireCollisionExitEvent(shape);
			}
			contacts.Clear();
			_detectors.RemoveAt(index);
		}
		else
		{
			if (shape.collisionMode != 0)
			{
				return;
			}
			for (int i = 0; i < _detectors.Count; i++)
			{
				if (((uint)_detectors[i].shape.layerMask & (uint)shape.layer) == 0)
				{
					continue;
				}
				List<ContactData> contacts2 = _detectors[i].contacts;
				for (int num2 = contacts2.Count - 1; num2 >= 0; num2--)
				{
					if (contacts2[num2].shape == shape)
					{
						_detectors[i].shape.FireCollisionExitEvent(shape);
						shape.FireCollisionExitEvent(_detectors[i].shape);
						contacts2.QuickRemoveAt(num2);
					}
				}
			}
			int num3 = ShapeUtil.LayerToIndex(shape.layer);
			_volumes[num3].Remove(shape);
		}
	}

	private void FixedUpdate()
	{
		_frameFlag = !_frameFlag;
		for (int i = 0; i < _pendingShapeAdditions.Count; i++)
		{
			AddShape(_pendingShapeAdditions[i]);
		}
		_pendingShapeAdditions.Clear();
		UpdateWorldBounds(_detectors);
		for (int j = 0; j < 4; j++)
		{
			UpdateWorldBounds(_volumes[j]);
		}
		for (int k = 0; k < _detectors.Count; k++)
		{
			ShapeData shapeData = _detectors[k];
			for (int l = 0; l < 4; l++)
			{
				if ((shapeData.shape.layerMask & (1 << l)) == 0)
				{
					continue;
				}
				for (int m = 0; m < _volumes[l].Count; m++)
				{
					ShapeData shapeData2 = _volumes[l][m];
					if (!((!shapeData2.shape.pointChecksOnly) ? TestCollision(shapeData, shapeData2) : TestPointInside(shapeData, shapeData2)))
					{
						continue;
					}
					bool flag = false;
					for (int n = 0; n < shapeData.contacts.Count; n++)
					{
						if (shapeData.contacts[n].shape == shapeData2.shape)
						{
							shapeData.contacts[n] = new ContactData(shapeData2.shape, shapeData.contacts[n].newFlag, _frameFlag);
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						shapeData.contacts.Add(new ContactData(shapeData2.shape, newFlag: true, _frameFlag));
					}
				}
			}
		}
		_locked = true;
		for (int num = 0; num < _detectors.Count; num++)
		{
			List<ContactData> contacts = _detectors[num].contacts;
			for (int num2 = contacts.Count - 1; num2 >= 0; num2--)
			{
				if (contacts[num2].newFlag)
				{
					_detectors[num].shape.FireCollisionEnterEvent(contacts[num2].shape);
					contacts[num2].shape.FireCollisionEnterEvent(_detectors[num].shape);
					contacts[num2] = new ContactData(contacts[num2].shape, newFlag: false, _frameFlag);
				}
				if (contacts[num2].frameFlag != _frameFlag)
				{
					_detectors[num].shape.FireCollisionExitEvent(contacts[num2].shape);
					contacts[num2].shape.FireCollisionExitEvent(_detectors[num].shape);
					contacts.QuickRemoveAt(num2);
				}
			}
		}
		while (_pendingShapeRemovals.Count > 0)
		{
			RemoveShapeAndContacts(_pendingShapeRemovals.Dequeue());
		}
		_locked = false;
		if (s_quantumRetryList != null && s_quantumRetryList.Count > 0)
		{
			QuantumObject quantumObject = s_quantumRetryList.Dequeue();
			if (!quantumObject.IsLocked() && !quantumObject.Retry())
			{
				s_quantumRetryList.Enqueue(quantumObject);
			}
		}
	}

	private void UpdateWorldBounds(Layer layer)
	{
		for (int i = 0; i < layer.Count; i++)
		{
			ShapeData shapeData = layer[i];
			SphereBounds sphereBounds = shapeData.shape.CalcWorldBounds();
			shapeData.worldBoundsCenter = sphereBounds.center;
			shapeData.worldBoundsRadius = sphereBounds.radius;
			shapeData.shapeDataDirty = true;
		}
	}

	private void UpdateWorldShapeData(ShapeData shapeData)
	{
		float worldSpaceRadius;
		Vector3 worldSpaceP3;
		Vector3 worldSpaceP4;
		switch (shapeData.type)
		{
		case ShapeData.Type.Sphere:
			shapeData.sphere.worldCenter = ShapeUtil.Sphere.CalcWorldSpaceCenter(shapeData.sphere.sphereShape);
			shapeData.sphere.worldRadius = ShapeUtil.Sphere.CalcWorldSpaceRadius(shapeData.sphere.sphereShape);
			break;
		case ShapeData.Type.Hemisphere:
			shapeData.sphere.worldCenter = ShapeUtil.Sphere.CalcWorldSpaceCenter(shapeData.hemisphere.hemisphereShape);
			shapeData.sphere.worldRadius = ShapeUtil.Sphere.CalcWorldSpaceRadius(shapeData.hemisphere.hemisphereShape);
			shapeData.hemisphere.worldAxis = ShapeUtil.Hemisphere.CalcWorldSpaceAxis(shapeData.hemisphere.hemisphereShape);
			break;
		case ShapeData.Type.Capsule:
		case ShapeData.Type.Hemicapsule:
			ShapeUtil.Capsule.CalcWorldSpaceEndpoints(shapeData.capsule.capsuleShape, out worldSpaceRadius, out worldSpaceP3, out worldSpaceP4);
			shapeData.capsule.worldStartPoint = worldSpaceP3;
			shapeData.capsule.worldEndPoint = worldSpaceP4;
			shapeData.capsule.worldRadius = worldSpaceRadius;
			break;
		case ShapeData.Type.Cylinder:
			ShapeUtil.Cylinder.CalcWorldSpaceEndpoints(shapeData.cylinder.cylinderShape, out worldSpaceRadius, out worldSpaceP3, out worldSpaceP4);
			shapeData.capsule.worldStartPoint = worldSpaceP3;
			shapeData.capsule.worldEndPoint = worldSpaceP4;
			shapeData.capsule.worldRadius = worldSpaceRadius;
			break;
		case ShapeData.Type.Cone:
		{
			ShapeUtil.Cone.CalcWorldSpaceEndpoints(shapeData.cone.coneShape, out var worldSpaceTopRadius, out var worldSpaceBottomRadius, out var worldSpaceP, out var worldSpaceP2);
			shapeData.cone.worldStartPoint = worldSpaceP;
			shapeData.cone.worldEndPoint = worldSpaceP2;
			shapeData.cone.worldStartRadius = worldSpaceTopRadius;
			shapeData.cone.worldEndRadius = worldSpaceBottomRadius;
			break;
		}
		case ShapeData.Type.Box:
		{
			ShapeUtil.Box.CalcWorldSpaceData(shapeData.box.boxShape, out var center, out var size, ref shapeData.box.worldAxes, ref shapeData.box.worldVertices);
			shapeData.box.worldCenter = center;
			shapeData.box.worldSize = size;
			break;
		}
		}
		shapeData.shapeDataDirty = false;
	}

	private bool TestPointInside(ShapeData detector, ShapeData volume)
	{
		if (PointInside.Sphere(detector.worldBoundsCenter, volume.worldBoundsCenter, volume.worldBoundsRadius))
		{
			if (volume.shapeDataDirty)
			{
				UpdateWorldShapeData(volume);
			}
			switch (volume.type)
			{
			case ShapeData.Type.Sphere:
				return true;
			case ShapeData.Type.Hemisphere:
				return PointInside.Hemisphere(detector.worldBoundsCenter, volume.sphere.worldCenter, volume.sphere.worldRadius, volume.hemisphere.worldAxis);
			case ShapeData.Type.Capsule:
				return PointInside.Capsule(detector.worldBoundsCenter, volume.capsule.worldStartPoint, volume.capsule.worldEndPoint, volume.capsule.worldRadius);
			case ShapeData.Type.Hemicapsule:
				return PointInside.Hemicapsule(detector.worldBoundsCenter, volume.capsule.worldStartPoint, volume.capsule.worldEndPoint, volume.capsule.worldRadius, volume.hemicapsule.hemicapsuleShape.cap);
			case ShapeData.Type.Cylinder:
				return PointInside.Cylinder(detector.worldBoundsCenter, volume.capsule.worldStartPoint, volume.capsule.worldEndPoint, volume.capsule.worldRadius);
			case ShapeData.Type.Cone:
				return PointInside.Cone(detector.worldBoundsCenter, volume.cone.worldStartPoint, volume.cone.worldEndPoint, volume.cone.worldStartRadius, volume.cone.worldEndRadius);
			case ShapeData.Type.Box:
				return PointInside.Box(detector.worldBoundsCenter, volume.box.worldCenter, volume.box.worldSize, volume.box.worldAxes);
			}
		}
		return false;
	}

	private bool TestCollision(ShapeData detector, ShapeData volume)
	{
		if (Intersection.SphereSphere(detector.worldBoundsCenter, detector.worldBoundsRadius, volume.worldBoundsCenter, volume.worldBoundsRadius))
		{
			if (detector.shapeDataDirty)
			{
				UpdateWorldShapeData(detector);
			}
			if (volume.shapeDataDirty)
			{
				UpdateWorldShapeData(volume);
			}
			switch (detector.type)
			{
			case ShapeData.Type.Sphere:
				switch (volume.type)
				{
				case ShapeData.Type.Sphere:
					return true;
				case ShapeData.Type.Hemisphere:
					return Intersection.SphereHemisphere(detector.sphere.worldCenter, detector.sphere.worldRadius, volume.sphere.worldCenter, volume.sphere.worldRadius, volume.hemisphere.worldAxis);
				case ShapeData.Type.Capsule:
					return Intersection.SphereCapsule(detector.sphere.worldCenter, detector.sphere.worldRadius, volume.capsule.worldStartPoint, volume.capsule.worldEndPoint, volume.capsule.worldRadius);
				case ShapeData.Type.Hemicapsule:
					return Intersection.SphereHemicapsule(detector.sphere.worldCenter, detector.sphere.worldRadius, volume.capsule.worldStartPoint, volume.capsule.worldEndPoint, volume.capsule.worldRadius, volume.hemicapsule.hemicapsuleShape.cap);
				case ShapeData.Type.Cylinder:
					return Intersection.SphereCylinder(detector.sphere.worldCenter, detector.sphere.worldRadius, volume.capsule.worldStartPoint, volume.capsule.worldEndPoint, volume.capsule.worldRadius);
				case ShapeData.Type.Cone:
					return Intersection.SphereCone(detector.sphere.worldCenter, detector.sphere.worldRadius, volume.cone.worldStartPoint, volume.cone.worldEndPoint, volume.cone.worldStartRadius, volume.cone.worldEndRadius);
				case ShapeData.Type.Box:
					return Intersection.SphereBox(detector.sphere.worldCenter, detector.sphere.worldRadius, volume.box.worldCenter, volume.box.worldSize, volume.box.worldAxes);
				}
				break;
			case ShapeData.Type.Capsule:
				switch (volume.type)
				{
				case ShapeData.Type.Sphere:
					return Intersection.SphereCapsule(volume.sphere.worldCenter, volume.sphere.worldRadius, detector.capsule.worldStartPoint, detector.capsule.worldEndPoint, detector.capsule.worldRadius);
				case ShapeData.Type.Capsule:
					return Intersection.CapsuleCapsule(detector.capsule.worldStartPoint, detector.capsule.worldEndPoint, detector.capsule.worldRadius, volume.capsule.worldStartPoint, volume.capsule.worldEndPoint, volume.capsule.worldRadius);
				case ShapeData.Type.Box:
					return Intersection.CapsuleBox(detector.capsule.worldStartPoint, detector.capsule.worldEndPoint, detector.capsule.worldRadius, volume.box.worldCenter, volume.box.worldSize, volume.box.worldAxes, volume.box.worldVertices);
				}
				break;
			case ShapeData.Type.Box:
				switch (volume.type)
				{
				case ShapeData.Type.Sphere:
					return Intersection.SphereBox(volume.sphere.worldCenter, volume.sphere.worldRadius, detector.box.worldCenter, detector.box.worldSize, detector.box.worldAxes);
				case ShapeData.Type.Capsule:
					return Intersection.CapsuleBox(volume.capsule.worldStartPoint, volume.capsule.worldEndPoint, volume.capsule.worldRadius, detector.box.worldCenter, detector.box.worldSize, detector.box.worldAxes, detector.box.worldVertices);
				case ShapeData.Type.Box:
					return Intersection.BoxBox(detector.box.worldCenter, detector.box.worldSize, detector.box.worldAxes, detector.box.worldVertices, volume.box.worldCenter, volume.box.worldSize, volume.box.worldAxes, volume.box.worldVertices);
				}
				break;
			}
			Debug.LogError(string.Concat("Collision test between ", detector.type, " and ", volume.type, " not handled."));
			Debug.Break();
		}
		return false;
	}
}
