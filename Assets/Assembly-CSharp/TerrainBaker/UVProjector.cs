using UnityEngine;

namespace TerrainBaker
{
	public abstract class UVProjector : MonoBehaviour
	{
		public abstract Vector2 TransformPointToUV(Vector3 worldSpacePoint);
	}
}
