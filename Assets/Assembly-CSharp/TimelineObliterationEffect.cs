using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TimelineObliterationEffect : MonoBehaviour
{
	public delegate void TimelineCrackEffectEvent();

	private Renderer _renderer;

	[SerializeField]
	private AnimationCurve _lengthProgressionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private float _randomDelay = 1f;

	private float _timeToComplete = 5f;

	private int _propID_Progression;

	private float _timer;

	private float _delay;

	private bool _isPlaying;

	private bool _isComplete;

	private Vector2 _evaluationState;

	public bool isPlaying => _isPlaying;

	public bool isComplete => _isComplete;

	public float effectTime => _timer;

	public float effectTotalTime => _timeToComplete;

	public event TimelineCrackEffectEvent OnCrackEffectComplete;

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
		_propID_Progression = Shader.PropertyToID("_Progression");
		_timer = 0f;
		_delay = Random.Range(0f, _randomDelay);
		_renderer.enabled = false;
		if (_lengthProgressionCurve.length > 1 && _lengthProgressionCurve.keys[_lengthProgressionCurve.length - 2].value == _lengthProgressionCurve.keys[_lengthProgressionCurve.length - 1].value)
		{
			_timeToComplete = _lengthProgressionCurve.keys[_lengthProgressionCurve.length - 2].time;
		}
		else
		{
			_timeToComplete = _lengthProgressionCurve.keys[_lengthProgressionCurve.length - 1].time;
		}
		_timeToComplete += _delay;
		base.enabled = false;
	}

	private void Update()
	{
		_timer += Time.deltaTime;
		float time = _timer - _delay;
		_renderer.material.SetFloat(_propID_Progression, _lengthProgressionCurve.Evaluate(time));
		if (_timer >= _timeToComplete && !_isComplete)
		{
			_isComplete = true;
			if (this.OnCrackEffectComplete != null)
			{
				this.OnCrackEffectComplete();
			}
			base.enabled = false;
		}
	}

	public void PlayEffect()
	{
		_isPlaying = true;
		_renderer.enabled = true;
		_renderer.material.SetFloat(_propID_Progression, _lengthProgressionCurve.Evaluate(0f));
		base.enabled = true;
	}
}
