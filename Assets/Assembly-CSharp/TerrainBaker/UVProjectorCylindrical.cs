using System;
using UnityEngine;

namespace TerrainBaker
{
	public class UVProjectorCylindrical : UVProjector
	{
		[SerializeField]
		protected float _height = 1f;

		[SerializeField]
		protected float _arc = 90f;

		public float height
		{
			get
			{
				return _height;
			}
			set
			{
				_height = value;
			}
		}

		public float arc
		{
			get
			{
				return _arc;
			}
			set
			{
				_arc = value;
			}
		}

		public override Vector2 TransformPointToUV(Vector3 worldSpacePoint)
		{
			Vector3 vector = base.transform.InverseTransformPoint(worldSpacePoint);
			float num = Mathf.Atan2(vector.x, vector.z) / (_arc * ((float)Math.PI / 180f));
			float num2 = vector.y / _height;
			return new Vector2(num + 0.5f, num2 + 0.5f);
		}
	}
}
