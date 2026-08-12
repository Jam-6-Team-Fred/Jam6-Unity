using UnityEngine;

[RequireComponent(typeof(VectionFieldEmitter))]
public class SupernovaStreamersController : MonoBehaviour
{
	private VectionFieldEmitter _vectionFieldEmitter;

	[SerializeField]
	private float _playDist = 10000f;

	private SunController _sunController;

	private void Awake()
	{
		_vectionFieldEmitter = GetComponent<VectionFieldEmitter>();
		_sunController = this.GetAttachedOWRigidbody().GetComponent<SunController>();
		_sunController.OnSupernovaStart += OnSupernovaStart;
		GlobalMessenger.AddListener("FlashbackStart", OnFlashbackStart);
		_vectionFieldEmitter.enabled = false;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_sunController != null)
		{
			_sunController.OnSupernovaStart -= OnSupernovaStart;
		}
		GlobalMessenger.RemoveListener("FlashbackStart", OnFlashbackStart);
	}

	private void OnSupernovaStart()
	{
		base.enabled = true;
	}

	private void OnFlashbackStart()
	{
		base.enabled = false;
	}

	private void SetParentHelper(Transform parent)
	{
		if (base.transform.parent != parent)
		{
			base.transform.SetParent(parent);
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
		}
	}

	private void LateUpdate()
	{
		OWCamera activeCamera = Locator.GetActiveCamera();
		Vector3 vector = _sunController.transform.position - activeCamera.transform.position;
		float magnitude = vector.magnitude;
		vector /= magnitude;
		OWRigidbody attachedOWRigidbody = activeCamera.GetAttachedOWRigidbody();
		if (attachedOWRigidbody != null)
		{
			SectorDetector componentInChildren = attachedOWRigidbody.GetComponentInChildren<SectorDetector>();
			if (componentInChildren != null)
			{
				Sector lastEnteredSector = componentInChildren.GetLastEnteredSector();
				if (lastEnteredSector != null && lastEnteredSector.GetName() == Sector.Name.Ship)
				{
					lastEnteredSector = Locator.GetShipDetector().GetComponent<SectorDetector>().GetLastEnteredSector();
				}
				if (lastEnteredSector != null)
				{
					SetParentHelper(lastEnteredSector.GetOWRigidbody().transform);
				}
				else
				{
					SetParentHelper(_sunController.transform);
				}
			}
			else
			{
				SetParentHelper(attachedOWRigidbody.transform);
			}
		}
		else
		{
			SetParentHelper(_sunController.transform);
		}
		float supernovaRadius = _sunController.GetSupernovaRadius();
		bool flag = Mathf.Abs(magnitude - supernovaRadius) < _playDist;
		_vectionFieldEmitter.emitterTransform = activeCamera.transform;
		_vectionFieldEmitter.directionalDir = base.transform.InverseTransformDirection(-vector);
		_vectionFieldEmitter.enabled = flag;
	}
}
