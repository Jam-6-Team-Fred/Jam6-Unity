using UnityEngine;

[RequireComponent(typeof(OWRenderer))]
public class SingularityController : MonoBehaviour
{
	public enum State
	{
		Creating = 0,
		Stable = 1,
		Collapsing = 2,
		Collapsed = 3
	}

	public enum TransitionEffectType
	{
		EnterBlackHole = 0,
		ExitWhiteHole = 1,
		PlayerEnterBlackHole = 2,
		PlayerExitWhiteHole = 3
	}

	public delegate void SingularityEffectEvent();

	private OWRenderer _renderer;

	private int _propID_Radius;

	[SerializeField]
	private AnimationCurve _creationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _destructionCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	[SerializeField]
	private bool _startActive;

	[SerializeField]
	private bool _muteSingularityEffectAudio = true;

	[SerializeField]
	private OWAudioSource _owAmbientSource;

	[SerializeField]
	private OWAudioSource _owOneShotSource;

	private State _state;

	private float _targetRadius;

	private float _baseRadius;

	private float _currentRadius;

	private float _timer;

	private bool _hasLifespan;

	private float _lifeTimer;

	private PlayerAudioController _playerAudioController;

	public event SingularityEffectEvent OnCreation;

	public event SingularityEffectEvent OnCollapse;

	public State GetState()
	{
		return _state;
	}

	public float GetCreationLength()
	{
		return _creationCurve.keys[_creationCurve.length - 1].time;
	}

	public float GetCollapseLength()
	{
		return _destructionCurve.keys[_destructionCurve.length - 1].time;
	}

	private void Awake()
	{
		_renderer = GetComponent<OWRenderer>();
		_propID_Radius = Shader.PropertyToID("_Radius");
		_state = (_startActive ? State.Stable : State.Collapsed);
		_targetRadius = _renderer.sharedMaterial.GetFloat("_Radius");
		_baseRadius = (_startActive ? _targetRadius : 0f);
		_currentRadius = _baseRadius;
		_renderer.SetMaterialProperty(_propID_Radius, _currentRadius);
		_renderer.SetActivation(_startActive);
	}

	private void Start()
	{
		if (_owAmbientSource != null)
		{
			_owAmbientSource.SetLocalVolume(0f);
			if (_startActive && !_muteSingularityEffectAudio)
			{
				_owAmbientSource.SetLocalVolume(1f);
				_owAmbientSource.Play();
				_owAmbientSource.RandomizePlayhead();
			}
		}
		base.enabled = _startActive;
	}

	public void Create()
	{
		if (_state == State.Creating || _state == State.Stable)
		{
			return;
		}
		_state = State.Creating;
		_baseRadius = _currentRadius;
		_timer = 0f;
		_renderer.SetActivation(active: true);
		if (!_muteSingularityEffectAudio)
		{
			if (_owAmbientSource != null)
			{
				_owAmbientSource.SetLocalVolume(0f);
				_owAmbientSource.Play();
				_owAmbientSource.RandomizePlayhead();
			}
			if (_owOneShotSource != null)
			{
				_owOneShotSource.PlayOneShot(AudioType.SingularityCreate);
			}
		}
		base.enabled = true;
	}

	public void Collapse()
	{
		if (_state != State.Collapsing && _state != State.Collapsed)
		{
			_state = State.Collapsing;
			_baseRadius = _currentRadius;
			_timer = 0f;
			base.enabled = true;
			if (!_muteSingularityEffectAudio && _owOneShotSource != null)
			{
				_owOneShotSource.PlayOneShot(AudioType.SingularityCollapse);
			}
		}
	}

	public void CreateWithLifetime(float lifetime)
	{
		Create();
		_hasLifespan = true;
		_lifeTimer = lifetime;
	}

	public void CollapseImmediate()
	{
		_state = State.Collapsed;
		_hasLifespan = false;
		_currentRadius = 0f;
		_renderer.SetActivation(active: false);
		_renderer.SetMaterialProperty(_propID_Radius, 0f);
		if (_owAmbientSource != null)
		{
			_owAmbientSource.FadeOut(0.5f);
		}
		base.enabled = false;
	}

	public void PlayEntryAudio(bool isPlayer = false)
	{
		if (!isPlayer)
		{
			_owOneShotSource.PlayOneShot(AudioType.SingularityOnObjectEnter);
		}
	}

	public void PlayExitAudio(bool isPlayer = false)
	{
		if (isPlayer)
		{
			Locator.GetPlayerAudioController().PlayPlayerSingularityTransit();
		}
		else
		{
			_owOneShotSource.PlayOneShot(AudioType.SingularityOnObjectExit);
		}
	}

	private void Update()
	{
		_timer += Time.deltaTime;
		switch (_state)
		{
		case State.Creating:
		{
			float num2 = _creationCurve.Evaluate(_timer);
			_currentRadius = Mathf.LerpUnclamped(_baseRadius, _targetRadius, num2);
			_renderer.SetMaterialProperty(_propID_Radius, _currentRadius);
			if (_owAmbientSource != null)
			{
				_owAmbientSource.SetLocalVolume(num2);
			}
			if (_timer >= GetCreationLength())
			{
				if (_hasLifespan && _lifeTimer <= 0f)
				{
					Collapse();
				}
				else
				{
					_state = State.Stable;
					base.enabled = _hasLifespan;
				}
				if (this.OnCreation != null)
				{
					this.OnCreation();
				}
			}
			break;
		}
		case State.Collapsing:
		{
			float num = _destructionCurve.Evaluate(_timer);
			_currentRadius = Mathf.LerpUnclamped(0f, _baseRadius, num);
			_renderer.SetMaterialProperty(_propID_Radius, _currentRadius);
			if (_owAmbientSource != null)
			{
				_owAmbientSource.SetLocalVolume(num);
			}
			if (_timer >= GetCollapseLength())
			{
				_state = State.Collapsed;
				_renderer.SetActivation(active: false);
				if (_owAmbientSource != null)
				{
					_owAmbientSource.Stop();
				}
				base.enabled = false;
				if (this.OnCollapse != null)
				{
					this.OnCollapse();
				}
			}
			break;
		}
		case State.Stable:
			if (_hasLifespan)
			{
				_lifeTimer -= Time.deltaTime;
				if (_lifeTimer <= 0f)
				{
					Collapse();
				}
			}
			break;
		default:
			base.enabled = false;
			break;
		}
	}
}
