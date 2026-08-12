using UnityEngine;

public class RingRiverPathAudioController : RiverPathAudioController
{
	[Header("River Audio Sources")]
	[SerializeField]
	private RiverPathAudioSource _riverSource;

	[SerializeField]
	private RiverPathAudioSource _reservoirSource;

	[SerializeField]
	private RiverPathAudioSource _coveSource;

	[Header("River Zones")]
	[SerializeField]
	private OWTriggerVolume[] _coveVolumes;

	[SerializeField]
	private float _startReservoirDegrees = 300f;

	private OWRingRiverCollider _riverCollider;

	private int _coveCount;

	private bool _inReservoir;

	protected override void Awake()
	{
		base.Awake();
		_riverAudioSources = new RiverPathAudioSource[3] { _riverSource, _coveSource, _reservoirSource };
		for (int i = 0; i < _coveVolumes.Length; i++)
		{
			_coveVolumes[i].OnEntry += OnEnterCove;
			_coveVolumes[i].OnExit += OnExitCove;
		}
	}

	protected override void Start()
	{
		base.Start();
		_riverCollider = Locator.GetRingRiverFluidVolume().GetCollider();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		for (int i = 0; i < _coveVolumes.Length; i++)
		{
			_coveVolumes[i].OnEntry -= OnEnterCove;
			_coveVolumes[i].OnExit -= OnExitCove;
		}
	}

	protected override RiverPathAudioSource GetDefaultRiverSource()
	{
		return _riverSource;
	}

	protected override void OnActivationUpdated(bool active)
	{
		if (active)
		{
			CheckPlayerInReservoir();
		}
		else
		{
			_inReservoir = false;
		}
	}

	protected override void FixedUpdate()
	{
		CheckPlayerInReservoir();
		base.FixedUpdate();
	}

	private void CheckPlayerInReservoir()
	{
		bool inReservoir = _inReservoir;
		float num = _riverCollider.WorldPositionToDegrees(_playerTransform.position);
		_inReservoir = _riverCollider.GetFloodLerp() <= 0.001f && num > _startReservoirDegrees && num <= 360f;
		if (inReservoir && !_inReservoir)
		{
			SetActiveRiverSource(_riverSource);
		}
		else if (!inReservoir && _inReservoir)
		{
			SetActiveRiverSource(_reservoirSource);
		}
	}

	private void OnEnterCove(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_coveCount++;
			if (_coveCount == 1)
			{
				SetActiveRiverSource(_coveSource);
			}
		}
	}

	private void OnExitCove(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_coveCount = Mathf.Max(_coveCount - 1, 0);
			if (_coveCount == 0)
			{
				SetActiveRiverSource(_riverSource);
			}
		}
	}
}
