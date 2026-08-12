using UnityEngine;

public class DreamTorchRealityProjector : MonoBehaviour
{
	[SerializeField]
	private DreamTorch _dreamTorch;

	[SerializeField]
	private GameObject _lightRoot;

	[SerializeField]
	private GameObject _darkRoot;

	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private OWLightController _lightController;

	private OWCollider[] _lightColliders;

	private OWCollider[] _darkColliders;

	private DitheringAnimator _lightDitherAnimator;

	private DitheringAnimator _darkDitherAnimator;

	private void Awake()
	{
		_dreamTorch.OnTorchLit += new OWEvent<DreamTorch>.OWCallback(OnTorchLit);
		_dreamTorch.OnTorchUnlit += new OWEvent<DreamTorch>.OWCallback(OnTorchUnlit);
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
			_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		}
	}

	private void Start()
	{
		if (_lightController != null)
		{
			_lightController.SetIntensity(_dreamTorch.IsLit() ? 1f : 0f);
		}
		if (_darkRoot != null)
		{
			_darkDitherAnimator = _darkRoot.GetComponent<DitheringAnimator>();
			_darkColliders = _darkRoot.GetComponentsInChildren<OWCollider>();
			bool flag = false;
			_darkDitherAnimator.SetVisibleImmediate(!flag);
			for (int i = 0; i < _darkColliders.Length; i++)
			{
				_darkColliders[i].SetActivation(!flag);
			}
		}
		if (_lightRoot != null)
		{
			_lightDitherAnimator = _lightRoot.GetComponent<DitheringAnimator>();
			_lightColliders = _lightRoot.GetComponentsInChildren<OWCollider>();
			bool flag2 = false;
			_lightDitherAnimator.SetVisibleImmediate(flag2);
			for (int j = 0; j < _lightColliders.Length; j++)
			{
				_lightColliders[j].SetActivation(flag2);
			}
		}
	}

	private void OnDestroy()
	{
		_dreamTorch.OnTorchLit -= new OWEvent<DreamTorch>.OWCallback(OnTorchLit);
		_dreamTorch.OnTorchUnlit -= new OWEvent<DreamTorch>.OWCallback(OnTorchUnlit);
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
			_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
		}
	}

	private void OnTorchLit(DreamTorch torch)
	{
		UpdateVisibility();
	}

	private void OnTorchUnlit(DreamTorch torch)
	{
		UpdateVisibility();
	}

	private void OnDetectLight()
	{
		if (_lightController != null)
		{
			bool flag = true;
			_lightController.FadeTo(flag ? 1f : 0f, flag ? 0.5f : 0.5f);
		}
		if (_lightRoot != null)
		{
			bool flag2 = true;
			_lightDitherAnimator.SetVisible(flag2, 3f);
			for (int i = 0; i < _lightColliders.Length; i++)
			{
				_lightColliders[i].SetActivation(flag2);
			}
		}
	}

	private void OnDetectDarkness()
	{
	}

	private void UpdateVisibility()
	{
		if (_lightController != null)
		{
			bool flag = _dreamTorch.IsLit();
			_lightController.FadeTo(flag ? 1f : 0f, flag ? 0.5f : 0.5f);
		}
		if (_lightRoot != null)
		{
			bool flag2 = _dreamTorch.IsLit();
			_lightDitherAnimator.SetVisible(flag2, 3f);
			for (int i = 0; i < _lightColliders.Length; i++)
			{
				_lightColliders[i].SetActivation(flag2);
			}
		}
		if (_darkRoot != null)
		{
			bool flag3 = _dreamTorch.IsLit();
			_darkDitherAnimator.SetVisible(!flag3, 3f);
			for (int j = 0; j < _darkColliders.Length; j++)
			{
				_darkColliders[j].SetActivation(!flag3);
			}
		}
	}
}
