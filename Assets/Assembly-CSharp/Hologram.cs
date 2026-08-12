using UnityEngine;

public class Hologram : MonoBehaviour
{
	public delegate void HologramCompleteEvent(Hologram hologram);

	[SerializeField]
	private float _startScale = 0.5f;

	private float _startAnimTime;

	private float _animFraction;

	private float _startAnimFraction;

	private float _targetAnimFraction;

	private bool _animating;

	private bool _isActive;

	private bool _firedCompleteEvent;

	private Animator _animator;

	private Transform _spawnPoint;

	private Transform _displayPoint;

	public event HologramCompleteEvent OnHologramComplete;

	private void Awake()
	{
		_animFraction = 0f;
		base.enabled = false;
		base.gameObject.SetActive(value: false);
		_animator = GetComponent<Animator>();
		if (_animator != null)
		{
			_animator.enabled = false;
		}
		MonoBehaviour.print("hologram awake " + base.gameObject.name);
	}

	public void SetSpawnAndDisplayPoints(Transform spawnPoint, Transform displayPoint)
	{
		_spawnPoint = spawnPoint;
		_displayPoint = displayPoint;
		base.transform.position = _spawnPoint.position;
		base.transform.rotation = _spawnPoint.rotation;
		base.transform.localScale = Vector3.one * _startScale;
	}

	public float GetAnimFraction()
	{
		return _animFraction;
	}

	public bool IsActive()
	{
		return _isActive;
	}

	public bool IsCompleted()
	{
		return _firedCompleteEvent;
	}

	public void Activate()
	{
		AnimateTo(1f);
		_isActive = true;
		base.enabled = true;
		base.gameObject.SetActive(value: true);
		OnActivation();
		MonoBehaviour.print("activate hologram");
	}

	public void Deactivate()
	{
		AnimateTo(0f);
		_isActive = false;
		OnDeactivation();
	}

	private void AnimateTo(float targetFraction)
	{
		_startAnimFraction = _animFraction;
		_startAnimTime = Time.time;
		_targetAnimFraction = targetFraction;
		_animating = true;
	}

	protected virtual void OnActivation()
	{
	}

	protected virtual void OnFinishActivation()
	{
	}

	protected virtual void OnDeactivation()
	{
	}

	protected virtual void UpdateHologram()
	{
	}

	protected void CompleteHologram()
	{
		if (this.OnHologramComplete != null && !_firedCompleteEvent)
		{
			this.OnHologramComplete(this);
		}
		_firedCompleteEvent = true;
	}

	private void FixedUpdate()
	{
		if (_animating)
		{
			float t = Mathf.InverseLerp(_startAnimTime, _startAnimTime + 6f, Time.time);
			t = Mathf.SmoothStep(0f, 1f, t);
			_animFraction = Mathf.Lerp(_startAnimFraction, _targetAnimFraction, t);
			base.transform.position = Vector3.Lerp(_spawnPoint.position, _displayPoint.position, _animFraction);
			base.transform.rotation = Quaternion.Slerp(_spawnPoint.rotation, _displayPoint.rotation, _animFraction);
			base.transform.localScale = Vector3.Lerp(Vector3.one * _startScale, Vector3.one, _animFraction);
			if (_animFraction != _targetAnimFraction)
			{
				return;
			}
			_animating = false;
			if (_targetAnimFraction <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
			else if (_targetAnimFraction >= 1f)
			{
				if (_animator != null)
				{
					_animator.enabled = true;
				}
				OnFinishActivation();
			}
		}
		else if (!IsCompleted())
		{
			UpdateHologram();
		}
	}
}
