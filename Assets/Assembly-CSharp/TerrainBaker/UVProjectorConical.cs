using System;
using UnityEngine;

namespace TerrainBaker
{
	public class UVProjectorConical : UVProjector
	{
		[SerializeField]
		protected float _bottomHeight;

		[SerializeField]
		protected float _topHeight = 1f;

		[SerializeField]
		protected float _arc = 90f;

		public float bottomHeight
		{
			get
			{
				return _bottomHeight;
			}
			set
			{
				_bottomHeight = value;
			}
		}

		public float topHeight
		{
			get
			{
				return _topHeight;
			}
			set
			{
				_topHeight = value;
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
			float y = (vector.magnitude - _bottomHeight) / (_topHeight - _bottomHeight);
			return new Vector2(num + 0.5f, y);
		}
	}
}
