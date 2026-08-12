using UnityEngine;

public class SarcophagusRingworldController : MonoBehaviour
{
	[SerializeField]
	private Transform _leftDoor;

	[SerializeField]
	private Transform _rightDoor;

	[SerializeField]
	private float _doorOpenDistance = 0.5f;

	[Space]
	[SerializeField]
	private OWRenderer[] _firstSealLockedVisuals = new OWRenderer[0];

	[SerializeField]
	private OWLight2[] _firstSealLockedLights = new OWLight2[0];

	[SerializeField]
	private OWRenderer[] _firstSealUnlockedVisuals = new OWRenderer[0];

	[SerializeField]
	private OWRenderer[] _secondSealLockedVisuals = new OWRenderer[0];

	[SerializeField]
	private OWLight2[] _secondSealLockedLights = new OWLight2[0];

	[SerializeField]
	private OWRenderer[] _secondSealUnlockedVisuals = new OWRenderer[0];

	[SerializeField]
	private OWRenderer[] _thirdSealLockedVisuals = new OWRenderer[0];

	[SerializeField]
	private OWLight2[] _thirdSealLockedLights = new OWLight2[0];

	[SerializeField]
	private OWRenderer[] _thirdSealUnlockedVisuals = new OWRenderer[0];

	[Space]
	[SerializeField]
	private OWRenderer[] _lanternFlameVisuals = new OWRenderer[0];

	[SerializeField]
	private OWLight2[] _lanternFlameLights = new OWLight2[0];

	private DreamObjectProjector _firstSealProjector;

	private DreamObjectProjector _secondSealProjector;

	private DreamObjectProjector _thirdSealProjector;

	private PrisonerDirector _prisonerDirector;

	private bool _firstSealExtinguished;

	private bool _secondSealExtinguished;

	private bool _thirdSealExtinguised;

	private void Start()
	{
		DreamWorldController dreamWorldController = Locator.GetDreamWorldController();
		if (dreamWorldController != null)
		{
			_firstSealProjector = dreamWorldController.GetSarcophagusController().firstSealProjector;
			_secondSealProjector = dreamWorldController.GetSarcophagusController().secondSealProjector;
			_thirdSealProjector = dreamWorldController.GetSarcophagusController().thirdSealProjector;
			_prisonerDirector = dreamWorldController.GetPrisonerDirector();
			_firstSealProjector.OnProjectorExtinguished += new OWEvent.OWCallback(OnFirstSealExtinguish);
			_secondSealProjector.OnProjectorExtinguished += new OWEvent.OWCallback(OnSecondSealExtinguish);
			_thirdSealProjector.OnProjectorExtinguished += new OWEvent.OWCallback(OnThirdSealExtinguish);
			_prisonerDirector.OnPrisonerDeparted += new OWEvent.OWCallback(OnPrisonerDeparted);
		}
	}

	private void OnDestroy()
	{
		if (_firstSealProjector != null)
		{
			_firstSealProjector.OnProjectorExtinguished -= new OWEvent.OWCallback(OnFirstSealExtinguish);
		}
		if (_secondSealProjector != null)
		{
			_secondSealProjector.OnProjectorExtinguished -= new OWEvent.OWCallback(OnSecondSealExtinguish);
		}
		if (_thirdSealProjector != null)
		{
			_thirdSealProjector.OnProjectorExtinguished -= new OWEvent.OWCallback(OnThirdSealExtinguish);
		}
		if (_prisonerDirector != null)
		{
			_prisonerDirector.OnPrisonerDeparted -= new OWEvent.OWCallback(OnPrisonerDeparted);
		}
	}

	private void UnlockSeal(OWRenderer[] lockedVisuals, OWLight2[] lockedLights, OWRenderer[] unlockedVisuals)
	{
		for (int i = 0; i < lockedVisuals.Length; i++)
		{
			lockedVisuals[i].SetActivation(active: false);
		}
		for (int j = 0; j < lockedLights.Length; j++)
		{
			lockedLights[j].SetActivation(active: false);
		}
		for (int k = 0; k < unlockedVisuals.Length; k++)
		{
			unlockedVisuals[k].SetActivation(active: true);
		}
		if (_firstSealExtinguished && _secondSealExtinguished && _thirdSealExtinguised)
		{
			_leftDoor.localPosition = new Vector3(0f, 0f, 0f - _doorOpenDistance);
			_rightDoor.localPosition = new Vector3(0f, 0f, _doorOpenDistance);
		}
	}

	private void OnFirstSealExtinguish()
	{
		_firstSealExtinguished = true;
		UnlockSeal(_firstSealLockedVisuals, _firstSealLockedLights, _firstSealUnlockedVisuals);
	}

	private void OnSecondSealExtinguish()
	{
		_secondSealExtinguished = true;
		UnlockSeal(_secondSealLockedVisuals, _secondSealLockedLights, _secondSealUnlockedVisuals);
	}

	private void OnThirdSealExtinguish()
	{
		_thirdSealExtinguised = true;
		UnlockSeal(_thirdSealLockedVisuals, _thirdSealLockedLights, _thirdSealUnlockedVisuals);
	}

	private void OnPrisonerDeparted()
	{
		for (int i = 0; i < _lanternFlameVisuals.Length; i++)
		{
			_lanternFlameVisuals[i].SetActivation(active: false);
		}
		for (int j = 0; j < _lanternFlameLights.Length; j++)
		{
			_lanternFlameLights[j].SetActivation(active: false);
		}
	}
}
