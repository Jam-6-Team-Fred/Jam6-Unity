using GhostEnums;
using UnityEngine;

public class ChaseAction : GhostAction
{
	private float _lastScreamTime;

	public override Name GetName()
	{
		return Name.Chase;
	}

	public override float CalculateUtility()
	{
		if (_data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed || (PlayerData.GetReducedFrights() && !_data.reducedFrights_allowChase))
		{
			return -100f;
		}
		bool flag = _data.sensor.isPlayerVisible || _data.sensor.isPlayerHeldLanternVisible || _data.sensor.inContactWithPlayer;
		if ((_running && _data.timeSincePlayerLocationKnown < 5f) || (flag && _data.playerLocation.distance < _data.playerMinLanternRange + 0.5f))
		{
			return 95f;
		}
		return -100f;
	}

	protected override void OnEnterAction()
	{
		_controller.SetLanternConcealed(concealed: false);
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Chase);
		if (Time.time > _lastScreamTime + 10f && !PlayerData.GetReducedFrights())
		{
			_effects.PlayVoiceAudioNear(AudioType.Ghost_Chase);
			_lastScreamTime = Time.time;
		}
	}

	public override bool Update_Action()
	{
		if (_data.playerLocation.distance > 10f && !_controller.GetNodeMap().CheckLocalPointInBounds(_data.lastKnownPlayerLocation.localPosition))
		{
			return false;
		}
		_controller.PathfindToLocalPosition(_data.lastKnownPlayerLocation.localPosition, MoveType.CHASE);
		_controller.FaceLocalPosition(_data.lastKnownPlayerLocation.localPosition, TurnSpeed.FAST);
		return true;
	}
}
