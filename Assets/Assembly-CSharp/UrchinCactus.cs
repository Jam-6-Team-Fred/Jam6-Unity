using UnityEngine;

public class UrchinCactus : MonoBehaviour
{
	[SerializeField]
	private float _retractedRadius = 1f;

	[SerializeField]
	private float _extendedRadius = 3f;

	[SerializeField]
	private bool _previewExtended = true;

	[SerializeField]
	private GameObject _urchinModel;

	[SerializeField]
	private SphereCollider _hazardCollider;

	[SerializeField]
	private SphereCollider _physicalCollider;

	[SerializeField]
	private SandLevelController _sandSphere;

	private float _extension = 1f;

	private float _distFromPlanetCenter;

	private void Awake()
	{
		OWRigidbody attachedOWRigidbody = this.GetAttachedOWRigidbody();
		_distFromPlanetCenter = Vector3.Distance(base.transform.position, attachedOWRigidbody.transform.position);
		SetExtension(_extension);
	}

	private void OnValidate()
	{
		SetExtension(_previewExtended ? 1f : 0f);
	}

	private void OnDestroy()
	{
	}

	private void SetExtension(float extension)
	{
		float num = Mathf.Lerp(_retractedRadius, _extendedRadius, extension);
		if (_physicalCollider.radius != num)
		{
			_physicalCollider.radius = num;
		}
		if (_hazardCollider.radius != num + 0.1f)
		{
			_hazardCollider.radius = num + 0.1f;
		}
		if (_urchinModel.transform.localScale != Vector3.one * num)
		{
			_urchinModel.transform.localScale = Vector3.one * num;
		}
	}

	private void FixedUpdate()
	{
		float num = 1f;
		if (_sandSphere != null && _sandSphere.GetRadius() > _distFromPlanetCenter)
		{
			num = 0f;
		}
		float num2 = ((num < _extension) ? 5f : 1f);
		_extension = Mathf.MoveTowards(_extension, num, Time.deltaTime * num2);
		if (num != _extension)
		{
			SetExtension(_extension);
		}
	}
}
