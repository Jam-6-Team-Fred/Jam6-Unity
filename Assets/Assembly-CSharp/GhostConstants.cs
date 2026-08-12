using GhostEnums;

public static class GhostConstants
{
	public const float MAX_VISION_DISTANCE = 50f;

	public const float HALF_VISION_ANGLE_XZ = 20f;

	public const float LOW_THREAT_PLAYER_VISIBLE_DIST = 20f;

	public const float GRAB_DISTANCE = 2f;

	public const float GRAB_ANGLE = 20f;

	public const float WAKE_DELAY = 5f;

	public const float SLEEP_DELAY = 5f;

	public const float SECONDS_TO_AGGRO = 4f;

	public static float GetMoveSpeed(MoveType moveType)
	{
		switch (moveType)
		{
		case MoveType.PATROL:
		case MoveType.SEARCH:
		case MoveType.INVESTIGATE:
			return 2f;
		case MoveType.MOVE_TO_COVER:
		case MoveType.GRAB:
		case MoveType.CHASE:
			return 8f;
		default:
			return 0f;
		}
	}

	public static float GetMoveAcceleration(MoveType moveType)
	{
		switch (moveType)
		{
		case MoveType.PATROL:
			return 5f;
		case MoveType.SEARCH:
			return 10f;
		case MoveType.MOVE_TO_COVER:
			return 20f;
		case MoveType.GRAB:
			return 30f;
		case MoveType.INVESTIGATE:
			return 10f;
		case MoveType.CHASE:
			return 30f;
		default:
			return 0f;
		}
	}

	public static float GetTurnSpeed(TurnSpeed turnSpeed)
	{
		switch (turnSpeed)
		{
		case TurnSpeed.SLOWEST:
			return 22f;
		case TurnSpeed.SLOW:
			return 45f;
		case TurnSpeed.MEDIUM:
			return 90f;
		case TurnSpeed.FAST:
			return 180f;
		case TurnSpeed.FASTEST:
			return 360f;
		default:
			return 0f;
		}
	}

	public static float GetTurnAcceleration(TurnSpeed turnSpeed)
	{
		switch (turnSpeed)
		{
		case TurnSpeed.SLOWEST:
		case TurnSpeed.SLOW:
		case TurnSpeed.MEDIUM:
			return 360f;
		case TurnSpeed.FAST:
		case TurnSpeed.FASTEST:
			return 1080f;
		default:
			return 0f;
		}
	}
}
