using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CullGroup : MonoBehaviour
{
	public enum ParticleSystemSuspendMode
	{
		None = 0,
		Pause = 1,
		Stop = 2
	}

	[Serializable]
	public struct ParticleSystemPair
	{
		public Renderer renderer;

		public ParticleSystem particleSystem;

		public ParticleSystemPair(Renderer r, ParticleSystem ps)
		{
			renderer = r;
			particleSystem = ps;
		}
	}

	public static MaterialPropertyBlock s_matPropBlock;

	public static int s_propID_unityLODFade;

	[SerializeField]
	[HideInInspector]
	private bool _prebuilt;

	[SerializeField]
	[HideInInspector]
	private List<Renderer> _staticRenderers;

	[SerializeField]
	[HideInInspector]
	private List<OWRenderer> _dynamicRenderers;

	[SerializeField]
	[HideInInspector]
	private List<ParticleSystemPair> _particleSystems;

	[SerializeField]
	[HideInInspector]
	private List<StreamingRenderMeshHandle> _streamingMeshes;

	[SerializeField]
	[HideInInspector]
	private List<LODGroup> _lodGroups;

	[SerializeField]
	[HideInInspector]
	private SphereBounds _dynamicSphereBounds;

	[SerializeField]
	[HideInInspector]
	private SphereBounds _finalSphereBounds;

	[SerializeField]
	[HideInInspector]
	private Vector3 _localStaticBoundsCenter;

	[SerializeField]
	[HideInInspector]
	private SphereBounds _staticSphereBounds;

	[SerializeField]
	private bool _crossfade = true;

	[SerializeField]
	private float _crossfadeLength = 1f;

	[SerializeField]
	private bool _occlusionCulling;

	[SerializeField]
	private bool _dynamicCullingBounds;

	[SerializeField]
	private ParticleSystemSuspendMode _particleSystemSuspendMode = ParticleSystemSuspendMode.Stop;

	[SerializeField]
	private bool _waitForStreaming;

	private bool _preOcclusionRenderersEnabled;

	private bool _visible = true;

	private bool _renderersEnabled = true;

	private bool _particleSystemsSuspended;

	private bool _fadeComplete = true;

	private float _fadeTimer = 1f;

	private int _streamingMeshLoadCounter;

	protected virtual void Awake()
	{
		if (!_prebuilt)
		{
			BuildCullGroup();
			if (_waitForStreaming)
			{
				FindStreamingMeshes();
			}
		}
		_fadeTimer = _crossfadeLength;
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_unityLODFade = Shader.PropertyToID("unity_LODFade");
		}
		if (_waitForStreaming && _streamingMeshes != null)
		{
			StreamingMeshHandle.StreamingMeshEvent value = OnStreamingMeshLoaded;
			StreamingMeshHandle.StreamingMeshEvent value2 = OnStreamingMeshUnloaded;
			for (int i = 0; i < _streamingMeshes.Count; i++)
			{
				_streamingMeshes[i].OnMeshLoaded += value;
				_streamingMeshes[i].OnMeshUnloaded += value2;
			}
		}
		base.enabled = false;
	}

	protected virtual void OnDestroy()
	{
		if (!_waitForStreaming || _streamingMeshes == null)
		{
			return;
		}
		StreamingMeshHandle.StreamingMeshEvent value = OnStreamingMeshLoaded;
		StreamingMeshHandle.StreamingMeshEvent value2 = OnStreamingMeshUnloaded;
		for (int i = 0; i < _streamingMeshes.Count; i++)
		{
			if (_streamingMeshes[i] != null)
			{
				_streamingMeshes[i].OnMeshLoaded -= value;
				_streamingMeshes[i].OnMeshUnloaded -= value2;
			}
		}
	}

	private void OnStreamingMeshLoaded()
	{
		_streamingMeshLoadCounter++;
	}

	private void OnStreamingMeshUnloaded()
	{
		_streamingMeshLoadCounter--;
	}

	protected virtual void LateUpdate()
	{
		if (_crossfade && !_fadeComplete)
		{
			if (_visible)
			{
				if (!_renderersEnabled)
				{
					SetRenderersEnabled(newEnabled: true);
				}
				if (_particleSystemsSuspended)
				{
					SetParticleSystemsSuspended(newSuspended: false);
				}
			}
			_fadeTimer = Mathf.MoveTowards(_fadeTimer, _visible ? _crossfadeLength : 0f, Time.deltaTime);
			float num = _fadeTimer / _crossfadeLength;
			if (!_visible && num <= 0f)
			{
				SetRenderersEnabled(newEnabled: false);
				SetParticleSystemsSuspended(newSuspended: true);
				ClearRenderersCrossfade();
				_fadeComplete = true;
			}
			else if (_visible && num >= 1f)
			{
				ClearRenderersCrossfade();
				_fadeComplete = true;
			}
			else
			{
				SetRenderersCrossfade(num);
			}
		}
		if (!_crossfade || _fadeComplete)
		{
			base.enabled = false;
		}
	}

	protected void SetRenderersEnabled(bool newEnabled)
	{
		if (_renderersEnabled == newEnabled)
		{
			return;
		}
		for (int i = 0; i < _staticRenderers.Count; i++)
		{
			if (_staticRenderers[i] != null)
			{
				_staticRenderers[i].enabled = newEnabled;
			}
		}
		for (int j = 0; j < _dynamicRenderers.Count; j++)
		{
			if (_dynamicRenderers[j] != null)
			{
				_dynamicRenderers[j].SetLODActivation(newEnabled);
			}
		}
		for (int k = 0; k < _particleSystems.Count; k++)
		{
			if (_particleSystems[k].renderer != null)
			{
				_particleSystems[k].renderer.enabled = newEnabled;
			}
		}
		if (_lodGroups != null)
		{
			for (int l = 0; l < _lodGroups.Count; l++)
			{
				if (_lodGroups[l] != null)
				{
					_lodGroups[l].enabled = newEnabled;
				}
			}
		}
		_renderersEnabled = newEnabled;
	}

	protected void SetParticleSystemsSuspended(bool newSuspended)
	{
		if (_particleSystemsSuspended == newSuspended)
		{
			return;
		}
		if (_particleSystemSuspendMode != 0)
		{
			for (int i = 0; i < _particleSystems.Count; i++)
			{
				if (!(_particleSystems[i].particleSystem != null))
				{
					continue;
				}
				if (newSuspended)
				{
					if (_particleSystemSuspendMode == ParticleSystemSuspendMode.Pause)
					{
						_particleSystems[i].particleSystem.Pause(withChildren: true);
					}
					else
					{
						_particleSystems[i].particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
					}
				}
				else
				{
					_particleSystems[i].particleSystem.Play(withChildren: true);
				}
			}
		}
		_particleSystemsSuspended = newSuspended;
	}

	protected void SetRenderersCrossfade(float fadeFactor)
	{
		if (!_crossfade)
		{
			return;
		}
		float y = Mathf.Floor(fadeFactor * 16f) / 16f;
		Vector4 value = new Vector4(fadeFactor, y, 0f, 0f);
		s_matPropBlock.SetVector(s_propID_unityLODFade, value);
		for (int i = 0; i < _staticRenderers.Count; i++)
		{
			if (_staticRenderers[i] != null)
			{
				_staticRenderers[i].SetPropertyBlock(s_matPropBlock);
			}
		}
		for (int j = 0; j < _dynamicRenderers.Count; j++)
		{
			if (_dynamicRenderers[j] != null)
			{
				_dynamicRenderers[j].SetMaterialProperty(s_propID_unityLODFade, value);
			}
		}
		for (int k = 0; k < _particleSystems.Count; k++)
		{
			if (_particleSystems[k].renderer != null)
			{
				_particleSystems[k].renderer.SetPropertyBlock(s_matPropBlock);
			}
		}
	}

	protected void ClearRenderersCrossfade()
	{
		for (int i = 0; i < _staticRenderers.Count; i++)
		{
			if (_staticRenderers[i] != null)
			{
				_staticRenderers[i].SetPropertyBlock(null);
			}
		}
		for (int j = 0; j < _dynamicRenderers.Count; j++)
		{
			if (_dynamicRenderers[j] != null)
			{
				_dynamicRenderers[j].SetMaterialProperty(s_propID_unityLODFade, Vector4.zero);
			}
		}
		for (int k = 0; k < _particleSystems.Count; k++)
		{
			if (_particleSystems[k].renderer != null)
			{
				_particleSystems[k].renderer.SetPropertyBlock(null);
			}
		}
	}

	private void Cull(OWCamera owCamera)
	{
		_preOcclusionRenderersEnabled = _renderersEnabled;
		if (!_renderersEnabled || OcclusionSphere._occluders == null || owCamera is OWSceneViewCamera)
		{
			return;
		}
		UpdateGroupBounds();
		Vector3 position = owCamera.transform.position;
		for (int i = 0; i < OcclusionSphere._occluders.Count; i++)
		{
			if (OcclusionSphere._occluders[i].GetSphereBounds().Occludes(_finalSphereBounds, position))
			{
				SetRenderersEnabled(newEnabled: false);
				break;
			}
		}
	}

	private void RevertCull(OWCamera owCamera)
	{
		if (_renderersEnabled != _preOcclusionRenderersEnabled && OcclusionSphere._occluders != null && !(owCamera is OWSceneViewCamera))
		{
			SetRenderersEnabled(_preOcclusionRenderersEnabled);
		}
	}

	public static SphereBounds CalculateAlignedBounds(List<Renderer> renderers)
	{
		SphereBounds result = new SphereBounds(Vector3.zero, 0f);
		if (renderers == null || renderers.Count == 0)
		{
			return result;
		}
		bool flag = false;
		for (int i = 0; i < renderers.Count; i++)
		{
			if (!(renderers[i] == null))
			{
				if (!flag)
				{
					result = new SphereBounds(renderers[i].bounds);
					flag = true;
				}
				else
				{
					result.Encapsulate(renderers[i].bounds);
				}
			}
		}
		return result;
	}

	public static SphereBounds CalculateAlignedBounds(List<ParticleSystemPair> renderers)
	{
		SphereBounds result = new SphereBounds(Vector3.zero, 0f);
		if (renderers == null || renderers.Count == 0)
		{
			return result;
		}
		bool flag = false;
		for (int i = 0; i < renderers.Count; i++)
		{
			if (!(renderers[i].renderer == null))
			{
				if (!flag)
				{
					result = new SphereBounds(renderers[i].renderer.bounds);
					flag = true;
				}
				else
				{
					result.Encapsulate(renderers[i].renderer.bounds);
				}
			}
		}
		return result;
	}

	public static SphereBounds CalculateOrientedBounds(List<Renderer> renderers, List<OWRenderer> dynamicRenderers)
	{
		SphereBounds result = new SphereBounds(Vector3.zero, 0f);
		if (renderers == null && dynamicRenderers == null)
		{
			return result;
		}
		bool flag = false;
		if (renderers != null)
		{
			for (int i = 0; i < renderers.Count; i++)
			{
				if (renderers[i] == null)
				{
					continue;
				}
				SphereBounds sphereBounds;
				if (renderers[i] is MeshRenderer)
				{
					MeshFilter component = renderers[i].GetComponent<MeshFilter>();
					if (!component || !component.sharedMesh)
					{
						continue;
					}
					Matrix4x4 localToWorldMatrix = renderers[i].transform.localToWorldMatrix;
					Bounds bounds = component.sharedMesh.bounds;
					Vector3 sphereCenter = localToWorldMatrix.MultiplyPoint3x4(bounds.center);
					float magnitude = localToWorldMatrix.MultiplyVector(bounds.extents).magnitude;
					sphereBounds = new SphereBounds(sphereCenter, magnitude);
				}
				else
				{
					sphereBounds = new SphereBounds(renderers[i].bounds);
				}
				if (!flag)
				{
					result = sphereBounds;
					flag = true;
				}
				else
				{
					result.Encapsulate(sphereBounds);
				}
			}
		}
		if (dynamicRenderers != null)
		{
			for (int j = 0; j < dynamicRenderers.Count; j++)
			{
				if (dynamicRenderers[j] == null)
				{
					continue;
				}
				Renderer renderer = dynamicRenderers[j].GetRenderer();
				SphereBounds sphereBounds2;
				if (renderer is MeshRenderer)
				{
					MeshFilter component2 = renderer.GetComponent<MeshFilter>();
					if (!component2 || !component2.sharedMesh)
					{
						continue;
					}
					Matrix4x4 localToWorldMatrix2 = renderer.transform.localToWorldMatrix;
					Bounds bounds2 = component2.sharedMesh.bounds;
					Vector3 sphereCenter2 = localToWorldMatrix2.MultiplyPoint3x4(bounds2.center);
					float magnitude2 = localToWorldMatrix2.MultiplyVector(bounds2.extents).magnitude;
					sphereBounds2 = new SphereBounds(sphereCenter2, magnitude2);
				}
				else
				{
					sphereBounds2 = new SphereBounds(renderer.bounds);
				}
				if (!flag)
				{
					result = sphereBounds2;
					flag = true;
				}
				else
				{
					result.Encapsulate(sphereBounds2);
				}
			}
		}
		return result;
	}

	public void RecalculateGroupBounds()
	{
		if (_staticRenderers == null && _dynamicRenderers == null)
		{
			_staticSphereBounds = new SphereBounds(base.transform.position, 0f);
			_localStaticBoundsCenter = Vector3.zero;
		}
		else
		{
			_staticSphereBounds = CalculateOrientedBounds(_staticRenderers, _dynamicRenderers);
			_localStaticBoundsCenter = base.transform.InverseTransformPoint(_staticSphereBounds.center);
		}
		if (_particleSystems == null || _particleSystems.Count == 0)
		{
			_dynamicSphereBounds = new SphereBounds(base.transform.position, 0f);
		}
		else
		{
			_dynamicSphereBounds = CalculateAlignedBounds(_particleSystems);
		}
	}

	public void UpdateGroupBounds()
	{
		_staticSphereBounds.center = base.transform.TransformPoint(_localStaticBoundsCenter);
		_finalSphereBounds = _staticSphereBounds;
		if (_dynamicCullingBounds && _particleSystems != null && _particleSystems.Count > 0)
		{
			_dynamicSphereBounds = CalculateAlignedBounds(_particleSystems);
			_finalSphereBounds = _staticSphereBounds;
			_finalSphereBounds.Encapsulate(_dynamicSphereBounds);
		}
	}

	private void BuildCullGroup()
	{
		_staticRenderers = new List<Renderer>();
		_dynamicRenderers = new List<OWRenderer>();
		_particleSystems = new List<ParticleSystemPair>();
		_lodGroups = new List<LODGroup>();
		RecursivelyAddRenderers(base.transform);
		if (_lodGroups.Count == 0)
		{
			_lodGroups = null;
		}
		RecalculateGroupBounds();
	}

	private void FindStreamingMeshes()
	{
		_streamingMeshes = new List<StreamingRenderMeshHandle>();
		for (int i = 0; i < _staticRenderers.Count; i++)
		{
			StreamingRenderMeshHandle component = _staticRenderers[i].GetComponent<StreamingRenderMeshHandle>();
			if (component != null)
			{
				_streamingMeshes.Add(component);
			}
		}
		for (int j = 0; j < _dynamicRenderers.Count; j++)
		{
			StreamingRenderMeshHandle component2 = _dynamicRenderers[j].GetComponent<StreamingRenderMeshHandle>();
			if (component2 != null)
			{
				_streamingMeshes.Add(component2);
			}
		}
	}

	protected void RecursivelyAddRenderers(Transform parent, bool addOWRenderer = false)
	{
		if (!ShouldIncludeObject(parent))
		{
			return;
		}
		if (!addOWRenderer)
		{
			IGroupController component = parent.GetComponent<IGroupController>();
			if (component != null)
			{
				addOWRenderer = (component.groupControlMask & 1) > 0;
			}
		}
		Renderer component2 = parent.GetComponent<Renderer>();
		if (component2 != null)
		{
			OWRenderer component3 = parent.GetComponent<OWRenderer>();
			if (component3 != null)
			{
				_dynamicRenderers.Add(component3);
			}
			else if (addOWRenderer)
			{
				_dynamicRenderers.Add(component2.gameObject.AddComponent<OWRenderer>());
			}
			else if (component2 is ParticleSystemRenderer)
			{
				_particleSystems.Add(new ParticleSystemPair(component2, component2.GetComponent<ParticleSystem>()));
			}
			else
			{
				_staticRenderers.Add(component2);
			}
		}
		LODGroup component4 = parent.GetComponent<LODGroup>();
		if (component4 != null)
		{
			_lodGroups.Add(component4);
		}
		foreach (Transform item in parent)
		{
			RecursivelyAddRenderers(item, addOWRenderer);
		}
	}

	protected virtual bool ShouldIncludeObject(Transform transform)
	{
		if (transform == null)
		{
			return false;
		}
		CullGroupExcluder component = transform.GetComponent<CullGroupExcluder>();
		if (component != null && component.gameObject != base.gameObject)
		{
			return false;
		}
		CullGroup component2 = transform.GetComponent<CullGroup>();
		if (component2 != null && component2.gameObject != base.gameObject)
		{
			return false;
		}
		if (transform.GetComponent<SectorProxy>() != null)
		{
			return false;
		}
		if (transform.GetComponent<OWItem>() != null)
		{
			return false;
		}
		return true;
	}

	public int GetTotalRendererCount()
	{
		return _staticRenderers.Count + _dynamicRenderers.Count + _particleSystems.Count;
	}

	public int GetGeneralRendererCount()
	{
		return _staticRenderers.Count + _dynamicRenderers.Count;
	}

	public int GetParticleSystemRenderersCount()
	{
		return _particleSystems.Count;
	}

	public SphereBounds GetSphereBounds()
	{
		return _finalSphereBounds;
	}

	public void SetVisible(bool visible, bool instant = false, bool updateSuspension = true)
	{
		_visible = visible;
		if (_crossfade && !instant)
		{
			_fadeComplete = false;
			base.enabled = true;
			return;
		}
		SetRenderersEnabled(visible);
		if (updateSuspension)
		{
			SetParticleSystemsSuspended(!visible);
		}
		if (!_fadeComplete)
		{
			ClearRenderersCrossfade();
			_fadeComplete = true;
		}
		_fadeTimer = (visible ? _crossfadeLength : 0f);
	}

	public bool IsVisible()
	{
		return _visible;
	}

	public bool IsCrossfading()
	{
		return !_fadeComplete;
	}

	public bool IsWaitingForStreaming()
	{
		if (_waitForStreaming)
		{
			return _streamingMeshLoadCounter < _streamingMeshes.Count;
		}
		return false;
	}
}
