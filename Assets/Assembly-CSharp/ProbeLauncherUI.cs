using UnityEngine;
using UnityEngine.UI;

public class ProbeLauncherUI : MonoBehaviour
{
	private ProbeLauncher _probeLauncher;

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private Image _image;

	[SerializeField]
	private Image _bracketImage;

	[SerializeField]
	private Texture2D _frontSnapshotOverlay;

	[SerializeField]
	private Texture2D _rearSnapshotOverlay;

	[SerializeField]
	private Texture2D _obscuredCameraOverlay;

	[SerializeField]
	private bool _nonSuitUI;

	private Material _material;

	private float _snapshotTime;

	private static ScreenPrompt s_takeSnapshotPrompt;

	private static ScreenPrompt s_removeSnapshopPrompt;

	private void Start()
	{
		if (s_removeSnapshopPrompt == null)
		{
			s_takeSnapshotPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.ProbeSnapshotPrompt));
			s_removeSnapshopPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.ProbeRemoveSnapshotPrompt));
			Locator.GetPromptManager().AddScreenPrompt(s_takeSnapshotPrompt, PromptPosition.Center);
			Locator.GetPromptManager().AddScreenPrompt(s_removeSnapshopPrompt, PromptPosition.UpperRight);
		}
		GlobalMessenger.AddListener("SuitUp", DeactivateUI);
		GlobalMessenger.AddListener("RemoveSuit", DeactivateUI);
		HideProbeHUD();
		_material = Object.Instantiate(_image.material);
		_image.material = _material;
		if (_bracketImage != null)
		{
			_bracketImage.enabled = false;
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("SuitUp", DeactivateUI);
		GlobalMessenger.RemoveListener("RemoveSuit", DeactivateUI);
		if (_material != null)
		{
			Object.Destroy(_material);
		}
		_material = null;
		if (_probeLauncher != null)
		{
			_probeLauncher.OnTakeSnapshot -= OnTakeSnapshot;
			_probeLauncher.OnRetrieveMyProbe -= OnRetrieveProbe;
			_probeLauncher = null;
		}
	}

	public void SetProbeLauncher(ProbeLauncher probeLauncher)
	{
		if (_probeLauncher != null)
		{
			_probeLauncher.OnTakeSnapshot -= OnTakeSnapshot;
			_probeLauncher.OnRetrieveMyProbe -= OnRetrieveProbe;
		}
		_probeLauncher = probeLauncher;
		if (_probeLauncher != null)
		{
			_probeLauncher.OnTakeSnapshot += OnTakeSnapshot;
			_probeLauncher.OnRetrieveMyProbe += OnRetrieveProbe;
		}
	}

	public void ActivateUI()
	{
		if (_canvas != null)
		{
			_canvas.enabled = true;
		}
	}

	public void DeactivateUI()
	{
		HideProbeHUD();
	}

	private void OnRetrieveProbe()
	{
		HideProbeHUD();
	}

	private void HideProbeHUD()
	{
		if (_canvas != null)
		{
			_canvas.enabled = false;
		}
		_image.enabled = false;
		s_removeSnapshopPrompt.SetVisibility(isVisible: false);
		GlobalMessenger.FireEvent("Probe Snapshot Removed");
	}

	private void OnTakeSnapshot(RenderTexture snapshot, ProbeCamera camera, float secondsAnchored)
	{
		s_takeSnapshotPrompt.SetVisibility(isVisible: false);
		if (_probeLauncher.GetName() == ProbeLauncher.Name.Player)
		{
			if (_nonSuitUI)
			{
				if (Locator.GetPlayerSuit().IsWearingSuit())
				{
					return;
				}
				s_removeSnapshopPrompt.SetVisibility(isVisible: true);
				ActivateUI();
			}
			else if (!Locator.GetPlayerSuit().IsWearingSuit())
			{
				return;
			}
		}
		_snapshotTime = Time.time;
		if (_canvas != null)
		{
			_canvas.enabled = true;
		}
		_image.enabled = true;
		if (camera.GetID() == ProbeCamera.ID.Reverse)
		{
			_image.material.SetTexture("_MainTex", _rearSnapshotOverlay);
		}
		else
		{
			_image.material.SetTexture("_MainTex", _frontSnapshotOverlay);
		}
		_image.material.SetTexture("_MainTex", snapshot);
		_image.SetMaterialDirty();
	}

	private void Update()
	{
		if ((Locator.GetPlayerSuit().IsWearingSuit() && !_nonSuitUI) || (!Locator.GetPlayerSuit().IsWearingSuit() && _nonSuitUI))
		{
			if (_probeLauncher.InPhotoMode())
			{
				if (_bracketImage != null && !_bracketImage.enabled)
				{
					_bracketImage.enabled = true;
				}
				if (!Locator.GetPlayerSuit().IsWearingSuit(includeTrainingSuit: false) && !s_takeSnapshotPrompt.IsVisible())
				{
					s_takeSnapshotPrompt.SetVisibility(!PlayerData.GetPersistentCondition("HAS_TAKEN_HANDHELD_SNAPSHOT"));
				}
			}
			else
			{
				if (_bracketImage != null && _bracketImage.enabled)
				{
					_bracketImage.enabled = false;
				}
				if (s_takeSnapshotPrompt.IsVisible())
				{
					s_takeSnapshotPrompt.SetVisibility(isVisible: false);
				}
			}
		}
		if (_nonSuitUI && s_removeSnapshopPrompt.IsVisible() && OWInput.IsNewlyPressed(InputLibrary.cancel))
		{
			DeactivateUI();
		}
	}
}
