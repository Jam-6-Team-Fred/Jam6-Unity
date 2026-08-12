using UnityEngine;

public abstract class RaftCarrier : MonoBehaviour
{
	protected enum DockState
	{
		Ready = 0,
		AligningBelow = 1,
		LiftDelay = 2,
		Lifting = 3,
		Docked = 4,
		WaitForExit = 5,
		ResettingHook = 6,
		ResettingCrane = 7
	}

	[SerializeField]
	protected OWTriggerVolume _trigger;

	[SerializeField]
	protected Transform _craneChains;

	[SerializeField]
	protected Transform _craneHookRoot;

	[SerializeField]
	protected float _craneHookChainOffset;

	[SerializeField]
	protected float _hookStartLocalY;

	[SerializeField]
	protected float _raftAlignSpeed = 4f;

	[SerializeField]
	protected float _liftingDelay = 0.25f;

	[SerializeField]
	protected Vector2 _chainTilingModifier;

	[SerializeField]
	protected OWAudioSource _oneShotAudio;

	[SerializeField]
	protected OWAudioSource _loopingAudio;

	[SerializeField]
	protected Animator _hooksAnimator;

	protected DockState _state;

	protected RaftController _raft;

	protected Vector3 _hookStartPosLocal;

	private float _hookChainStartOffset;

	private Transform _origHookParent;

	private MeshRenderer _chainsRenderer;

	private Vector2 _origChainUVTile;

	private float _liftDelayStartTime;

	private DockState _prevState;

	private MaterialPropertyBlock _chainsPropertyBlock;

	protected virtual void Awake()
	{
		_trigger.OnEntry += OnEntry;
		_chainsPropertyBlock = new MaterialPropertyBlock();
	}

	protected virtual void Start()
	{
		if (_craneHookRoot != null)
		{
			_hookChainStartOffset = Mathf.Abs(Vector3.Magnitude(_craneChains.localPosition - _craneHookRoot.localPosition)) - _craneHookChainOffset;
			Vector3 localPosition = _craneHookRoot.localPosition;
			_craneHookRoot.localPosition = new Vector3(localPosition.x, _hookStartLocalY, localPosition.z);
			_hookStartPosLocal = _craneHookRoot.localPosition;
			_origHookParent = _craneHookRoot.parent;
			_chainsRenderer = _craneChains.GetComponent<MeshRenderer>();
			_origChainUVTile = Vector2.one;
			UpdateChainScale();
		}
	}

	protected virtual void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
	}

	public void PlayHookAnimation()
	{
		if (_hooksAnimator != null)
		{
			_hooksAnimator.SetTrigger("Hook");
		}
	}

	public bool MoveHookToReturn(float speed)
	{
		Vector3 vector = Vector3.Project(_hookStartPosLocal - _craneHookRoot.localPosition, Vector3.up);
		Vector3 vector2 = vector.normalized * Time.deltaTime * speed;
		bool result = false;
		if (vector.sqrMagnitude < vector2.sqrMagnitude)
		{
			vector2 = vector;
			result = true;
		}
		_craneHookRoot.localPosition += vector2;
		UpdateChainScale();
		return result;
	}

	protected abstract Transform GetAlignDestination();

	protected abstract void MoveAfterAlign();

	protected virtual void FixedUpdate()
	{
		if (_state == DockState.LiftDelay)
		{
			_raft.SetZeroVelocity();
			if (Time.time >= _liftDelayStartTime + _liftingDelay)
			{
				_raft.EnableForces();
				MoveAfterAlign();
				_state = DockState.Lifting;
			}
		}
		if (_state == DockState.AligningBelow)
		{
			Vector3 vector = Vector3.Project(_craneHookRoot.parent.InverseTransformPoint(_raft.transform.position) - _hookStartPosLocal, Vector3.up);
			_craneHookRoot.localPosition = Vector3.Lerp(_hookStartPosLocal, _hookStartPosLocal + vector, _raft.currentDistanceLerp);
			UpdateChainScale();
		}
		else if (_state == DockState.Lifting)
		{
			if (_prevState == DockState.LiftDelay)
			{
				_craneHookRoot.parent = _raft.transform;
			}
			UpdateChainScale();
		}
		if (_state != DockState.Lifting && _prevState == DockState.Lifting)
		{
			_craneHookRoot.parent = _origHookParent;
		}
		_prevState = _state;
	}

	protected virtual void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("RaftDetector") && _state == DockState.Ready)
		{
			_raft = hitObj.GetComponentInParent<RaftController>();
			_raft.OnArriveAtTarget += new OWEvent.OWCallback(OnArriveAtTarget);
			GetAlignDestination().localEulerAngles = Vector3.zero;
			Vector3 to = GetAlignDestination().InverseTransformDirection(_raft.transform.forward);
			to.y = 0f;
			float value = OWMath.Angle(Vector3.forward, to, Vector3.up);
			value = OWMath.RoundToNearestMultiple(value, 90f);
			GetAlignDestination().localEulerAngles = new Vector3(0f, value, 0f);
			Vector3 vector = GetAlignDestination().position - _raft.GetBody().GetPosition();
			vector = Vector3.Project(vector, _raft.transform.up);
			Vector3 position = GetAlignDestination().position - GetAlignDestination().up * vector.magnitude;
			_raft.MoveToTarget(position, GetAlignDestination().rotation, _raftAlignSpeed, reenableForcesAfter: false);
			_oneShotAudio.PlayOneShot(AudioType.Raft_Reel_Start);
			_loopingAudio.FadeIn(0.2f);
			_state = DockState.AligningBelow;
		}
	}

	protected virtual void OnArriveAtTarget()
	{
		if (_state == DockState.AligningBelow)
		{
			_liftDelayStartTime = Time.time;
			_state = DockState.LiftDelay;
			_raft.SetZeroVelocity();
			PlayHookAnimation();
		}
	}

	private void UpdateChainScale()
	{
		_chainsRenderer.GetPropertyBlock(_chainsPropertyBlock);
		float num = (Mathf.Abs(Vector3.Magnitude(_craneChains.position - _craneHookRoot.position)) - _craneHookChainOffset) / _hookChainStartOffset;
		_chainsPropertyBlock.SetVector("_MainTex_ST", new Vector4(Mathf.Lerp(_origChainUVTile.x, _origChainUVTile.x * num, _chainTilingModifier.x), Mathf.Lerp(_origChainUVTile.x, _origChainUVTile.y * num, _chainTilingModifier.y), 0f, 0f));
		_chainsRenderer.SetPropertyBlock(_chainsPropertyBlock);
		_craneChains.localScale = new Vector3(1f, num, 1f);
	}
}
