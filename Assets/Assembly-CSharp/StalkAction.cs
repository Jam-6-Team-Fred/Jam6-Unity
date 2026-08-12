using GhostEnums;
using UnityEngine;

public class StalkAction : GhostAction
{
	private bool _wasPlayerLanternConcealed;

	private bool _isFocusingLight;

	private bool _shouldFocusLightOnPlayer;

	private float _changeFocusTime;

	public override Name GetName()
	{
		return Name.Stalk;
	}

	public override float CalculateUtility()
	{
		if (_data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed)
		{
			return -100f;
		}
		if ((_running && _data.timeSincePlayerLocationKnown < 4f) || _data.isPlayerLocationKnown)
		{
			return 85f;
		}
		return -100f;
	}

	protected override void OnEnterAction()
	{
		_isFocusingLight = (_shouldFocusLightOnPlayer = (_wasPlayerLanternConcealed = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController()
			.IsConcealed()) || !_data.sensor.isPlayerHoldingLantern);
		_changeFocusTime = 0f;
		_controller.ChangeLanternFocus(_isFocusingLight ? 1f : 0f);
		_controller.SetLanternConcealed(!_isFocusingLight);
		_controller.FaceVelocity();
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
		_effects.PlayVoiceAudioNear(_data.fastStalkUnlocked ? AudioType.Ghost_Stalk_Fast : AudioType.Ghost_Stalk);
	}

	public override bool Update_Action()
	{
		if (!_data.fastStalkUnlocked && _data.illuminatedByPlayerMeter > 4f)
		{
			_data.fastStalkUnlocked = true;
			_effects.PlayVoiceAudioNear(AudioType.Ghost_Stalk_Fast);
		}
		return true;
	}

	public override void FixedUpdate_Action()
	{
		float num = GhostConstants.GetMoveSpeed(MoveType.SEARCH);
		if (_data.fastStalkUnlocked)
		{
			num += 1.5f;
		}
		if (_controller.GetNodeMap().CheckLocalPointInBounds(_data.lastKnownPlayerLocation.localPosition))
		{
			_controller.PathfindToLocalPosition(_data.lastKnownPlayerLocation.localPosition, num, GhostConstants.GetMoveAcceleration(MoveType.SEARCH));
		}
		_controller.FaceLocalPosition(_data.lastKnownPlayerLocation.localPosition, TurnSpeed.MEDIUM);
		bool flag = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController()
			.IsConcealed();
		bool num2 = !_wasPlayerLanternConcealed && flag && _data.wasPlayerLocationKnown;
		_wasPlayerLanternConcealed = flag;
		bool flag2 = (!_data.sensor.isPlayerHoldingLantern && _data.wasPlayerLocationKnown) || _data.sensor.isPlayerDroppedLanternVisible;
		if ((num2 || flag2) && !_shouldFocusLightOnPlayer)
		{
			_shouldFocusLightOnPlayer = true;
			_changeFocusTime = Time.time + 0.5f;
		}
		else if (_data.sensor.isPlayerHeldLanternVisible && _shouldFocusLightOnPlayer)
		{
			_shouldFocusLightOnPlayer = false;
			_changeFocusTime = Time.time + 0.5f;
		}
		if (_isFocusingLight != _shouldFocusLightOnPlayer && Time.time > _changeFocusTime)
		{
			if (_shouldFocusLightOnPlayer)
			{
				_controller.SetLanternConcealed(concealed: false);
				_controller.ChangeLanternFocus(1f);
			}
			else
			{
				_controller.SetLanternConcealed(concealed: true);
			}
			_isFocusingLight = _shouldFocusLightOnPlayer;
		}
	}
}
