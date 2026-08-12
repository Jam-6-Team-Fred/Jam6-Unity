using GhostEnums;

public class GrabAction : GhostAction
{
	private bool _playerIsGrabbed;

	private bool _grabAnimComplete;

	public override Name GetName()
	{
		return Name.Grab;
	}

	public override float CalculateUtility()
	{
		if (PlayerState.IsAttached() || !_data.sensor.inContactWithPlayer)
		{
			return -100f;
		}
		return 100f;
	}

	public override bool IsInterruptible()
	{
		return false;
	}

	protected override void OnEnterAction()
	{
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Chase);
		_effects.PlayGrabAnimation();
		_effects.OnGrabComplete += new OWEvent.OWCallback(OnGrabComplete);
		_controller.SetLanternConcealed(concealed: false);
		_controller.ChangeLanternFocus(0f);
		if (_data.previousAction != Name.Chase)
		{
			_effects.PlayVoiceAudioNear((_data.sensor.isPlayerVisible || PlayerData.GetReducedFrights()) ? AudioType.Ghost_Grab_Shout : AudioType.Ghost_Grab_Scream);
		}
	}

	protected override void OnExitAction()
	{
		_effects.PlayDefaultAnimation();
		_playerIsGrabbed = false;
		_grabAnimComplete = false;
		_effects.OnGrabComplete -= new OWEvent.OWCallback(OnGrabComplete);
	}

	public override bool Update_Action()
	{
		if (_playerIsGrabbed)
		{
			return true;
		}
		if (_data.playerLocation.distanceXZ > 1.7f)
		{
			_controller.MoveToLocalPosition(_data.playerLocation.localPosition, MoveType.GRAB);
		}
		_controller.FaceLocalPosition(_data.playerLocation.localPosition, TurnSpeed.FASTEST);
		if (_sensors.CanGrabPlayer())
		{
			GrabPlayer();
		}
		return !_grabAnimComplete;
	}

	private void GrabPlayer()
	{
		_playerIsGrabbed = true;
		_controller.StopMovingInstantly();
		_controller.StopFacing();
		_controller.SetLanternConcealed(concealed: true, playAudio: false);
		_controller.GetGrabController().GrabPlayer(1f);
	}

	private void OnGrabComplete()
	{
		_grabAnimComplete = true;
	}

	public bool isPlayerGrabbed()
	{
		return _playerIsGrabbed;
	}
}
