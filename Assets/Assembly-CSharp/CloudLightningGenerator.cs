using System.Collections.Generic;
using UnityEngine;

public class CloudLightningGenerator : MonoBehaviour
{
	private Queue<CloudLightning> _lightningPool;

	private Queue<OWAudioSource> _audioSourcePool;

	[SerializeField]
	protected float _altitude = 900f;

	[Space]
	[SerializeField]
	private Gradient _lightColor = new Gradient();

	[SerializeField]
	private Range _lightIntensity = new Range(3f, 5f);

	[SerializeField]
	private Range _lightDuration = new Range(0.5f, 2f);

	[SerializeField]
	private Range _lightRadius = new Range(300f, 800f);

	[SerializeField]
	private CloudLightning.AnimSettings[] _lightRandomAnimSettings = new CloudLightning.AnimSettings[0];

	[SerializeField]
	private LightRenderMode _lightRenderMode = LightRenderMode.ForceVertex;

	[SerializeField]
	private Range _delay = new Range(0f, 10f);

	[SerializeField]
	private Range _branches = new Range(0f, 5f);

	[SerializeField]
	protected Range _branchDistance = new Range(10f, 50f);

	[SerializeField]
	private Range _branchDelay = new Range(0.1f, 0.5f);

	[Space]
	[SerializeField]
	private GameObject _audioPrefab;

	[SerializeField]
	private Sector _audioSector;

	private float _delayTimer;

	private bool _isBranching;

	private int _numBranches;

	private float _branchTimer;

	protected Vector3 _lastLightningPosition;

	protected virtual void Awake()
	{
		int num = Mathf.RoundToInt(_branches.max) * 2;
		int num2 = Mathf.RoundToInt(_branches.max);
		_lightningPool = new Queue<CloudLightning>(num);
		for (int i = 0; i < num; i++)
		{
			GameObject obj = new GameObject(base.name + "_CloudLightningInstance");
			obj.transform.SetParent(base.transform);
			obj.transform.localPosition = Vector3.zero;
			obj.SetActive(value: false);
			obj.AddComponent<Light>().renderMode = _lightRenderMode;
			CloudLightning cloudLightning = obj.AddComponent<CloudLightning>();
			cloudLightning.OnComplete += ReturnCloudLightning;
			_lightningPool.Enqueue(cloudLightning);
		}
		if (_audioPrefab != null)
		{
			_audioSourcePool = new Queue<OWAudioSource>(num2);
			for (int j = 0; j < num2; j++)
			{
				GameObject obj2 = Object.Instantiate(_audioPrefab, base.transform);
				obj2.SetActive(value: true);
				OWAudioSource requiredComponent = obj2.GetRequiredComponent<OWAudioSource>();
				_audioSourcePool.Enqueue(requiredComponent);
			}
		}
		_delayTimer = _delay.random;
		_isBranching = false;
	}

	protected virtual void Update()
	{
		_delayTimer -= Time.deltaTime;
		if (_delayTimer <= 0f && !_isBranching)
		{
			SpawnLightning(GetLightningStartPosition());
			_isBranching = true;
			_numBranches = Mathf.RoundToInt(_branches.random);
			_branchTimer = _branchDelay.random;
		}
		if (!_isBranching)
		{
			return;
		}
		_branchTimer -= Time.deltaTime;
		if (_branchTimer <= 0f)
		{
			if (_numBranches <= 0)
			{
				_isBranching = false;
				_delayTimer = _delay.random;
			}
			else
			{
				SpawnLightning(GetLightningBranchPosition());
				_branchTimer = _branchDelay.random;
				_numBranches--;
			}
		}
	}

	private void SpawnLightning(Vector3 localPosition)
	{
		CloudLightning cloudLightning = GetCloudLightning();
		if (cloudLightning == null)
		{
			return;
		}
		cloudLightning.transform.localPosition = localPosition;
		RandomizeCloudLightning(cloudLightning);
		cloudLightning.ResetLightning();
		_lastLightningPosition = localPosition;
		if (!(_audioPrefab == null))
		{
			OWAudioSource oWAudioSource = _audioSourcePool.Peek();
			if (_audioSector != null && _audioSector.ContainsOccupant(DynamicOccupant.Player) && oWAudioSource != null && !oWAudioSource.isPlaying && !PlayerState.IsCameraUnderwater() && (!PlayerState.IsInsideShip() || !(Locator.GetShipDetector() != null) || !Locator.GetShipDetector().GetComponent<FluidDetector>().InFluidType(FluidVolume.Type.WATER)))
			{
				OWAudioSource oWAudioSource2 = _audioSourcePool.Dequeue();
				oWAudioSource2.transform.localPosition = localPosition;
				oWAudioSource2.Stop();
				oWAudioSource2.Play();
				_audioSourcePool.Enqueue(oWAudioSource2);
			}
		}
	}

	private CloudLightning GetCloudLightning()
	{
		if (_lightningPool.Count == 0)
		{
			return null;
		}
		CloudLightning cloudLightning = _lightningPool.Dequeue();
		cloudLightning.gameObject.SetActive(value: true);
		return cloudLightning;
	}

	private void ReturnCloudLightning(CloudLightning lightning)
	{
		lightning.gameObject.SetActive(value: false);
		_lightningPool.Enqueue(lightning);
	}

	protected virtual Vector3 GetLightningStartPosition()
	{
		return Random.onUnitSphere * _altitude;
	}

	protected virtual Vector3 GetLightningBranchPosition()
	{
		Vector3 vector = Vector3.ProjectOnPlane(Random.onUnitSphere, _lastLightningPosition).normalized * _branchDistance.random;
		return (_lastLightningPosition + vector).normalized * _altitude;
	}

	protected virtual void RandomizeCloudLightning(CloudLightning lightning)
	{
		lightning.lightColor = _lightColor.Evaluate(Random.value);
		lightning.lightIntensity = _lightIntensity.random;
		lightning.lightLength = _lightDuration.random;
		lightning.lightRadius = _lightRadius.random;
		lightning.lightAnimSettings = _lightRandomAnimSettings[Random.Range(0, _lightRandomAnimSettings.Length)];
	}
}
