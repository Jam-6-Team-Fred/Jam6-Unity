using System.Collections.Generic;
using UnityEngine;

public class DreamWarpVolume : MonoBehaviour
{
	[SerializeField]
	private Transform _destinationTransform;

	[SerializeField]
	private Sector _destinationSector;

	[SerializeField]
	private Transform _loopTransform;

	[Space]
	[SerializeField]
	private OWRenderer[] _fadeDepartRenderers = new OWRenderer[0];

	[SerializeField]
	private float _fadeDepartStartDist = 30f;

	[SerializeField]
	private float _fadeDepartEndDist = 20f;

	[Space]
	[SerializeField]
	private OWRenderer[] _fadeArriveRenderers = new OWRenderer[0];

	[SerializeField]
	private float _fadeArriveStartDist = 30f;

	[SerializeField]
	private float _fadeArriveEndDist = 20f;

	[Space]
	[SerializeField]
	private OWRenderer _fallFadeRenderer;

	[SerializeField]
	private float _fallFadeTriggerDist = 10f;

	[SerializeField]
	private OWTriggerVolume _limboVolume;

	[SerializeField]
	private float _fallFadeOutLength = 0.5f;

	[SerializeField]
	private float _fallFadeInLength = 0.5f;

	[SerializeField]
	private Transform _fallDestination;

	private OWRigidbody _attachedBody;

	private OWTriggerVolume _triggerVolume;

	private OWRigidbody _destinationBody;

	private List<OWRigidbody> _trackedBodies = new List<OWRigidbody>(8);

	private bool _playerInVolume;

	private OWRigidbody _playerBody;

	private bool _playerInLimbo;

	private bool _playerFallingToUnderground;

	private bool _playerWarpedToUnderground;

	private FogWarpEffectBubbleController _playerFogWarpBubble;

	private float _fallFadeTime;

	private bool _tunnelAudioActive;

	private bool _fadingBackIn;

	public OWEvent<OWRigidbody> OnWarpBody;

	private void Awake()
	{
		_attachedBody = this.GetAttachedOWRigidbody();
		_triggerVolume = base.gameObject.GetAddComponent<OWTriggerVolume>();
		_destinationBody = _destinationTransform.GetAttachedOWRigidbody();
		_triggerVolume.OnEntry += OnEnterTriggerVolume;
		_triggerVolume.OnExit += OnExitTriggerVolume;
		SetDepartRendererActive(active: false);
		SetArriveRendererActive(active: false);
		base.enabled = false;
	}

	private void Start()
	{
		_playerFogWarpBubble = Locator.GetPlayerCamera().GetComponentInChildren<FogWarpEffectBubbleController>(includeInactive: true);
	}

	private void OnEnable()
	{
		GlobalMessenger.AddListener("ExitDreamWorld", OnPlayerExitDreamworld);
	}

	private void OnDisable()
	{
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnPlayerExitDreamworld);
	}

	private void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEnterTriggerVolume;
		_triggerVolume.OnExit -= OnExitTriggerVolume;
	}

	private void FixedUpdate()
	{
		if (_playerFallingToUnderground)
		{
			if (!_playerWarpedToUnderground)
			{
				float num = Mathf.Clamp01((Time.timeSinceLevelLoad - _fallFadeTime) / _fallFadeOutLength);
				SetFallRendererFade(num);
				if (num >= 1f)
				{
					GlobalMessenger.FireEvent("PlayerDreamWarp");
					Quaternion quaternion = Quaternion.Inverse(base.transform.rotation) * _playerBody.transform.rotation;
					_playerBody.WarpToPositionRotation(_fallDestination.position, _fallDestination.rotation * quaternion);
					_playerBody.SetVelocity(_attachedBody.GetVelocity() - _fallDestination.up * 1f);
					_playerWarpedToUnderground = true;
					_fallFadeTime = Time.timeSinceLevelLoad;
				}
				return;
			}
			float num2 = 1f - Mathf.Clamp01((Time.timeSinceLevelLoad - _fallFadeTime) / _fallFadeInLength);
			SetFallRendererFade(num2);
			if (num2 <= 0f)
			{
				_limboVolume.RemoveObjectFromVolume(Locator.GetPlayerDetector());
				_limboVolume.RemoveObjectFromVolume(Locator.GetPlayerCameraDetector());
				_limboVolume.RemoveObjectFromVolume(Locator.GetDreamWorldController().GetPlayerLantern().GetFluidDetector()
					.gameObject);
					_playerFallingToUnderground = false;
					_playerWarpedToUnderground = false;
					SetDepartRendererActive(active: false);
					SetArriveRendererActive(active: false);
					_fadingBackIn = false;
					base.enabled = false;
				}
			}
			else if (_playerInVolume)
			{
				float num3 = CalcDepartFade(_playerBody.transform.position);
				SetDepartRendererFade(num3);
				if (!_tunnelAudioActive && num3 > 0f)
				{
					Locator.GetDreamWorldAudioController().OnEnterLoadTunnel();
					_tunnelAudioActive = true;
				}
				if (!_playerInLimbo && num3 >= 1f)
				{
					_limboVolume.AddObjectToVolume(Locator.GetPlayerDetector());
					_limboVolume.AddObjectToVolume(Locator.GetPlayerCameraDetector());
					_limboVolume.AddObjectToVolume(Locator.GetDreamWorldController().GetPlayerLantern().GetFluidDetector()
						.gameObject);
						_playerInLimbo = true;
					}
					if (_playerInLimbo && CheckExitedByFalling(_playerBody.transform.position))
					{
						_playerFallingToUnderground = true;
						_fallFadeTime = Time.timeSinceLevelLoad;
						Locator.GetDreamWorldAudioController().OnExitLoadTunnel(exitByFalling: true);
						_tunnelAudioActive = false;
					}
					if (Vector3.Dot(base.transform.forward, _playerBody.transform.position - base.transform.position) > 0f)
					{
						if (_playerInVolume)
						{
							GlobalMessenger.FireEvent("PlayerDreamWarp");
						}
						for (int i = 0; i < _trackedBodies.Count; i++)
						{
							WarpBody(_trackedBodies[i], _destinationBody, _destinationTransform);
						}
						_trackedBodies.Clear();
						_playerInVolume = false;
						if (!Physics.autoSyncTransforms)
						{
							Physics.SyncTransforms();
						}
						_destinationSector.AddOccupant(Locator.GetPlayerSectorDetector());
						_fadingBackIn = true;
						SetDepartRendererActive(active: false);
						SetArriveRendererActive(active: true);
						SetArriveRendererFade(1f);
					}
				}
				else if (_fadingBackIn)
				{
					float num4 = CalcArriveFade(_playerBody.transform.position);
					SetArriveRendererFade(num4);
					if (_tunnelAudioActive && num4 < 1f)
					{
						Locator.GetDreamWorldAudioController().OnExitLoadTunnel();
						_tunnelAudioActive = false;
					}
					if (_playerInLimbo && num4 < 1f)
					{
						_limboVolume.RemoveObjectFromVolume(Locator.GetPlayerDetector());
						_limboVolume.RemoveObjectFromVolume(Locator.GetPlayerCameraDetector());
						_limboVolume.RemoveObjectFromVolume(Locator.GetDreamWorldController().GetPlayerLantern().GetFluidDetector()
							.gameObject);
							_playerInLimbo = false;
						}
						if (_playerInLimbo && CheckExitedByFalling(_playerBody.transform.position))
						{
							_playerFallingToUnderground = true;
							_fallFadeTime = Time.timeSinceLevelLoad;
							Locator.GetDreamWorldAudioController().OnExitLoadTunnel(exitByFalling: true);
							_tunnelAudioActive = false;
						}
						if (num4 <= 0f)
						{
							SetArriveRendererActive(active: false);
							_fadingBackIn = false;
							_playerBody = null;
							base.enabled = false;
						}
					}
					else
					{
						if (_tunnelAudioActive)
						{
							Locator.GetDreamWorldAudioController().OnExitLoadTunnel();
							_tunnelAudioActive = false;
						}
						SetDepartRendererActive(active: false);
						SetArriveRendererActive(active: false);
						_fadingBackIn = false;
						base.enabled = false;
					}
				}

				private void SetDepartRendererActive(bool active)
				{
					for (int i = 0; i < _fadeDepartRenderers.Length; i++)
					{
						_fadeDepartRenderers[i].SetActivation(active);
					}
				}

				private void SetArriveRendererActive(bool active)
				{
					for (int i = 0; i < _fadeArriveRenderers.Length; i++)
					{
						_fadeArriveRenderers[i].SetActivation(active);
					}
				}

				private void SetDepartRendererFade(float fade)
				{
					for (int i = 0; i < _fadeDepartRenderers.Length; i++)
					{
						Color originalColor = _fadeDepartRenderers[i].GetOriginalColor();
						_fadeDepartRenderers[i].SetColor(new Color(originalColor.r, originalColor.g, originalColor.b, fade));
					}
				}

				private void SetArriveRendererFade(float fade)
				{
					for (int i = 0; i < _fadeArriveRenderers.Length; i++)
					{
						Color originalColor = _fadeArriveRenderers[i].GetOriginalColor();
						_fadeArriveRenderers[i].SetColor(new Color(originalColor.r, originalColor.g, originalColor.b, fade));
					}
				}

				private void SetFallRendererFade(float fade)
				{
					_playerFogWarpBubble.SetFogFade(fade, Color.black);
					Color originalColor = _fallFadeRenderer.GetOriginalColor();
					_fallFadeRenderer.SetColor(new Color(originalColor.r, originalColor.g, originalColor.b, fade));
					bool flag = fade > 0f;
					if (_fallFadeRenderer.IsActive() != flag)
					{
						_fallFadeRenderer.SetActivation(flag);
					}
				}

				private float CalcDepartFade(Vector3 position)
				{
					float value = Vector3.Dot(-base.transform.forward, position - base.transform.position);
					return Mathf.InverseLerp(_fadeDepartStartDist, _fadeDepartEndDist, value);
				}

				private float CalcArriveFade(Vector3 position)
				{
					float value = Vector3.Dot(_destinationTransform.forward, position - _destinationTransform.position);
					return Mathf.InverseLerp(_fadeArriveStartDist, _fadeArriveEndDist, value);
				}

				private bool CheckExitedByFalling(Vector3 position)
				{
					return Vector3.Dot(-base.transform.up, position - base.transform.position) > _fallFadeTriggerDist;
				}

				private void WarpBody(OWRigidbody bodyToWarp, OWRigidbody destinationBody, Transform destinationTransform)
				{
					bodyToWarp.MoveToRelativeLocation(new RelativeLocationData(bodyToWarp, _attachedBody, base.transform), destinationBody, destinationTransform);
					OnWarpBody.Invoke(bodyToWarp);
				}

				private void OnEnterTriggerVolume(GameObject hitObj)
				{
					OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
					if (_trackedBodies.SafeAdd(attachedOWRigidbody) && !_playerInVolume && attachedOWRigidbody.CompareTag("Player"))
					{
						_playerInVolume = true;
						_playerBody = attachedOWRigidbody;
						_playerInLimbo = false;
						_playerFallingToUnderground = false;
						_playerWarpedToUnderground = false;
						_tunnelAudioActive = false;
						SetDepartRendererActive(active: true);
						SetDepartRendererFade(CalcDepartFade(_playerBody.transform.position));
						base.enabled = true;
					}
				}

				private void OnExitTriggerVolume(GameObject hitObj)
				{
					OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
					if (!_trackedBodies.Contains(attachedOWRigidbody))
					{
						return;
					}
					if (attachedOWRigidbody.CompareTag("Player"))
					{
						_trackedBodies.Remove(attachedOWRigidbody);
						_playerInVolume = false;
						_playerInLimbo = false;
						SetDepartRendererActive(active: false);
					}
					else if (Vector3.Dot(base.transform.forward, attachedOWRigidbody.transform.position - base.transform.position) > 0f)
					{
						if (attachedOWRigidbody.GetComponent<DreamRaftController>() != null)
						{
							Locator.GetDreamWorldController().ExtinguishDreamRaft();
							_trackedBodies.Remove(attachedOWRigidbody);
						}
						else
						{
							WarpBody(attachedOWRigidbody, _playerInVolume ? _destinationBody : _attachedBody, _playerInVolume ? _destinationTransform : _loopTransform);
							_trackedBodies.Remove(attachedOWRigidbody);
						}
					}
					else
					{
						_trackedBodies.Remove(attachedOWRigidbody);
					}
				}

				private void OnPlayerExitDreamworld()
				{
					if (_playerFallingToUnderground)
					{
						SetFallRendererFade(0f);
						_playerFallingToUnderground = false;
					}
					if (_playerInLimbo)
					{
						_limboVolume.RemoveObjectFromVolume(Locator.GetPlayerDetector());
						_limboVolume.RemoveObjectFromVolume(Locator.GetPlayerCameraDetector());
						_limboVolume.RemoveObjectFromVolume(Locator.GetDreamWorldController().GetPlayerLantern().GetFluidDetector()
							.gameObject);
							_playerInLimbo = false;
						}
						if (_tunnelAudioActive)
						{
							Locator.GetDreamWorldAudioController().OnExitLoadTunnel();
							_tunnelAudioActive = false;
						}
						if (_playerInVolume)
						{
							SetDepartRendererActive(active: false);
							SetArriveRendererActive(active: false);
							OnExitTriggerVolume(_playerBody.gameObject);
						}
					}

					private void OnDrawGizmosSelected()
					{
						Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
						Gizmos.color = Color.cyan;
						Gizmos.DrawRay(Vector3.zero, Vector3.up * 10f);
						Gizmos.DrawWireCube(Vector3.zero, new Vector3(20f, 20f, 0f));
						Vector3 vector = Vector3.back * _fadeDepartStartDist;
						Vector3 vector2 = Vector3.back * _fadeDepartEndDist;
						Gizmos.DrawLine(vector, vector2);
						Gizmos.DrawRay(vector, Vector3.up * 10f);
						Gizmos.DrawRay(vector2, Vector3.up * 10f);
						OWGizmos.DrawWireCircle(vector, Vector3.back, 10f);
						OWGizmos.DrawWireCircle(vector2, Vector3.back, 10f);
						Gizmos.color = new Color(1f, 0.5f, 0.5f, 1f);
						Vector3 vector3 = Vector3.down * _fallFadeTriggerDist + Vector3.back * _fadeDepartEndDist * 0.5f;
						Gizmos.DrawWireCube(vector3, new Vector3(20f, 0f, _fadeDepartEndDist));
						Gizmos.DrawLine(vector3 + Vector3.right * 10f + Vector3.back * _fadeDepartEndDist * 0.5f, vector3 + Vector3.left * 10f + Vector3.forward * _fadeArriveEndDist * 0.5f);
						Gizmos.DrawLine(vector3 + Vector3.left * 10f + Vector3.back * _fadeDepartEndDist * 0.5f, vector3 + Vector3.right * 10f + Vector3.forward * _fadeArriveEndDist * 0.5f);
						if (_destinationTransform != null)
						{
							Gizmos.matrix = Matrix4x4.TRS(_destinationTransform.position, _destinationTransform.rotation, Vector3.one);
							Gizmos.color = Color.green;
							Gizmos.DrawRay(Vector3.zero, Vector3.up * 10f);
							Gizmos.DrawWireCube(Vector3.zero, new Vector3(20f, 20f, 0f));
							Vector3 vector4 = Vector3.forward * _fadeArriveStartDist;
							Vector3 vector5 = Vector3.forward * _fadeArriveEndDist;
							Gizmos.DrawLine(vector4, vector5);
							Gizmos.DrawRay(vector4, Vector3.up * 10f);
							Gizmos.DrawRay(vector5, Vector3.up * 10f);
							OWGizmos.DrawWireCircle(vector4, Vector3.forward, 10f);
							OWGizmos.DrawWireCircle(vector5, Vector3.forward, 10f);
							Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
							Vector3 vector6 = Vector3.down * _fallFadeTriggerDist + Vector3.forward * _fadeArriveEndDist * 0.5f;
							Gizmos.DrawWireCube(vector6, new Vector3(20f, 0f, _fadeArriveEndDist));
							Gizmos.DrawLine(vector6 + Vector3.right * 10f + Vector3.back * _fadeArriveEndDist * 0.5f, vector6 + Vector3.left * 10f + Vector3.forward * _fadeArriveEndDist * 0.5f);
							Gizmos.DrawLine(vector6 + Vector3.left * 10f + Vector3.back * _fadeArriveEndDist * 0.5f, vector6 + Vector3.right * 10f + Vector3.forward * _fadeArriveEndDist * 0.5f);
						}
						if (_loopTransform != null)
						{
							Gizmos.matrix = Matrix4x4.TRS(_loopTransform.position, _loopTransform.rotation, Vector3.one);
							Gizmos.color = Color.magenta;
							Gizmos.DrawRay(Vector3.zero, Vector3.up * 10f);
							Gizmos.DrawWireCube(Vector3.zero, new Vector3(20f, 20f, 0f));
						}
						if (_fallDestination != null)
						{
							Gizmos.matrix = Matrix4x4.TRS(_fallDestination.position, _fallDestination.rotation, Vector3.one);
							Gizmos.color = Color.red;
							Gizmos.DrawRay(Vector3.zero, Vector3.forward * 10f);
							OWGizmos.DrawWireCircle(Vector3.zero, Vector3.up, 10f);
						}
					}
				}
