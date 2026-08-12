using System.Collections.Generic;
using UnityEngine;

public class OldGrowthForestController : MonoBehaviour
{
	[SerializeField]
	private PlayerCloneController _playerClone;

	[SerializeField]
	private GameObject _treeRoot;

	[SerializeField]
	private GameObject _foliageRoot;

	[SerializeField]
	private Material _quantumMaterial;

	[SerializeField]
	private FluidVolume _airVolume;

	[SerializeField]
	private OWAudioSource _desolateAmbience;

	private List<VisibilityObject> _trees;

	private List<VisibilityObject> _petrifiedTrees;

	private List<VisibilityObject> _visitedTrees;

	private List<VisibilityObject> _foliage;

	private float _forestIsDarkTime;

	private bool _hidePetrifiedTrees;

	private int _visitedTreeCount;

	private void Awake()
	{
		_trees = new List<VisibilityObject>();
		_petrifiedTrees = new List<VisibilityObject>();
		_foliage = new List<VisibilityObject>();
		_visitedTrees = new List<VisibilityObject>();
		GlobalMessenger<EyeState>.AddListener("EyeStateChanged", OnEyeStateChanged);
	}

	private void Start()
	{
		base.enabled = false;
		VisibilityObject[] componentsInChildren = _treeRoot.GetComponentsInChildren<VisibilityObject>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			_trees.Add(componentsInChildren[i]);
			componentsInChildren[i].SetActivation(active: false);
		}
		VisibilityObject[] componentsInChildren2 = _foliageRoot.GetComponentsInChildren<VisibilityObject>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			_foliage.Add(componentsInChildren2[j]);
			componentsInChildren2[j].SetActivation(active: false);
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger<EyeState>.RemoveListener("EyeStateChanged", OnEyeStateChanged);
	}

	private void OnEyeStateChanged(EyeState state)
	{
		if (state == EyeState.ForestIsDark)
		{
			base.enabled = true;
			_forestIsDarkTime = Time.time;
			_airVolume.SetVolumeActivation(active: false);
			for (int i = 0; i < _trees.Count; i++)
			{
				_trees[i].SetActivation(active: true);
			}
			for (int j = 0; j < _foliage.Count; j++)
			{
				_foliage[j].SetActivation(active: true);
			}
		}
	}

	private void Update()
	{
		if (Time.time < _forestIsDarkTime + 1f || (Locator.GetProbe() != null && Locator.GetProbe().IsLaunched() && !Locator.GetProbe().IsAnchored()))
		{
			return;
		}
		for (int num = _foliage.Count - 1; num >= 0; num--)
		{
			if (!_foliage[num].IsVisible() || !CheckIllumination(_foliage[num].transform.position))
			{
				_foliage[num].gameObject.SetActive(value: false);
				_foliage.RemoveAt(num);
			}
		}
		for (int num2 = _visitedTrees.Count - 1; num2 >= 0; num2--)
		{
			if (!_visitedTrees[num2].IsVisible() || !CheckIllumination(_visitedTrees[num2].transform.position))
			{
				_visitedTrees[num2].gameObject.SetActive(value: false);
				_visitedTrees.RemoveAt(num2);
			}
		}
		for (int num3 = _petrifiedTrees.Count - 1; num3 >= 0; num3--)
		{
			if (_hidePetrifiedTrees)
			{
				if (!_petrifiedTrees[num3].IsVisible() || !CheckIllumination(_petrifiedTrees[num3].transform.position))
				{
					_petrifiedTrees[num3].gameObject.SetActive(value: false);
					_petrifiedTrees.RemoveAt(num3);
				}
			}
			else
			{
				if (!PlayerState.InZeroG() && _petrifiedTrees[num3].IsVisible())
				{
					Vector3 from = _petrifiedTrees[num3].transform.position - Locator.GetPlayerTransform().position;
					from.y = 0f;
					if (Locator.GetFlashlight().IsFlashlightOn() && from.magnitude < 30f && Vector3.Angle(from, Locator.GetPlayerTransform().forward) < 30f)
					{
						_visitedTreeCount++;
						Debug.Log("VISITED TREE " + _visitedTreeCount, _petrifiedTrees[num3]);
						_visitedTrees.Add(_petrifiedTrees[num3]);
						_petrifiedTrees.RemoveAt(num3);
						if (_visitedTreeCount == 3)
						{
							_hidePetrifiedTrees = true;
							MonoBehaviour.print("HIDE PETRIFIED TREES");
						}
					}
				}
				if (Time.time > _forestIsDarkTime + 20f)
				{
					MonoBehaviour.print("OUT OF TIME");
					_hidePetrifiedTrees = true;
				}
			}
		}
		for (int num4 = _trees.Count - 1; num4 >= 0; num4--)
		{
			if (!_trees[num4].IsVisible() || !CheckIllumination(_trees[num4].transform.position))
			{
				Petrify(_trees[num4].gameObject, _quantumMaterial);
				_petrifiedTrees.Add(_trees[num4]);
				_trees.RemoveAt(num4);
			}
		}
		if (Locator.GetPlayerController().IsGrounded() && Locator.GetPlayerController().GetRelativeGroundVelocity().magnitude < 10f && _trees.Count == 0 && _petrifiedTrees.Count == 0 && _foliage.Count == 0 && _visitedTrees.Count == 0)
		{
			MonoBehaviour.print("ACTIVATE CLONE");
			_playerClone.ActivateClone();
			base.enabled = false;
		}
	}

	private bool CheckIllumination(Vector3 worldPosition)
	{
		if (Locator.GetFlashlight().IsFlashlightOn())
		{
			Vector3 vector = Locator.GetPlayerTransform().position - worldPosition;
			vector.y = 0f;
			if (vector.magnitude < 50f)
			{
				return true;
			}
		}
		if (Locator.GetProbe() != null && Locator.GetProbe().IsAnchored())
		{
			Vector3 vector2 = Locator.GetProbe().transform.position - worldPosition;
			vector2.y = 0f;
			if (vector2.magnitude < 50f)
			{
				return true;
			}
		}
		return false;
	}

	private void Petrify(GameObject treeObj, Material quantumRockMaterial)
	{
		MeshRenderer[] componentsInChildren = treeObj.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].gameObject.name.Contains("Trunk"))
			{
				componentsInChildren[i].sharedMaterial = quantumRockMaterial;
			}
			else
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}
}
