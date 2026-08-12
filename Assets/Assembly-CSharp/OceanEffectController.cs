using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OceanEffectController : SectoredMonoBehaviour
{
	private struct Splash
	{
		public Vector3 localPosition;

		public float localRadius;

		public float startTime;

		public float length;

		public float localHeight;

		public float localWidth;
	}

	private const int kNumCalmZones = 8;

	private const int kNumChopZones = 16;

	private const int kNumSplashes = 4;

	[SerializeField]
	private TessellatedSphereRenderer _ocean;

	private Transform _oceanTransform;

	private bool _initialized;

	private bool _active;

	private List<OceanCalmZone> _calmZones;

	private Vector4[] _calmZonePositionsArray;

	private Vector4[] _calmZoneParamsArray;

	private int _propID_CalmZonePositions;

	private int _propID_CalmZoneParams;

	private List<OceanChopZone> _chopZones;

	private Vector4[] _chopZoneArray;

	private int _propID_ChopZones;

	private List<Splash> _splashes;

	private Vector4[] _splashPositionsArray;

	private Vector4[] _splashParamsArray;

	private int _propID_SplashPositions;

	private int _propID_SplashParams;

	protected override void Awake()
	{
		base.Awake();
		if (!_initialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		_calmZones = new List<OceanCalmZone>(8);
		_calmZonePositionsArray = new Vector4[8];
		_calmZoneParamsArray = new Vector4[8];
		_propID_CalmZonePositions = Shader.PropertyToID("_CalmZonePositions");
		_propID_CalmZoneParams = Shader.PropertyToID("_CalmZoneParams");
		_chopZones = new List<OceanChopZone>(16);
		_chopZoneArray = new Vector4[16];
		_propID_ChopZones = Shader.PropertyToID("_ChopZones");
		_splashes = new List<Splash>(4);
		_splashPositionsArray = new Vector4[4];
		_splashParamsArray = new Vector4[4];
		_propID_SplashPositions = Shader.PropertyToID("_SplashPositions");
		_propID_SplashParams = Shader.PropertyToID("_SplashParams");
		_initialized = true;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (_active && !flag)
		{
			_splashes.Clear();
			for (int i = 0; i < 4; i++)
			{
				_splashPositionsArray[i].Set(0f, 0f, 0f, 1f);
				_splashParamsArray[i].Set(0f, 0f, 1f, 0f);
			}
			Shader.SetGlobalVectorArray(_propID_SplashPositions, _splashPositionsArray);
			Shader.SetGlobalVectorArray(_propID_SplashParams, _splashParamsArray);
		}
		_active = flag;
		base.enabled = flag;
	}

	private void LateUpdate()
	{
		if (_ocean == null)
		{
			return;
		}
		if (_oceanTransform == null)
		{
			_oceanTransform = _ocean.transform;
		}
		float num = Mathf.Max(Mathf.Max(_oceanTransform.lossyScale.x, _oceanTransform.lossyScale.y), _oceanTransform.lossyScale.z);
		for (int i = 0; i < 8; i++)
		{
			if (_calmZones != null && i < _calmZones.Count)
			{
				Vector3 vector = _oceanTransform.InverseTransformPoint(_calmZones[i].transform.position);
				float num2 = _calmZones[i].globalRadius / num;
				float newY = num2 * (1f - _calmZones[i].fadeFactor);
				_calmZonePositionsArray[i].Set(vector.x, vector.y, vector.z, num2);
				_calmZoneParamsArray[i].Set(_calmZones[i].strength, newY, 0f, 0f);
			}
			else
			{
				_calmZonePositionsArray[i].Set(0f, 0f, 0f, 0f);
				_calmZoneParamsArray[i].Set(0f, 0f, 0f, 0f);
			}
		}
		for (int j = 0; j < 16; j++)
		{
			if (_chopZones != null && j < _chopZones.Count)
			{
				Vector3 vector2 = _oceanTransform.InverseTransformPoint(_chopZones[j].transform.position);
				float newW = _chopZones[j].globalRadius / num;
				_chopZoneArray[j].Set(vector2.x, vector2.y, vector2.z, newW);
			}
			else
			{
				_chopZoneArray[j].Set(0f, 0f, 0f, 0f);
			}
		}
		for (int k = 0; k < 4; k++)
		{
			if (_splashes != null && k < _splashes.Count)
			{
				float f = (Time.time - _splashes[k].startTime) / _splashes[k].length;
				_splashPositionsArray[k].Set(_splashes[k].localPosition.x, _splashes[k].localPosition.y, _splashes[k].localPosition.z, _splashes[k].localRadius);
				_splashParamsArray[k].Set(Mathf.Sqrt(f), _splashes[k].localHeight, _splashes[k].localWidth, 0f);
			}
			else
			{
				_splashPositionsArray[k].Set(0f, 0f, 0f, 1f);
				_splashParamsArray[k].Set(0f, 0f, 1f, 0f);
			}
		}
		if (_splashes != null)
		{
			for (int l = 0; l < _splashes.Count; l++)
			{
				if (Time.time > _splashes[l].startTime + _splashes[l].length)
				{
					_splashes.QuickRemoveAt(l);
					l--;
				}
			}
		}
		for (int m = 0; m < _ocean.sharedMaterials.Length; m++)
		{
			Material obj = _ocean.sharedMaterials[m];
			obj.SetVectorArray(_propID_CalmZonePositions, _calmZonePositionsArray);
			obj.SetVectorArray(_propID_CalmZoneParams, _calmZoneParamsArray);
			obj.SetVectorArray(_propID_ChopZones, _chopZoneArray);
			obj.SetVectorArray(_propID_SplashPositions, _splashPositionsArray);
			obj.SetVectorArray(_propID_SplashParams, _splashParamsArray);
		}
	}

	public void AddCalmZone(OceanCalmZone calmZone)
	{
		if (!_initialized)
		{
			Initialize();
		}
		_calmZones.Add(calmZone);
	}

	public void RemoveCalmZone(OceanCalmZone calmZone)
	{
		if (!_initialized)
		{
			Initialize();
		}
		_calmZones.Remove(calmZone);
	}

	public void AddChopZone(OceanChopZone chopZone)
	{
		if (!_initialized)
		{
			Initialize();
		}
		_chopZones.Add(chopZone);
	}

	public void RemoveChopZone(OceanChopZone chopZone)
	{
		if (!_initialized)
		{
			Initialize();
		}
		_chopZones.QuickRemove(chopZone);
	}

	public void CreateSplash(Vector3 worldPos, float radius, float splashLength, float waveHeight, float waveWidth)
	{
		if (_active)
		{
			if (!_initialized)
			{
				Initialize();
			}
			float num = Mathf.Max(Mathf.Max(_oceanTransform.lossyScale.x, _oceanTransform.lossyScale.y), _oceanTransform.lossyScale.z);
			Splash item = default(Splash);
			item.localPosition = _oceanTransform.InverseTransformPoint(worldPos);
			item.localRadius = radius / num;
			item.startTime = Time.time;
			item.length = splashLength;
			item.localHeight = waveHeight / num;
			item.localWidth = waveWidth / num;
			_splashes.Add(item);
		}
	}
}
