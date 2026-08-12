using UnityEngine;

public class PlayerMovementAudio : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _footstepAudio;

	[SerializeField]
	private OWAudioSource _jumpAudio;

	[SerializeField]
	private OWAudioSource _slidingAudio;

	private PlayerCharacterController _playerController;

	private FluidDetector _fluidDetector;

	private PlayerAnimController _animationController;

	private bool _leftFootEverGrounded;

	private bool _rightFootEverGrounded;

	private void Start()
	{
		_playerController = Locator.GetPlayerController();
		_playerController.OnJump += OnJump;
		_fluidDetector = Locator.GetPlayerDetector().GetComponent<FluidDetector>();
		_animationController = Locator.GetPlayerTransform().GetComponentInChildren<PlayerAnimController>();
		_animationController.OnLeftFootGrounded += OnLeftFootGrounded;
		_animationController.OnRightFootGrounded += OnRightFootGrounded;
		_slidingAudio.AssignAudioLibraryClip(AudioType.MovementIceLSiding);
		_slidingAudio.SetLocalVolume(0f);
	}

	private void OnDestroy()
	{
		_playerController.OnJump -= OnJump;
		_animationController.OnLeftFootGrounded -= PlayFootstep;
		_animationController.OnRightFootGrounded -= PlayFootstep;
	}

	private void FixedUpdate()
	{
		bool flag = _playerController.IsSlidingOnIce();
		if (flag)
		{
			if (!_slidingAudio.isPlaying)
			{
				_slidingAudio.SetLocalVolume(0f);
				_slidingAudio.Play();
			}
			float magnitude = (_playerController.GetGroundBody().GetPointVelocity(_playerController.GetBody().GetPosition()) - _playerController.GetBody().GetVelocity()).magnitude;
			float localVolume = _slidingAudio.GetLocalVolume();
			float target = Mathf.InverseLerp(0f, 8f, magnitude);
			localVolume = Mathf.MoveTowards(localVolume, target, Time.deltaTime * 20f);
			_slidingAudio.SetLocalVolume(localVolume);
		}
		else if (!flag && _slidingAudio.isPlaying)
		{
			_slidingAudio.FadeOut(0.05f);
		}
	}

	private void OnLeftFootGrounded()
	{
		if (_leftFootEverGrounded && _rightFootEverGrounded && !_playerController.IsSlidingOnIce())
		{
			PlayFootstep();
		}
		_leftFootEverGrounded = true;
	}

	private void OnRightFootGrounded()
	{
		if (_leftFootEverGrounded && _rightFootEverGrounded && !_playerController.IsSlidingOnIce())
		{
			PlayFootstep();
		}
		_rightFootEverGrounded = true;
	}

	private void PlayFootstep()
	{
		AudioType audioType = ((!PlayerState.IsCameraUnderwater() && _fluidDetector.InFluidType(FluidVolume.Type.WATER)) ? AudioType.MovementShallowWaterFootstep : GetFootstepAudioType(_playerController.GetGroundSurface()));
		if (audioType != 0)
		{
			_footstepAudio.pitch = Random.Range(0.9f, 1.1f);
			_footstepAudio.PlayOneShot(audioType, 0.7f);
		}
	}

	private void OnJump()
	{
		_jumpAudio.pitch = Random.Range(0.9f, 1.1f);
		_jumpAudio.PlayOneShot(AudioType.MovementJump);
	}

	public static AudioType GetFootstepAudioType(SurfaceType surfaceType)
	{
		switch (surfaceType)
		{
		case SurfaceType.Gravel:
			return AudioType.MovementGravelFootsteps;
		case SurfaceType.Grass:
		case SurfaceType.Fabric:
			return AudioType.MovementGrassFootstep;
		case SurfaceType.Foliage:
			return AudioType.MovementLeavesFootsteps;
		case SurfaceType.Stone:
		case SurfaceType.Bone:
		case SurfaceType.Vine:
		case SurfaceType.QuantumRock:
			return AudioType.MovementStoneFootstep;
		case SurfaceType.Sand:
			return AudioType.MovementSandFootstep;
		case SurfaceType.Snow:
			return AudioType.MovementSnowFootstep;
		case SurfaceType.Ice:
			return AudioType.MovementIceFootstep;
		case SurfaceType.Obsidian:
		case SurfaceType.Crystal:
		case SurfaceType.Glass:
		case SurfaceType.Ceramic:
			return AudioType.MovementGlassFootsteps;
		case SurfaceType.Water:
			return AudioType.MovementShallowWaterFootstep;
		case SurfaceType.Wood:
			return AudioType.MovementWoodFootstep;
		case SurfaceType.Planks:
			return AudioType.MovementWoodCreakFootstep;
		case SurfaceType.Metal:
			return AudioType.MovementMetalFootstep;
		case SurfaceType.MetalNomai:
			return AudioType.MovementNomaiMetalFootstep;
		default:
			return AudioType.MovementDirtFootstep;
		}
	}

	public static AudioType GetLandingAudioType(SurfaceType surfaceType)
	{
		switch (surfaceType)
		{
		case SurfaceType.Gravel:
			return AudioType.MovementGravelLanding;
		case SurfaceType.Grass:
		case SurfaceType.Fabric:
			return AudioType.LandingGrass;
		case SurfaceType.Foliage:
			return AudioType.MovementLeavesLanding;
		case SurfaceType.Stone:
		case SurfaceType.Bone:
		case SurfaceType.Vine:
		case SurfaceType.QuantumRock:
			return AudioType.LandingStone;
		case SurfaceType.Sand:
			return AudioType.LandingSand;
		case SurfaceType.Snow:
			return AudioType.MovementSnowLanding;
		case SurfaceType.Ice:
			return AudioType.LandingIce;
		case SurfaceType.Obsidian:
		case SurfaceType.Crystal:
		case SurfaceType.Glass:
		case SurfaceType.Ceramic:
			return AudioType.MovementGlassLanding;
		case SurfaceType.Water:
			return AudioType.Splash_Water_Probe;
		case SurfaceType.Wood:
			return AudioType.MovementWoodLanding;
		case SurfaceType.Planks:
			return AudioType.MovementWoodCreakLanding;
		case SurfaceType.Metal:
			return AudioType.LandingMetal;
		case SurfaceType.MetalNomai:
			return AudioType.LandingNomaiMetal;
		default:
			return AudioType.LandingDirt;
		}
	}
}
