using System.Collections.Generic;
using UnityEngine;

public abstract class NoiseMaker : MonoBehaviour
{
	public const float FOG_WARP_MUTE_SECONDS = 0.5f;

	protected static List<NoiseMaker> _activeNoiseMakers;

	protected float _noiseRadius;

	protected OWRigidbody _attachedBody;

	private float _lastFogWarpTime;

	public static List<NoiseMaker> GetActiveNoiseMakers()
	{
		return _activeNoiseMakers;
	}

	public float GetNoiseRadius()
	{
		if (!(Time.time - _lastFogWarpTime < 0.5f))
		{
			return _noiseRadius;
		}
		return 0f;
	}

	public OWRigidbody GetAttachedBody()
	{
		return _attachedBody;
	}

	public Vector3 GetNoiseOrigin()
	{
		return _attachedBody.GetCenterOfMass();
	}

	public virtual void OnFogWarp()
	{
		_lastFogWarpTime = Time.time;
	}

	protected virtual void Awake()
	{
		_attachedBody = this.GetAttachedOWRigidbody();
	}

	protected virtual void OnEnable()
	{
		if (_activeNoiseMakers == null)
		{
			_activeNoiseMakers = new List<NoiseMaker>(8);
		}
		_activeNoiseMakers.Add(this);
	}

	protected virtual void OnDisable()
	{
		if (_activeNoiseMakers == null)
		{
			_activeNoiseMakers = new List<NoiseMaker>(8);
		}
		_activeNoiseMakers.Remove(this);
	}

	private void OnDrawGizmosSelected()
	{
		if (_attachedBody != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(_attachedBody.GetPosition(), GetNoiseRadius());
		}
	}
}
