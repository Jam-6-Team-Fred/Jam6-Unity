using UnityEngine;

public class BridgeCollapseController : MonoBehaviour
{
	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private GameObject[] _intactObjectRoots = new GameObject[0];

	[SerializeField]
	private GameObject[] _destroyedObjectRoots = new GameObject[0];

	[SerializeField]
	private OWRigidbody[] _fragments = new OWRigidbody[0];

	[SerializeField]
	private float _delay;

	private OWRigidbody _ringworldBody;

	private bool _collapsed;

	private float _collapseTime;

	private void Awake()
	{
		_ringworldBody = this.GetAttachedOWRigidbody();
		for (int i = 0; i < _destroyedObjectRoots.Length; i++)
		{
			_destroyedObjectRoots[i].SetActive(value: false);
		}
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
		}
	}

	public void StartCollapse()
	{
		if (_collapsed)
		{
			return;
		}
		SurveyorProbe probe = Locator.GetProbe();
		if (probe != null && probe.IsAnchored())
		{
			for (int i = 0; i < _intactObjectRoots.Length; i++)
			{
				if (probe.transform.IsChildOf(_intactObjectRoots[i].transform))
				{
					probe.Unanchor();
					break;
				}
			}
		}
		for (int j = 0; j < _intactObjectRoots.Length; j++)
		{
			_intactObjectRoots[j].SetActive(value: false);
		}
		for (int k = 0; k < _destroyedObjectRoots.Length; k++)
		{
			_destroyedObjectRoots[k].SetActive(value: true);
		}
		for (int l = 0; l < _fragments.Length; l++)
		{
			_fragments[l].SetVelocity(_ringworldBody.GetPointVelocity(_fragments[l].GetPosition()));
			_fragments[l].SetAngularVelocity(_ringworldBody.GetAngularVelocity());
		}
	}

	private void OnFloodImpact()
	{
		if (_delay > 0f)
		{
			base.enabled = true;
			_collapseTime = Time.time + _delay;
		}
		else
		{
			StartCollapse();
		}
	}

	private void FixedUpdate()
	{
		if (Time.time >= _collapseTime)
		{
			base.enabled = false;
			StartCollapse();
		}
	}
}
