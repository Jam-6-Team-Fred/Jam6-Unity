using UnityEngine;

[RequireComponent(typeof(LightSensor))]
public class LightDarkObjectStateSwapper : MonoBehaviour
{
	public static MaterialPropertyBlock s_matPropBlock;

	public static int s_propID_unityLODFade;

	[SerializeField]
	private GameObject _lightStateRoot;

	private Renderer[] _lightRenderers;

	private OWCollider[] _lightColliders;

	private LightSensor _lightSensor;

	private float _lightFraction;

	private void Awake()
	{
		_lightSensor = GetComponent<LightSensor>();
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_unityLODFade = Shader.PropertyToID("unity_LODFade");
		}
		if (_lightStateRoot != null)
		{
			_lightRenderers = _lightStateRoot.GetComponentsInChildren<Renderer>();
			_lightColliders = _lightStateRoot.GetComponentsInChildren<OWCollider>();
		}
	}

	private void Start()
	{
		OnDetectDarkness();
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void Update()
	{
		bool flag = _lightSensor.IsIlluminated();
		float num = (flag ? 10f : 1f);
		_lightFraction = Mathf.MoveTowards(_lightFraction, flag ? 1f : 0f, Time.deltaTime * num);
		UpdateDithering();
	}

	private void UpdateDithering()
	{
		float num = Mathf.Floor(_lightFraction * 16f) / 16f;
		Vector4 value = new Vector4(1f - _lightFraction, 1f - num, 0f, 0f);
		s_matPropBlock.SetVector(s_propID_unityLODFade, value);
		if (_lightRenderers == null)
		{
			return;
		}
		for (int i = 0; i < _lightRenderers.Length; i++)
		{
			if (_lightRenderers[i] != null)
			{
				_lightRenderers[i].SetPropertyBlock(s_matPropBlock);
			}
		}
	}

	private void OnDetectLight()
	{
		if (_lightStateRoot != null)
		{
			for (int i = 0; i < _lightColliders.Length; i++)
			{
				_lightColliders[i].SetActivation(active: true);
			}
		}
		_lightFraction = 1f;
		UpdateDithering();
	}

	private void OnDetectDarkness()
	{
		if (_lightStateRoot != null)
		{
			for (int i = 0; i < _lightColliders.Length; i++)
			{
				_lightColliders[i].SetActivation(active: false);
			}
		}
	}
}
