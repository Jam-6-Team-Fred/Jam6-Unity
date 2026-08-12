using UnityEngine;

public class CockpitDamageEffect : HullDamageEffect
{
	[Space]
	[SerializeField]
	private MeshRenderer _cracksRenderer;

	private ShipAudioController _shipAudioController;

	private int _propID_Cutoff;

	private MaterialPropertyBlock _matPropBlock_Cracks;

	private bool _playerInShip;

	protected override void Awake()
	{
		_shipAudioController = GameObject.FindGameObjectWithTag("Ship").GetComponentInChildren<ShipAudioController>();
		_propID_Cutoff = Shader.PropertyToID("_Cutoff");
		_matPropBlock_Cracks = new MaterialPropertyBlock();
		base.Awake();
		_playerInShip = false;
		GlobalMessenger.AddListener("EnterShip", OnEnterShip);
		GlobalMessenger.AddListener("ExitShip", OnExitShip);
		GlobalMessenger.AddListener("ShipHullBreach", OnShipHullBreach);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterShip", OnEnterShip);
		GlobalMessenger.RemoveListener("ExitShip", OnExitShip);
		GlobalMessenger.RemoveListener("ShipHullBreach", OnShipHullBreach);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if ((bool)_cracksRenderer)
		{
			_cracksRenderer.enabled = _playerInShip;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)_cracksRenderer && !_passiveEffectsOnly)
		{
			_cracksRenderer.enabled = false;
		}
	}

	public override void SetEffectBlend(float blend)
	{
		float blend2 = _blend;
		base.SetEffectBlend(blend);
		if (_shipAudioController != null && _blend > blend2)
		{
			_shipAudioController.PlayGlassCrackClip();
		}
		if ((bool)_cracksRenderer)
		{
			_matPropBlock_Cracks.SetFloat(_propID_Cutoff, 1f - _blend);
			_cracksRenderer.SetPropertyBlock(_matPropBlock_Cracks);
		}
	}

	private void OnEnterShip()
	{
		_playerInShip = true;
		if ((bool)_cracksRenderer && _matPropBlock_Cracks.GetFloat(_propID_Cutoff) < 1f)
		{
			_cracksRenderer.enabled = true;
		}
	}

	private void OnExitShip()
	{
		_playerInShip = false;
		if ((bool)_cracksRenderer)
		{
			_cracksRenderer.enabled = false;
		}
	}

	private void OnShipHullBreach()
	{
		if ((bool)_cracksRenderer && _matPropBlock_Cracks.GetFloat(_propID_Cutoff) < 1f)
		{
			_cracksRenderer.enabled = true;
		}
	}
}
