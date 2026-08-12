using System;
using System.Collections.Generic;
using UnityEngine;

public class FogLight : MonoBehaviour
{
	[Serializable]
	public class LightData
	{
		public Vector3 screenPos;

		public float scale;

		public float alpha;

		public float maxAlpha;

		public Color color;

		public void UpdatePosition(Vector3 targetScreenPos, float targetScale)
		{
			screenPos = targetScreenPos;
			scale = targetScale;
		}

		public bool UpdateAlpha(float targetAlpha, float fadeRate)
		{
			alpha = Mathf.MoveTowards(alpha, targetAlpha, Time.deltaTime * fadeRate);
			return alpha > 0f;
		}
	}

	[SerializeField]
	private Texture2D _lightIcon;

	[SerializeField]
	private Color _tint = Color.white;

	[SerializeField]
	private float _occlusionRange = 200f;

	[SerializeField]
	private float _minVisibleDistance;

	[SerializeField]
	private float _maxVisibleDistance = float.PositiveInfinity;

	[SerializeField]
	private float _maxAlpha = 0.5f;

	private OWRigidbody _parentBody;

	private Sector _sector;

	private LayerMask _blockLightMask;

	private LightData _primaryLightData = new LightData();

	private List<LightData> _linkedLightData = new List<LightData>();

	private Sector _linkedSector;

	private InnerFogWarpVolume _innerWarp;

	private List<FogLight> _linkedFogLights = new List<FogLight>();

	private void Awake()
	{
		_primaryLightData.color = _tint;
		_primaryLightData.maxAlpha = _maxAlpha;
		_blockLightMask = 1 << LayerMask.NameToLayer("Default");
		_innerWarp = GetComponentInParent<InnerFogWarpVolume>();
		if (_innerWarp != null)
		{
			_sector = _innerWarp.GetSector();
		}
		else
		{
			AnglerfishController componentInParent = GetComponentInParent<AnglerfishController>();
			if (componentInParent != null)
			{
				_sector = componentInParent.GetSector();
			}
			else
			{
				_sector = GetComponentInParent<Sector>();
			}
		}
		if (_sector == null)
		{
			Debug.LogError("Failed to find Sector", this);
			Debug.Break();
		}
		_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
		_sector.RegisterFogLight(this);
	}

	private void Start()
	{
		base.enabled = false;
		FogLightManager fogLightManager = Locator.GetFogLightManager();
		fogLightManager.RegisterLightData(_primaryLightData);
		if (_innerWarp != null && _innerWarp.GetLinkedFogWarpVolume() != null)
		{
			_linkedSector = _innerWarp.GetLinkedFogWarpVolume().GetSector();
			_linkedFogLights = _linkedSector.GetFogLights();
			if (_linkedFogLights == null)
			{
				_linkedFogLights = new List<FogLight>(0);
			}
			for (int i = 0; i < _linkedFogLights.Count; i++)
			{
				LightData lightData = new LightData();
				lightData.color = _linkedFogLights[i].GetTint();
				lightData.maxAlpha = _maxAlpha;
				_linkedLightData.Add(lightData);
				fogLightManager.RegisterLightData(lightData);
			}
		}
	}

	private void OnDisable()
	{
		_primaryLightData.alpha = 0f;
	}

	private void OnDestroy()
	{
		_sector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
	}

	public float GetMaxVisibleDistance()
	{
		return _maxVisibleDistance;
	}

	public Color GetTint()
	{
		return _tint;
	}

	private void Update()
	{
		UpdateFogLight();
	}

	private void UpdateFogLight()
	{
		float fadeRate = 2f;
		bool flag = false;
		bool flag2 = _sector.ContainsOccupant(DynamicOccupant.Player);
		Vector3 position = Locator.GetActiveCamera().transform.position;
		Vector3 direction = base.transform.position - position;
		float magnitude = direction.magnitude;
		float num = ((flag2 && !Locator.GetDeathManager().IsPlayerDying() && magnitude <= _maxVisibleDistance && magnitude >= _minVisibleDistance) ? 1f : 0f);
		if (_primaryLightData.alpha > 0f || num > 0f)
		{
			float num2 = 1f - Mathf.InverseLerp(100f, 2000f, magnitude);
			float targetScale = Mathf.Lerp(0.5f, 5f, num2 * num2);
			if (Physics.Raycast(position, direction, out var hitInfo, magnitude, _blockLightMask) && hitInfo.distance < _occlusionRange)
			{
				num = 0f;
				fadeRate = 5f;
			}
			if (flag2)
			{
				_primaryLightData.UpdatePosition(Locator.GetActiveCamera().WorldToScreenPoint(base.transform.position), targetScale);
			}
		}
		flag = _primaryLightData.UpdateAlpha(num, fadeRate) || flag;
		bool flag3 = true;
		Quaternion quaternion = Quaternion.identity;
		Vector3 a = Vector3.zero;
		for (int i = 0; i < _linkedFogLights.Count; i++)
		{
			fadeRate = 2f;
			if (_linkedFogLights[i].GetMaxVisibleDistance() != float.PositiveInfinity)
			{
				continue;
			}
			num = ((flag2 && magnitude < _minVisibleDistance) ? 1f : 0f);
			if (_linkedLightData[i].alpha > 0f || num > 0f)
			{
				OuterFogWarpVolume outerFogWarpVolume = (OuterFogWarpVolume)_innerWarp.GetLinkedFogWarpVolume();
				Vector3 vector = _innerWarp.transform.InverseTransformPoint(position);
				if (flag3)
				{
					OWRigidbody oWRigidbody = (PlayerState.IsInsideShip() ? Locator.GetShipBody() : Locator.GetPlayerBody());
					Vector3 toDirection = _innerWarp.transform.InverseTransformPoint(oWRigidbody.transform.position);
					Vector3 worldPos = outerFogWarpVolume.transform.TransformPoint(toDirection.normalized * outerFogWarpVolume.GetExitRadius());
					worldPos = outerFogWarpVolume.FindClosestWarpExitPosition(worldPos);
					Vector3 vector2 = outerFogWarpVolume.transform.InverseTransformPoint(worldPos);
					quaternion = Quaternion.FromToRotation(vector2, toDirection);
					Vector3 direction2 = position - oWRigidbody.transform.position;
					Vector3 vector3 = _innerWarp.transform.InverseTransformDirection(direction2);
					a = vector2 + Quaternion.Inverse(quaternion) * vector3;
					flag3 = false;
				}
				Vector3 vector4 = outerFogWarpVolume.transform.InverseTransformPoint(_linkedFogLights[i].transform.position);
				Vector3 vector5 = vector4;
				vector5 = quaternion * vector5;
				float value = Vector3.Distance(a, vector4);
				float num3 = 1f - Mathf.InverseLerp(100f, 2000f, value);
				float num4 = Mathf.Lerp(0.5f, 5f, num3 * num3);
				float value2 = Mathf.Max(0f, magnitude - _innerWarp.GetWarpRadius());
				float num5 = Mathf.InverseLerp(_minVisibleDistance, 0f, value2);
				num4 *= Mathf.Lerp(0.2f, 1f, num5 * num5);
				if (flag2 && (_sector != Locator.GetPlayerSectorDetector().GetLastExitedBrambleDimension() || magnitude < _minVisibleDistance))
				{
					float num6 = a.magnitude / _innerWarp.GetWarpRadius();
					float num7 = Mathf.Max(vector.magnitude, _innerWarp.GetWarpRadius()) * num6;
					Vector3 vector6 = quaternion * a.normalized * (num7 - vector.magnitude);
					Vector3 vector7 = _innerWarp.transform.TransformPoint(vector5 -= vector6);
					_linkedLightData[i].UpdatePosition(Locator.GetActiveCamera().WorldToScreenPoint(vector7), num4);
					direction = vector7 - position;
					float magnitude2 = vector.magnitude;
					if (Physics.Raycast(position, direction, out var hitInfo2, magnitude2, OWLayerMask.physicalMask) && hitInfo2.distance < _occlusionRange)
					{
						num = 0f;
						fadeRate = 5f;
					}
				}
				else
				{
					_linkedLightData[i].UpdatePosition(Locator.GetActiveCamera().WorldToScreenPoint(_linkedFogLights[i].transform.position), _linkedLightData[i].scale);
				}
			}
			flag = _linkedLightData[i].UpdateAlpha(num, fadeRate) || flag;
		}
		if (!flag2 && !flag)
		{
			base.enabled = false;
		}
	}

	private void OnOccupantEnterSector(SectorDetector detector)
	{
		if (detector.GetOccupantType() == DynamicOccupant.Player)
		{
			base.enabled = true;
			UpdateFogLight();
		}
	}

	private void OnDrawGizmos()
	{
		OWGizmos.IsDirectlySelected(base.gameObject);
	}
}
