using UnityEngine;

namespace TerrainBaker
{
	public class UVProjectorPlanar : UVProjector
	{
		public enum Plane
		{
			X = 0,
			Y = 1,
			Z = 2
		}

		[SerializeField]
		protected Plane _plane = Plane.Z;

		[SerializeField]
		protected Vector2 _scale = Vector2.one;

		public Plane plane
		{
			get
			{
				return _plane;
			}
			set
			{
				_plane = value;
			}
		}

		public Vector2 scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
			}
		}

		public override Vector2 TransformPointToUV(Vector3 worldSpacePoint)
		{
			Vector3 vector = base.transform.InverseTransformPoint(worldSpacePoint);
			switch (_plane)
			{
			case Plane.X:
				return new Vector2(vector.z / _scale.x + 0.5f, vector.y / _scale.y + 0.5f);
			case Plane.Y:
				return new Vector2(vector.x / _scale.x + 0.5f, vector.z / _scale.y + 0.5f);
			case Plane.Z:
				return new Vector2(vector.x / _scale.x + 0.5f, vector.y / _scale.y + 0.5f);
			default:
				return new Vector2(vector.x / _scale.x + 0.5f, vector.y / _scale.y + 0.5f);
			}
		}
	}
}
