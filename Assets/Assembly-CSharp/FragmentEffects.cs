using UnityEngine;

public class FragmentEffects : MonoBehaviour
{
	private FragmentIntegrity _fragmentIntegrity;

	private DetachableFragment _detachableFragment;

	[SerializeField]
	private ParticleSystem[] _impactParticles;

	[SerializeField]
	private ParticleSystem[] _detachmentParticles;

	[SerializeField]
	private ParticleSystem[] _destructionParticles;

	[SerializeField]
	private OWRenderer[] _destructionRenderers;

	[SerializeField]
	private float _destructionDissolveLength = 2f;

	private PolyLineEmitter[] _impactParticleEmitters;

	private PolyLineEmitter[] _detachmentParticleEmitters;

	private PolyLineEmitter[] _destructionParticleEmitters;

	private OWCollider[] _destructionColliders;

	private int _propID_Dissolve;

	private bool _dissoveStarted;

	private bool _collidersActive = true;

	private float _dissolveStartTime;

	private void Awake()
	{
		_fragmentIntegrity = GetComponentInParent<FragmentIntegrity>();
		_detachableFragment = _fragmentIntegrity.GetComponent<DetachableFragment>();
		if (_impactParticles != null)
		{
			_impactParticleEmitters = new PolyLineEmitter[_impactParticles.Length];
			for (int i = 0; i < _impactParticles.Length; i++)
			{
				if (_impactParticles[i] != null)
				{
					_impactParticleEmitters[i] = _impactParticles[i].GetComponent<PolyLineEmitter>();
				}
			}
		}
		if (_detachmentParticles != null)
		{
			_detachmentParticleEmitters = new PolyLineEmitter[_detachmentParticles.Length];
			for (int j = 0; j < _detachmentParticles.Length; j++)
			{
				if (_detachmentParticles[j] != null)
				{
					_detachmentParticleEmitters[j] = _detachmentParticles[j].GetComponent<PolyLineEmitter>();
				}
			}
		}
		if (_destructionParticles != null)
		{
			_destructionParticleEmitters = new PolyLineEmitter[_destructionParticles.Length];
			for (int k = 0; k < _destructionParticles.Length; k++)
			{
				if (_destructionParticles[k] != null)
				{
					_destructionParticleEmitters[k] = _destructionParticles[k].GetComponent<PolyLineEmitter>();
				}
			}
		}
		_fragmentIntegrity.OnTakeDamage += OnFragmentDamage;
		if (_destructionRenderers != null && _destructionRenderers.Length != 0)
		{
			_propID_Dissolve = Shader.PropertyToID("_Dissolve");
			_destructionColliders = new OWCollider[_destructionRenderers.Length];
			for (int l = 0; l < _destructionRenderers.Length; l++)
			{
				_destructionColliders[l] = _destructionRenderers[l].GetComponent<OWCollider>();
			}
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_fragmentIntegrity != null)
		{
			_fragmentIntegrity.OnTakeDamage -= OnFragmentDamage;
		}
	}

	private void Update()
	{
		if (_destructionRenderers == null || _destructionRenderers.Length == 0 || !_dissoveStarted)
		{
			return;
		}
		float num = (Time.time - _dissolveStartTime) / _destructionDissolveLength;
		for (int i = 0; i < _destructionRenderers.Length; i++)
		{
			_destructionRenderers[i].SetMaterialProperty(_propID_Dissolve, num);
		}
		if (_collidersActive && num >= 0.5f)
		{
			_collidersActive = false;
			for (int j = 0; j < _destructionColliders.Length; j++)
			{
				if (_destructionColliders[j] != null)
				{
					_destructionColliders[j].SetActivation(active: false);
				}
			}
		}
		if (num >= 1f)
		{
			for (int k = 0; k < _destructionRenderers.Length; k++)
			{
				_destructionRenderers[k].SetActivation(active: false);
			}
			base.enabled = false;
		}
	}

	private void OnFragmentDamage(float integrity)
	{
		if (integrity > 0f)
		{
			PlayImpactEffects();
		}
		else if (_detachableFragment != null && _detachableFragment.HasDestructibleRoot())
		{
			PlayDestructionEffects();
		}
		else if (_detachableFragment != null)
		{
			PlayDetachmentEffects();
		}
	}

	public void PlayImpactEffects()
	{
		if (_impactParticles == null)
		{
			return;
		}
		for (int i = 0; i < _impactParticles.Length; i++)
		{
			if (!(_impactParticles[i] == null))
			{
				if (_impactParticleEmitters[i] != null)
				{
					_impactParticleEmitters[i].Play();
				}
				else
				{
					_impactParticles[i].Play();
				}
			}
		}
	}

	public void PlayDetachmentEffects()
	{
		if (_detachmentParticles == null)
		{
			return;
		}
		for (int i = 0; i < _detachmentParticles.Length; i++)
		{
			if (!(_detachmentParticles[i] == null))
			{
				if (_detachmentParticleEmitters[i] != null)
				{
					_detachmentParticleEmitters[i].Play();
				}
				else
				{
					_detachmentParticles[i].Play();
				}
			}
		}
	}

	public void PlayDestructionEffects()
	{
		if (_destructionParticles != null)
		{
			for (int i = 0; i < _destructionParticles.Length; i++)
			{
				if (!(_destructionParticles[i] == null))
				{
					if (_destructionParticleEmitters[i] != null)
					{
						_destructionParticleEmitters[i].Play();
					}
					else
					{
						_destructionParticles[i].Play();
					}
				}
			}
		}
		if (_destructionRenderers != null && _destructionRenderers.Length != 0)
		{
			_dissoveStarted = true;
			_dissolveStartTime = Time.time;
			base.enabled = true;
		}
	}
}
