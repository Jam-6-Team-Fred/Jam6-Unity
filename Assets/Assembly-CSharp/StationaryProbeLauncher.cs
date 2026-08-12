using UnityEngine;

public class StationaryProbeLauncher : ProbeLauncher
{
	[SerializeField]
	private Transform _verticalPivot;

	[SerializeField]
	private VisibilityTracker _targetVisibilityTracker;

	[SerializeField]
	private bool _photosOnly;

	[SerializeField]
	private bool _lockInputY;

	[SerializeField]
	private bool _returnToStartPos;

	[SerializeField]
	private float _initialDegreesY;

	[SerializeField]
	private OWAudioSource _audioSource;

	private PlayerAttachPoint _attachPoint;

	private SingleInteractionVolume _interactVolume;

	private Quaternion _initRotX;

	private Quaternion _initRotY;

	private float _degreesX;

	private float _degreesY;

	private Vector3 _localUpAxis;

	private bool _returningToStartPos;

	protected override void Awake()
	{
		base.Awake();
		base.OnTakeSnapshot += OnSnapshot;
		if (_targetVisibilityTracker != null)
		{
			_targetVisibilityTracker.enabled = false;
		}
		else
		{
			Debug.LogError("Missing target visibility tracker.", this);
		}
		_interactVolume = GetComponentInChildren<SingleInteractionVolume>();
		_interactVolume.OnPressInteract += OnPressInteract;
		_attachPoint = GetComponentInChildren<PlayerAttachPoint>();
		_initRotX = base.transform.localRotation;
		_initRotY = _verticalPivot.localRotation;
		_localUpAxis = base.transform.parent.InverseTransformDirection(base.transform.up);
		_degreesY = _initialDegreesY;
		base.transform.localRotation = Quaternion.AngleAxis(_degreesX, _localUpAxis) * _initRotX;
		_verticalPivot.localRotation = Quaternion.AngleAxis(_degreesY, -Vector3.right) * _initRotY;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_interactVolume != null)
		{
			_interactVolume.OnPressInteract -= OnPressInteract;
		}
		base.OnTakeSnapshot -= OnSnapshot;
	}

	public override bool AllowLaunchMode()
	{
		return !_photosOnly;
	}

	protected override bool AllowInput()
	{
		if (IsEquipped())
		{
			return OWInput.IsInputMode(InputMode.StationaryProbeLauncher);
		}
		return false;
	}

	private void OnSnapshot(RenderTexture snapshot, ProbeCamera camera, float secondsAnchored)
	{
		if (_targetVisibilityTracker != null && _targetVisibilityTracker.IsVisibleToProbe(camera.GetOWCamera()) && (_targetVisibilityTracker.transform.position - camera.transform.position).magnitude < 200f)
		{
			MonoBehaviour.print("Snapshot of Impact Site");
			DialogueConditionManager.SharedInstance.SetConditionState("YesPicture", conditionState: true);
		}
	}

	private void OnPressInteract()
	{
		Locator.GetToolModeSwapper().UnequipTool();
		EquipTool();
		if (_targetVisibilityTracker != null)
		{
			_targetVisibilityTracker.enabled = true;
		}
		_attachPoint.AttachPlayer();
		_returningToStartPos = false;
		OWInput.ChangeInputMode(InputMode.StationaryProbeLauncher);
		_audioSource.SetLocalVolume(0f);
		_audioSource.Play();
		if (Locator.GetProbe().IsLaunched() && !Locator.GetProbe().IsRetrieving())
		{
			RetrieveProbe(playEffects: false);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!AllowInput())
		{
			return;
		}
		UpdateRotation();
		if (!_returningToStartPos && OWInput.IsNewlyPressed(InputLibrary.cancel) && _activeProbe == null)
		{
			RetrieveProbe();
			if (_returnToStartPos)
			{
				_returningToStartPos = true;
			}
			else
			{
				FinishExitSequence();
			}
			if (_targetVisibilityTracker != null)
			{
				_targetVisibilityTracker.enabled = false;
			}
		}
	}

	protected override bool UpdateOnEquip()
	{
		return true;
	}

	public override bool HasEquipAnimation()
	{
		return false;
	}

	private void FinishExitSequence()
	{
		_audioSource.Stop();
		_attachPoint.DetachPlayer();
		_interactVolume.ResetInteraction();
		OWInput.ChangeInputMode(InputMode.Character);
		UnequipTool();
	}

	private void UpdateRotation()
	{
		Vector2 axisValue = OWInput.GetAxisValue(InputLibrary.look);
		float x = axisValue.x;
		float num = (_lockInputY ? 0f : axisValue.y);
		float localVolume = (_returningToStartPos ? 1f : Mathf.Max(Mathf.Abs(x), Mathf.Abs(num)));
		_audioSource.SetLocalVolume(localVolume);
		if (_returningToStartPos)
		{
			_degreesX = Mathf.MoveTowards(_degreesX, 0f, Time.deltaTime * 80f);
			_degreesY = Mathf.MoveTowards(_degreesY, _initialDegreesY, Time.deltaTime * 80f);
			if (_degreesX == 0f && _degreesY == _initialDegreesY)
			{
				FinishExitSequence();
			}
		}
		else
		{
			_degreesX += x * 50f * Time.deltaTime;
			if (_degreesX > 180f)
			{
				_degreesX -= 360f;
			}
			else if (_degreesX < -180f)
			{
				_degreesX += 360f;
			}
			_degreesY = Mathf.Clamp(_degreesY + num * 50f * Time.deltaTime, -10f, 30f);
		}
		base.transform.localRotation = Quaternion.AngleAxis(_degreesX, _localUpAxis) * _initRotX;
		_verticalPivot.localRotation = Quaternion.AngleAxis(_degreesY, -Vector3.right) * _initRotY;
	}
}
