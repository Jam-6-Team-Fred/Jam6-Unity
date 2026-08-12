using UnityEngine;

public class PlayerFogWarpDetector : FogWarpDetector
{
	private FogWarpEffectBubbleController _playerEffectBubbleController;

	private FogWarpEffectBubbleController _shipLandingCamEffectBubbleController;

	private FogWarpDetector _shipFogDetector;

	private float _fogFraction;

	private Color _fogColor;

	private FogWarpVolume _targetFogColorWarpVolume;

	private float _startColorCrossfadeTime;

	private Color _startCrossfadeColor;

	protected override void Start()
	{
		base.Start();
		if (_name != Name.Player)
		{
			Debug.LogError("Player fog warp detector not named correctly");
			Debug.Break();
		}
		_playerEffectBubbleController = Locator.GetPlayerCamera().GetComponentInChildren<FogWarpEffectBubbleController>(includeInactive: true);
		if (Locator.GetShipDetector() != null)
		{
			_shipLandingCamEffectBubbleController = Locator.GetShipTransform().GetComponentInChildren<FogWarpEffectBubbleController>(includeInactive: true);
			_shipFogDetector = Locator.GetShipDetector().GetComponent<FogWarpDetector>();
		}
	}

	private void OnDisable()
	{
		_fogFraction = 0f;
	}

	public override void OnFogWarp()
	{
		base.OnFogWarp();
		_fogFraction = 1f;
	}

	private void LateUpdate()
	{
		if (!(PlanetaryFogController.GetActiveFogSphere() != null))
		{
			return;
		}
		float num = _targetFogFraction;
		if (PlayerState.IsInsideShip())
		{
			num = Mathf.Max(_shipFogDetector.GetTargetFogFraction(), num);
		}
		if (num < _fogFraction)
		{
			float num2 = (_closestFogWarp.UseFastFogFade() ? 1f : 0.2f);
			_fogFraction = Mathf.MoveTowards(_fogFraction, num, Time.deltaTime * num2);
		}
		else
		{
			_fogFraction = num;
		}
		if (_targetFogColorWarpVolume != _closestFogWarp)
		{
			_targetFogColorWarpVolume = _closestFogWarp;
			_startColorCrossfadeTime = Time.time;
			_startCrossfadeColor = _fogColor;
		}
		if (_targetFogColorWarpVolume != null)
		{
			Color fogColor = _targetFogColorWarpVolume.GetFogColor();
			if (_fogFraction <= 0f)
			{
				_fogColor = fogColor;
			}
			else
			{
				float t = Mathf.InverseLerp(_startColorCrossfadeTime, _startColorCrossfadeTime + 1f, Time.time);
				_fogColor = Color.Lerp(_startCrossfadeColor, fogColor, t);
			}
		}
		if (_playerEffectBubbleController != null)
		{
			_playerEffectBubbleController.SetFogFade(_fogFraction, _fogColor);
		}
		if (_shipLandingCamEffectBubbleController != null)
		{
			_shipLandingCamEffectBubbleController.SetFogFade(_fogFraction, _fogColor);
		}
	}
}
