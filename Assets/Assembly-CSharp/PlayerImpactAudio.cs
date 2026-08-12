using UnityEngine;

[RequireComponent(typeof(OWAudioSource))]
public class PlayerImpactAudio : MonoBehaviour
{
	private ImpactSensor _impactSensor;

	private PlayerResources _playerResources;

	private OWRigidbody _playerBody;

	private OWAudioSource _impactAudioSrc;

	private float _enterShipTime;

	private float _lastImpactAudioTime;

	private float _lastImpactAudioSpeed;

	private FluidDetector _playerFluidDetector;

	private FluidDetector _cameraFluidDetector;

	private void Awake()
	{
		_playerBody = base.gameObject.GetTaggedComponent<OWRigidbody>("Player");
		_impactSensor = base.gameObject.GetTaggedComponent<ImpactSensor>("Player");
		_playerResources = base.gameObject.GetTaggedComponent<PlayerResources>("Player");
		_impactSensor.OnImpact += OnImpact;
		GlobalMessenger.AddListener("EnterShip", OnEnterShip);
	}

	private void Start()
	{
		_impactAudioSrc = this.GetRequiredComponent<OWAudioSource>();
		_impactAudioSrc.playOnAwake = false;
		_playerFluidDetector = Locator.GetPlayerDetector().GetComponent<FluidDetector>();
		_cameraFluidDetector = Locator.GetPlayerCamera().GetComponentInChildren<PlayerCameraFluidDetector>();
	}

	private void OnDestroy()
	{
		_impactSensor.OnImpact -= OnImpact;
		GlobalMessenger.RemoveListener("EnterShip", OnEnterShip);
	}

	private void OnEnterShip()
	{
		_enterShipTime = Time.time;
	}

	private void OnImpact(ImpactData impact)
	{
		if (Time.time < _enterShipTime + 1f || (PlayerState.InDreamWorld() && PlayerState.IsCameraUnderwater()))
		{
			return;
		}
		float num = 3f;
		if (!(impact.speed > num))
		{
			return;
		}
		float num2 = 1f;
		float minImpactSpeed = _playerResources.GetMinImpactSpeed();
		float maxImpactSpeed = _playerResources.GetMaxImpactSpeed();
		if ((!(_lastImpactAudioSpeed <= minImpactSpeed) || !(impact.speed > minImpactSpeed) || PlayerState.IsInsideTheEye()) && Time.time < _lastImpactAudioTime + 0.2f)
		{
			return;
		}
		AudioType audioType = AudioType.None;
		if (impact.speed <= minImpactSpeed || PlayerState.IsInsideTheEye())
		{
			if (_playerFluidDetector.InFluidType(FluidVolume.Type.TRACTOR_BEAM))
			{
				return;
			}
			float b = Mathf.Lerp(num, minImpactSpeed, 0.5f);
			num2 = Mathf.InverseLerp(num, b, impact.speed);
			if (num2 > 0.5f)
			{
				RumbleManager.PulseLightImpact();
			}
			float num3 = Vector3.Angle(impact.velocity, _playerBody.transform.up);
			Vector3 vector = _playerBody.transform.InverseTransformPoint(impact.point);
			if (num3 < 60f && vector.y < -0.25f)
			{
				SurfaceType surfaceType = SurfaceType.None;
				if (_playerFluidDetector.InFluidType(FluidVolume.Type.WATER) && !_cameraFluidDetector.InFluidType(FluidVolume.Type.WATER))
				{
					surfaceType = SurfaceType.Water;
				}
				else
				{
					if (impact.otherBody.CompareTag("Raft"))
					{
						_impactAudioSrc.pitch = Random.Range(0.9f, 1f);
						_impactAudioSrc.PlayOneShot(AudioType.Raft_Impact_Player, num2);
						return;
					}
					for (int i = 0; i < impact.contactSurfaceTypes.Length; i++)
					{
						if (impact.contactSurfaceTypes[i] != 0)
						{
							surfaceType = impact.contactSurfaceTypes[i];
							break;
						}
					}
				}
				audioType = PlayerMovementAudio.GetLandingAudioType(surfaceType);
			}
			else
			{
				audioType = AudioType.ImpactLowSpeed;
			}
		}
		else if (impact.speed <= Mathf.Lerp(minImpactSpeed, maxImpactSpeed, 0.5f))
		{
			audioType = AudioType.ImpactMediumSpeed;
			RumbleManager.PulseMediumImpact();
		}
		else
		{
			audioType = AudioType.ImpactHighSpeed;
			RumbleManager.PulseHeavyImpact();
		}
		if (audioType != 0)
		{
			_impactAudioSrc.pitch = Random.Range(0.9f, 1.1f);
			_impactAudioSrc.PlayOneShot(audioType, num2);
			if (num2 > 0.1f)
			{
				_lastImpactAudioTime = Time.time;
				_lastImpactAudioSpeed = impact.speed;
			}
		}
	}
}
