using System;
using UnityEngine;

public class DistantStarController : MonoBehaviour
{
	[Serializable]
	private class ExplosionCurve
	{
		public AnimationCurve explosionsByTime;

		private ExplosionCurve()
		{
			explosionsByTime = new AnimationCurve();
		}

		public float Evaluate(float val)
		{
			return explosionsByTime.Evaluate(val);
		}

		public int AddKey(float time, float val)
		{
			return explosionsByTime.AddKey(time, val);
		}
	}

	[SerializeField]
	private float _starsUpdateIntervalInSeconds;

	private ParticleSystem _starField;

	private ParticleSystem.Particle[] _stars;

	private int _starsLength;

	[SerializeField]
	private GameObject _starExplosionObject;

	private float _timeStep;

	private int _explodeToThisIndex;

	private int _lastParticleListIndex;

	private float _starsIntervalUpdate;

	[SerializeField]
	private ExplosionCurve StarFieldExplosionCurve;

	private void Awake()
	{
		_starField = this.GetRequiredComponent<ParticleSystem>();
		_timeStep = 0f;
		_starsIntervalUpdate = _starsUpdateIntervalInSeconds;
		_lastParticleListIndex = 0;
		_starField.Play();
	}

	private void Update()
	{
		_timeStep = TimeLoop.GetFractionElapsed();
		_starsIntervalUpdate -= Time.deltaTime;
		if (_starsIntervalUpdate < 0f && _explodeToThisIndex < _starsLength)
		{
			_explodeToThisIndex = (int)(StarFieldExplosionCurve.Evaluate(_timeStep) * (float)_starsLength);
			for (int i = _lastParticleListIndex; i < _explodeToThisIndex; i++)
			{
				UnityEngine.Object.Instantiate(_starExplosionObject, _stars[i].position, Quaternion.identity, base.transform);
				Color color = new Color(0f, 0f, 0f, 0f);
				_stars[i].startColor = color;
			}
			_starsIntervalUpdate = _starsUpdateIntervalInSeconds;
			_lastParticleListIndex = _explodeToThisIndex;
		}
	}

	private void LateUpdate()
	{
		if (!_starField.isPaused)
		{
			_starField.Pause();
			_stars = new ParticleSystem.Particle[_starField.particleCount];
			_starsLength = _starField.GetParticles(_stars);
		}
	}
}
