using UnityEngine;

public class RingRiverController : MonoBehaviour
{
	public const float FLOOD_SECONDS = 60f;

	[SerializeField]
	private bool _debugToggleFlood;

	[HideInInspector]
	[SerializeField]
	private float _debugFloodLerp;

	[Space]
	[SerializeField]
	private TessellatedRingRenderer _river;

	[SerializeField]
	private OWRingRiverCollider _riverCollider;

	[SerializeField]
	private EffectRuleset _underwaterEffectRuleset;

	[SerializeField]
	private RingWaveAudioController _waveAudio;

	private int _propID_FloodLerp = Shader.PropertyToID("_FloodLerp");

	private int _propID_CylinderCenter = Shader.PropertyToID("_CylinderCenter");

	private int _propID_CylinderAxis = Shader.PropertyToID("_CylinderAxis");

	private int _propID_CylinderRight = Shader.PropertyToID("_CylinderRight");

	private int _propID_CylinderForward = Shader.PropertyToID("_CylinderForward");

	private Material[] _riverMaterials;

	private bool _updateFlood;

	private bool _floodComplete;

	private float _startFloodTime;

	private void OnValidate()
	{
		if (_debugFloodLerp < 0f)
		{
			_debugFloodLerp = 0f;
		}
		else if (_debugFloodLerp > 1f)
		{
			_debugFloodLerp = 1f;
		}
		if (_debugToggleFlood)
		{
			_debugToggleFlood = false;
			DebugToggleFlood();
		}
	}

	public void DebugToggleFlood()
	{
		_debugFloodLerp = ((_debugFloodLerp > 0.5f) ? 0f : 1f);
		_riverCollider.SetFloodLerp(_debugFloodLerp);
		for (int i = 0; i < _river.sharedMaterials.Length; i++)
		{
			if (_river.sharedMaterials[i] != null)
			{
				_river.sharedMaterials[i].SetFloat(_propID_FloodLerp, _debugFloodLerp);
			}
		}
	}

	private void Awake()
	{
		_riverMaterials = _river.sharedMaterials;
		RingRiverFloodSensor.Initialize(this);
	}

	private void Start()
	{
		_riverCollider.SetFloodLerp(0f);
		for (int i = 0; i < _riverMaterials.Length; i++)
		{
			_riverMaterials[i].SetFloat(_propID_FloodLerp, 0f);
		}
	}

	private void OnDestroy()
	{
		RingRiverFloodSensor.Teardown();
	}

	public void StartFlood()
	{
		_updateFlood = true;
		_startFloodTime = Time.time + 2f;
	}

	private void Update()
	{
		float num = (_floodComplete ? 1f : 0f);
		if (_updateFlood)
		{
			num = Mathf.InverseLerp(_startFloodTime, _startFloodTime + 60f, Time.time);
			if (num >= 0.999f)
			{
				num = 1f;
				_updateFlood = false;
				_floodComplete = true;
			}
			_riverCollider.SetFloodLerp(num);
			_waveAudio.SetFloodLerp(num);
			for (int i = 0; i < _riverMaterials.Length; i++)
			{
				_riverMaterials[i].SetFloat(_propID_FloodLerp, num);
			}
		}
		if (_river != null && _river.gameObject.activeInHierarchy && _river.enabled && _underwaterEffectRuleset != null)
		{
			Material effectBubbleMaterial = _underwaterEffectRuleset.GetEffectBubbleMaterial();
			Transform transform = _river.transform;
			effectBubbleMaterial.SetFloat(_propID_FloodLerp, num);
			effectBubbleMaterial.SetVector(_propID_CylinderCenter, transform.position);
			effectBubbleMaterial.SetVector(_propID_CylinderAxis, transform.up);
			effectBubbleMaterial.SetVector(_propID_CylinderRight, transform.right);
			effectBubbleMaterial.SetVector(_propID_CylinderForward, transform.forward);
		}
	}

	private void FixedUpdate()
	{
		if (_updateFlood)
		{
			float num = Mathf.InverseLerp(_startFloodTime, _startFloodTime + 60f, Time.time);
			_riverCollider.SetFloodLerp(num);
			RingRiverFloodSensor.UpdateFloodTime(num);
		}
	}
}
