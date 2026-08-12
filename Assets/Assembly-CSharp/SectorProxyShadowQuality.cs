using UnityEngine;

public class SectorProxyShadowQuality : SectoredMonoBehaviour
{
	private bool _overrideActive;

	private ProxyShadowCascade.Division[] _overrideCascadeDivisions;

	private float _prevShadowDistance;

	private int _prevShadowCascades;

	private ShadowResolution _prevShadowResolution;

	private ProxyShadowCascade.Division[] _prevCascadeDivisions;

	protected override void Awake()
	{
	}

	protected override void OnDestroy()
	{
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = _sector.ContainsOccupant(DynamicOccupant.Player);
		if (flag && !_overrideActive)
		{
			ApplyOverride();
		}
		else if (!flag && _overrideActive)
		{
			RevertOverride();
		}
	}

	private void ApplyOverride()
	{
		if (!_overrideActive)
		{
			_prevShadowDistance = QualitySettings.shadowDistance;
			_prevShadowCascades = QualitySettings.shadowCascades;
			_prevShadowResolution = QualitySettings.shadowResolution;
			_prevCascadeDivisions = ProxyShadowSettings.cascadeDivisions;
			QualitySettings.shadowDistance = 50f;
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowResolution = ShadowResolution.Low;
			ProxyShadowSettings.cascadeDivisions = _overrideCascadeDivisions;
			_overrideActive = true;
		}
	}

	private void RevertOverride()
	{
		if (_overrideActive)
		{
			QualitySettings.shadowDistance = _prevShadowDistance;
			QualitySettings.shadowCascades = _prevShadowCascades;
			QualitySettings.shadowResolution = _prevShadowResolution;
			ProxyShadowSettings.cascadeDivisions = _prevCascadeDivisions;
			_overrideActive = false;
		}
	}
}
