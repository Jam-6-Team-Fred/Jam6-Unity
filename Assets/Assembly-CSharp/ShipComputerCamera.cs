using UnityEngine;

public class ShipComputerCamera : MonoBehaviour
{
	private Camera _camera;

	private void Awake()
	{
		_camera = GetComponent<Camera>();
		_camera.aspect = 1.3333334f;
	}

	public void UpdatePosition(Vector3 targetPos, float targetOrthoSize)
	{
		Vector3 vector = targetPos - base.transform.localPosition;
		vector -= Vector3.Project(vector, Vector3.forward);
		base.transform.localPosition += vector * Time.deltaTime * 5f;
		float num = targetOrthoSize - _camera.orthographicSize;
		_camera.orthographicSize += num * Time.deltaTime * 5f;
	}

	public void SetPosition(Vector3 position, float orthoSize)
	{
		base.transform.localPosition = new Vector3(position.x, position.y, base.transform.localPosition.z);
		_camera.orthographicSize = orthoSize;
	}

	public float GetOrthoSize()
	{
		return _camera.orthographicSize;
	}
}
