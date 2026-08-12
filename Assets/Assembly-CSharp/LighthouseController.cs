using UnityEngine;

public class LighthouseController : MonoBehaviour
{
	[SerializeField]
	private BuildingRailController _stageOneController;

	[SerializeField]
	private BuildingRailController _stageTwoController;

	[SerializeField]
	private BuildingRailController _mapHouseStageTwoController;

	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[Header("Floor Damage")]
	[SerializeField]
	private CustomCollisionChecker _floorCollisionChecker;

	[SerializeField]
	private GameObject _floorIntact;

	[SerializeField]
	private GameObject _floorDamaged;

	[Header("Audio")]
	[SerializeField]
	private OWAudioSource _collapseAudio;

	[SerializeField]
	private OWAudioSource _splashAudio;

	[Header("Secret Passage")]
	[SerializeField]
	private OWLightController[] _muralRoomLights;

	[SerializeField]
	private int[] _doorLightIndices;

	[SerializeField]
	private AbstractDoor _secretDoor;

	[SerializeField]
	private GameObject _doorLightShaft;

	private bool _collapseStarted;

	private bool _hasFallAudioPartTwoPlayed;

	private bool _hasMapHouseStageTwoPlayed;

	private bool _hasSplashAudioPlayed;

	private float _fallAudioPartTwoTime;

	private float _mapHouseStageTwoTime;

	private float _splashAudioTime;

	private void Awake()
	{
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
		}
		if (_floorCollisionChecker != null)
		{
			_floorCollisionChecker.OnEnterCustomCollider += new OWEvent.OWCallback(OnEnterCustomCollider);
		}
		if (_floorDamaged != null)
		{
			_floorDamaged.SetActive(value: false);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
		}
		if (_floorCollisionChecker != null)
		{
			_floorCollisionChecker.OnEnterCustomCollider -= new OWEvent.OWCallback(OnEnterCustomCollider);
		}
	}

	public bool HasCollapseStarted()
	{
		return _collapseStarted;
	}

	public void StartCollapse()
	{
		if (!_collapseStarted)
		{
			base.enabled = true;
			_collapseStarted = true;
			_fallAudioPartTwoTime = Time.time + 3.5f;
			_splashAudioTime = Time.time + 6f;
			_mapHouseStageTwoTime = Time.time + 6f;
			_collapseAudio.PlayOneShot(AudioType.Tower_RW_Fall_1);
			if (Locator.GetDreamWorldAudioController() != null)
			{
				Locator.GetDreamWorldAudioController().PlayTowerOneShot(AudioType.Tower_DW_Fall_1);
			}
			_stageTwoController.StartMoveAlongRail();
			GlobalMessenger.FireEvent("LighthouseCollapseStageTwo");
		}
	}

	public void SetMuralRoomLightState(int index, bool lit)
	{
		if (index < 0 || index >= _muralRoomLights.Length)
		{
			Debug.LogError("Light index out of range!!!");
			Debug.Break();
		}
		else
		{
			_muralRoomLights[index].SetIntensity(lit ? 1f : 0f);
		}
	}

	public void SetDoorLightShaftActive(bool active)
	{
		_doorLightShaft.SetActive(active);
	}

	public void CheckMuralRoomSecretDoor()
	{
		bool flag = true;
		for (int i = 0; i < _doorLightIndices.Length; i++)
		{
			if (_muralRoomLights[_doorLightIndices[i]].GetIntensity() > 0f)
			{
				flag = false;
				break;
			}
		}
		if (flag != _secretDoor.IsOpen())
		{
			_secretDoor.SetOpenImmediate(flag);
		}
	}

	private void Update()
	{
		if (!_hasFallAudioPartTwoPlayed && Time.time > _fallAudioPartTwoTime)
		{
			_hasFallAudioPartTwoPlayed = true;
			_collapseAudio.PlayOneShot(AudioType.Tower_RW_Fall_2);
			if (Locator.GetDreamWorldAudioController() != null)
			{
				Locator.GetDreamWorldAudioController().PlayTowerOneShot(AudioType.Tower_DW_Fall_2);
			}
		}
		if (!_hasMapHouseStageTwoPlayed && Time.time > _mapHouseStageTwoTime)
		{
			_hasMapHouseStageTwoPlayed = true;
			_mapHouseStageTwoController.StartMoveAlongRail();
		}
		if (!_hasSplashAudioPlayed && Time.time > _splashAudioTime)
		{
			_hasSplashAudioPlayed = true;
			_splashAudio.PlayOneShot(AudioType.Tower_RW_Splash);
		}
		if (_hasFallAudioPartTwoPlayed && _hasSplashAudioPlayed)
		{
			base.enabled = false;
		}
	}

	private void OnFloodImpact()
	{
		_collapseAudio.PlayOneShot(AudioType.Tower_RW_Tilt);
		if (Locator.GetDreamWorldAudioController() != null)
		{
			Locator.GetDreamWorldAudioController().PlayTowerOneShot(AudioType.Tower_DW_Tilt);
		}
		_stageOneController.StartMoveAlongRail();
		GlobalMessenger.FireEvent("LighthouseCollapseStageOne");
	}

	private void OnEnterCustomCollider()
	{
		if (_floorIntact != null && _floorDamaged != null)
		{
			_floorIntact.SetActive(value: false);
			_floorDamaged.SetActive(value: true);
		}
	}
}
