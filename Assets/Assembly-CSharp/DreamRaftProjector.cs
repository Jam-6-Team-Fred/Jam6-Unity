using System.Collections.Generic;
using UnityEngine;

public class DreamRaftProjector : DreamObjectProjector
{
	private static List<DreamRaftProjector> s_dreamRaftProjectors = new List<DreamRaftProjector>(8);

	[Space]
	[SerializeField]
	private Transform _raftSpawn;

	[SerializeField]
	private SphereBounds _visibilityBounds = new SphereBounds(Vector3.zero, 0.5f);

	private DreamRaftProjection _dreamRaftProjection;

	protected override void Awake()
	{
		s_dreamRaftProjectors.Add(this);
		base.Awake();
		_dreamRaftProjection = _projections[0] as DreamRaftProjection;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		s_dreamRaftProjectors.Remove(this);
	}

	public void ExtinguishImmediately()
	{
		if (_lit)
		{
			for (int i = 0; i < _projections.Length; i++)
			{
				_projections[i].SetVisibleImmediate(visible: false, forceUpdate: true);
			}
			_lit = false;
			_unlitTime = Time.time;
			_interactReceiver.DisableInteraction();
			_flameController.FadeTo(0f, 1f);
			SyncProjectorState();
		}
	}

	private void SyncProjectorState()
	{
		for (int i = 0; i < s_dreamRaftProjectors.Count; i++)
		{
			if (!(s_dreamRaftProjectors[i] == this))
			{
				s_dreamRaftProjectors[i]._lit = _lit;
				s_dreamRaftProjectors[i]._litTime = _litTime;
				s_dreamRaftProjectors[i]._unlitTime = _unlitTime;
				if (!_lit)
				{
					s_dreamRaftProjectors[i]._interactReceiver.DisableInteraction();
					s_dreamRaftProjectors[i]._flameController.SetIntensity(0f);
				}
			}
		}
	}

	private void SpawnRaft()
	{
		for (int i = 0; i < _projections.Length; i++)
		{
			OWRigidbody attachedOWRigidbody = _projections[i].GetAttachedOWRigidbody();
			attachedOWRigidbody.SetPosition(_raftSpawn.position);
			attachedOWRigidbody.SetRotation(_raftSpawn.rotation);
		}
		Locator.GetDreamWorldController().RegisterLastUsedRaftProjector(this);
	}

	private void RespawnRaft()
	{
		for (int i = 0; i < _projections.Length; i++)
		{
			_projections[i].SetVisibleImmediate(visible: false, forceUpdate: true);
		}
		_lit = false;
		SetLit(lit: true);
	}

	private bool CheckRaftVisibleInWindow()
	{
		if (!_lit)
		{
			return false;
		}
		SphereBounds other = _dreamRaftProjection.CalcWorldVisibilityBounds();
		return new SphereBounds(base.transform.TransformPoint(_visibilityBounds.center), _visibilityBounds.radius).Overlaps(other, Locator.GetPlayerCamera().transform.position);
	}

	public override void SetLit(bool lit)
	{
		if (_lit != lit)
		{
			base.SetLit(lit);
			if (lit)
			{
				SpawnRaft();
			}
			SyncProjectorState();
		}
	}

	protected override void FixedUpdate()
	{
		if (_lightSensor.IsIlluminated())
		{
			if (!_lit)
			{
				SetLit(lit: true);
			}
			else if (!CheckRaftVisibleInWindow())
			{
				RespawnRaft();
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		bool flag = CheckRaftVisibleInWindow();
		_interactReceiver.SetInteractionEnabled(flag && _lit && Time.time > _litTime + 1f && !Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController()
			.IsFocused(0.1f));
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(_visibilityBounds.center, _visibilityBounds.radius);
		}
	}
}
