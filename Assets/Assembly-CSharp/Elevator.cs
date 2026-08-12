using UnityEngine;

public class Elevator : MonoBehaviour
{
	public delegate void PressInteractEvent();

	[SerializeField]
	private float _trackHeight = 10f;

	[SerializeField]
	private float _trackDepth;

	[SerializeField]
	private float _liftDuration = 3f;

	[SerializeField]
	private Animator _animator;

	private bool _hasGoneUp;

	[SerializeField]
	private OWAudioSource _owAudioSourceLP;

	[SerializeField]
	private OWAudioSource _owAudioSourceOneShot;

	[SerializeField]
	private OWTriggerVolume[] _killTriggers;

	private bool _thisScriptControlsActivation = true;

	private SingleInteractionVolume _interactVolume;

	private PlayerAttachPoint _attachPoint;

	private Vector3 _startLocalPos;

	private Vector3 _endLocalPos;

	private bool _goingToTheEnd;

	private Vector3 _initLocalPos;

	private Vector3 _targetLocalPos;

	private float _initLiftTime;

	private int _playerKillTriggerCount;

	public event PressInteractEvent OnPressInteractEvent;

	private void Awake()
	{
		_startLocalPos = base.transform.parent.InverseTransformPoint(base.transform.position - base.transform.up * _trackDepth);
		_endLocalPos = base.transform.parent.InverseTransformPoint(base.transform.position + base.transform.up * _trackHeight);
		_interactVolume = GetComponentInChildren<SingleInteractionVolume>();
		_attachPoint = this.GetRequiredComponentInChildren<PlayerAttachPoint>();
		_interactVolume.OnPressInteract += OnPressInteract;
		for (int i = 0; i < _killTriggers.Length; i++)
		{
			_killTriggers[i].OnEntry += OnEnterKillTrigger;
			_killTriggers[i].OnExit += OnExitKillTrigger;
		}
	}

	private void Start()
	{
		_owAudioSourceLP.SetLocalVolume(0f);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_interactVolume.OnPressInteract -= OnPressInteract;
		for (int i = 0; i < _killTriggers.Length; i++)
		{
			_killTriggers[i].OnEntry -= OnEnterKillTrigger;
			_killTriggers[i].OnExit -= OnExitKillTrigger;
		}
	}

	public void ChangePromptText(UITextType promptID, bool showKeyCommandIcon)
	{
		_interactVolume.ChangePrompt(promptID);
		_interactVolume.SetKeyCommandVisible(showKeyCommandIcon);
	}

	public void ResetInteractVolume(UITextType promptID, bool showKeyCommandIcon)
	{
		_interactVolume.SetPromptText(promptID);
		_interactVolume.SetKeyCommandVisible(showKeyCommandIcon);
	}

	public void GiveControlToElevator()
	{
		_thisScriptControlsActivation = true;
	}

	public void TakeControlFromElevator()
	{
		_thisScriptControlsActivation = false;
	}

	public float GetTrackPositionFraction()
	{
		return Vector3.Distance(base.transform.localPosition, _startLocalPos) / Vector3.Distance(_startLocalPos, _endLocalPos);
	}

	public void ReturnToStart()
	{
		_targetLocalPos = _startLocalPos;
		_goingToTheEnd = false;
		_interactVolume.transform.Rotate(0f, 180f, 0f);
		StartLift();
	}

	private void OnPressInteract()
	{
		if (this.OnPressInteractEvent != null)
		{
			this.OnPressInteractEvent();
		}
		if (_thisScriptControlsActivation)
		{
			AttachPlayerAndStartLift();
		}
		else
		{
			_interactVolume.ResetInteraction();
		}
	}

	public void AttachPlayerAndStartLift()
	{
		_interactVolume.transform.Rotate(0f, 180f, 0f);
		_attachPoint.AttachPlayer();
		_goingToTheEnd = !_goingToTheEnd;
		if (!_goingToTheEnd)
		{
			_targetLocalPos = _startLocalPos;
		}
		else
		{
			_targetLocalPos = _endLocalPos;
		}
		if (Locator.GetPlayerSuit().IsWearingSuit() && Locator.GetPlayerSuit().IsTrainingSuit())
		{
			Locator.GetPlayerSuit().RemoveSuit();
		}
		StartLift();
	}

	private void StartLift()
	{
		base.enabled = true;
		_initLocalPos = base.transform.localPosition;
		_initLiftTime = Time.time;
		_owAudioSourceOneShot.PlayOneShot(AudioType.TH_LiftActivate);
		_owAudioSourceLP.FadeIn(0.5f);
		_interactVolume.DisableInteraction();
	}

	private void Update()
	{
		float t = (Time.time - _initLiftTime) / _liftDuration;
		t = Mathf.SmoothStep(0f, 1f, t);
		Vector3 position = Vector3.Lerp(base.transform.parent.TransformPoint(_initLocalPos), base.transform.parent.TransformPoint(_targetLocalPos), t);
		if (float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
		{
			Debug.LogWarning("Elevator.Update() in " + base.gameObject.name + " encountered Infinity value", this);
			return;
		}
		if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
		{
			Debug.LogWarning("Elevator.Update() in " + base.gameObject.name + " encountered NaN value", this);
			return;
		}
		base.transform.position = position;
		if (_animator != null && t <= 1f)
		{
			_animator.enabled = true;
			if (!_hasGoneUp)
			{
				_animator.SetBool("GoingUp", value: true);
			}
			else if (_hasGoneUp)
			{
				_animator.SetBool("GoingDown", value: true);
			}
		}
		if (!(t >= 1f))
		{
			return;
		}
		base.enabled = false;
		_attachPoint.DetachPlayer();
		if (_interactVolume != null)
		{
			_interactVolume.ResetInteraction();
			_interactVolume.EnableInteraction();
		}
		if (_animator != null)
		{
			_animator.SetBool("GoingUp", value: false);
			_animator.SetBool("GoingDown", value: false);
			if (!_hasGoneUp)
			{
				_hasGoneUp = true;
			}
			else
			{
				_hasGoneUp = false;
			}
			_animator.enabled = false;
		}
		_owAudioSourceLP.FadeOut(0.5f);
		_owAudioSourceOneShot.PlayOneShot(AudioType.TH_LiftArrives);
	}

	private void OnEnterKillTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerKillTriggerCount++;
			if (_playerKillTriggerCount == _killTriggers.Length)
			{
				Locator.GetDeathManager().KillPlayer(DeathType.CrushedByElevator);
				RumbleManager.PlayerCrushedByElevator();
			}
		}
	}

	private void OnExitKillTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerKillTriggerCount--;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.up * _trackHeight);
		Gizmos.DrawLine(base.transform.position, base.transform.position - base.transform.up * _trackDepth);
	}
}
