using UnityEngine;

[RequireComponent(typeof(OWCamera))]
public class ProbeCamera : MonoBehaviour
{
	public enum ID
	{
		Forward = 0,
		Reverse = 1,
		PreLaunch = 2,
		Rotating = 3
	}

	public delegate void RotateCameraEvent(Vector2 rotation);

	public const int SNAPSHOT_RESOLUTION = 512;

	private static RenderTexture s_sharedSnapshotTexure;

	[SerializeField]
	private ID _id;

	private OWCamera _camera;

	private RenderTexture _snapshotTexture;

	private NoiseImageEffect _noiseEffect;

	private static OWCamera _lastSnapshotCamera;

	private Quaternion _origLocalRotation;

	private Quaternion _origParentLocalRotation;

	private Vector2 _cameraRotation = Vector2.zero;

	private QuantumMoon _quantumMoon;

	private SandLevelController _sandLevelController;

	public event RotateCameraEvent OnRotateCamera;

	public static RenderTexture GetSharedSnapshotTexture()
	{
		if (s_sharedSnapshotTexure == null)
		{
			s_sharedSnapshotTexure = new RenderTexture(512, 512, 16);
			s_sharedSnapshotTexure.name = "ProbeCameraSnapshot";
			s_sharedSnapshotTexure.hideFlags = HideFlags.HideAndDontSave;
			s_sharedSnapshotTexure.Create();
		}
		return s_sharedSnapshotTexure;
	}

	private void Awake()
	{
		_origLocalRotation = base.transform.localRotation;
		_origParentLocalRotation = base.transform.parent.localRotation;
		_camera = this.GetRequiredComponent<OWCamera>();
		_camera.enabled = false;
		_noiseEffect = GetComponent<NoiseImageEffect>();
		_snapshotTexture = GetSharedSnapshotTexture();
	}

	private void OnDestroy()
	{
		_snapshotTexture = null;
	}

	private void Start()
	{
		AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.QuantumMoon);
		if (astroObject != null)
		{
			_quantumMoon = astroObject.GetComponent<QuantumMoon>();
		}
	}

	public static OWCamera GetLastSnapshotCamera()
	{
		return _lastSnapshotCamera;
	}

	public OWCamera GetOWCamera()
	{
		return _camera;
	}

	public ID GetID()
	{
		return _id;
	}

	public void SetSandLevelController(SandLevelController sandLevelController)
	{
		_sandLevelController = sandLevelController;
	}

	public void RotateHorizontal(float degrees)
	{
		if (_id != ID.Rotating)
		{
			Debug.LogWarning("Tried to rotate a non-rotating ProbeCamera!", this);
			return;
		}
		_cameraRotation.x += degrees;
		base.transform.parent.localRotation = _origParentLocalRotation * Quaternion.AngleAxis(_cameraRotation.x, Vector3.up);
		if (this.OnRotateCamera != null)
		{
			this.OnRotateCamera(_cameraRotation);
		}
	}

	public void RotateVertical(float degrees)
	{
		if (_id != ID.Rotating)
		{
			Debug.LogWarning("Tried to rotate a non-rotating ProbeCamera!", this);
			return;
		}
		_cameraRotation.y = Mathf.Clamp(_cameraRotation.y + degrees, -90f, 0f);
		base.transform.localRotation = _origLocalRotation * Quaternion.AngleAxis(_cameraRotation.y, Vector3.right);
		if (this.OnRotateCamera != null)
		{
			this.OnRotateCamera(_cameraRotation);
		}
	}

	public void ResetRotation()
	{
		if (_id == ID.Rotating)
		{
			_cameraRotation = Vector2.zero;
			base.transform.localRotation = _origLocalRotation;
			base.transform.parent.localRotation = _origParentLocalRotation;
		}
	}

	public bool HasInterference()
	{
		if (_id == ID.PreLaunch)
		{
			return false;
		}
		if (_quantumMoon != null && _quantumMoon.IsPlayerInside() != _quantumMoon.IsProbeInside())
		{
			return true;
		}
		if (_sandLevelController != null && _sandLevelController.IsPointBuried(base.transform.position))
		{
			return true;
		}
		if (Locator.GetCloakFieldController() != null && Locator.GetCloakFieldController().isPlayerInsideCloak != Locator.GetCloakFieldController().isProbeInsideCloak)
		{
			return true;
		}
		return false;
	}

	public RenderTexture TakeSnapshot()
	{
		_lastSnapshotCamera = _camera;
		if (_noiseEffect != null)
		{
			_noiseEffect.enabled = HasInterference();
		}
		_camera.targetTexture = _snapshotTexture;
		_camera.Render();
		return _snapshotTexture;
	}
}
