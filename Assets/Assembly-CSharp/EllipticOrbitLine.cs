using System;
using UnityEngine;

public class EllipticOrbitLine : OrbitLine
{
	private Vector3 _semiMajorAxis;

	private Vector3 _semiMinorAxis;

	private Vector3 _upAxisDir;

	private float _fociDistance;

	private Vector3[] _verts;

	protected override void InitializeLineRenderer()
	{
		GetComponent<LineRenderer>().positionCount = _numVerts;
	}

	protected override void OnValidate()
	{
		if (_numVerts < 0 || _numVerts > 4096)
		{
			_numVerts = Mathf.Clamp(_numVerts, 0, 4096);
		}
		if (GetComponent<LineRenderer>().positionCount != _numVerts)
		{
			InitializeLineRenderer();
		}
	}

	protected override void Start()
	{
		base.Start();
		AstroObject astroObject = ((_astroObject != null) ? _astroObject.GetPrimaryBody() : null);
		InitialMotion initialMotion = ((_astroObject != null) ? _astroObject.GetComponent<InitialMotion>() : null);
		if ((bool)astroObject && (bool)initialMotion)
		{
			Vector3 rhs = _astroObject.transform.position - astroObject.transform.position;
			Vector3 initVelocity = initialMotion.GetInitVelocity();
			Vector3 vector = Vector3.Cross(initVelocity, rhs);
			Vector2 orbitEllipseSemiAxes = initialMotion.GetOrbitEllipseSemiAxes();
			_semiMajorAxis = rhs.normalized * orbitEllipseSemiAxes.x;
			_semiMinorAxis = initVelocity.normalized * orbitEllipseSemiAxes.y;
			_upAxisDir = vector.normalized;
			_fociDistance = Mathf.Sqrt(orbitEllipseSemiAxes.x * orbitEllipseSemiAxes.x - orbitEllipseSemiAxes.y * orbitEllipseSemiAxes.y);
		}
		_verts = new Vector3[_numVerts];
		base.enabled = false;
	}

	protected override void Update()
	{
		AstroObject astroObject = ((_astroObject != null) ? _astroObject.GetPrimaryBody() : null);
		if (astroObject == null)
		{
			base.enabled = false;
			return;
		}
		Vector3 vector = astroObject.transform.position + _semiMajorAxis.normalized * _fociDistance;
		float num = CalcProjectedAngleToCenter(vector, _semiMajorAxis, _semiMinorAxis, _astroObject.transform.position);
		for (int i = 0; i < _numVerts; i++)
		{
			float f = (float)i / (float)(_numVerts - 1) * (float)Math.PI * 2f - (num + (float)Math.PI);
			_verts[i] = _semiMajorAxis * Mathf.Cos(f) + _semiMinorAxis * Mathf.Sin(f);
		}
		_lineRenderer.SetPositions(_verts);
		base.transform.position = vector;
		base.transform.rotation = Quaternion.LookRotation(_semiMinorAxis, _upAxisDir);
		float num2 = DistanceToEllipticalOrbitLine(vector, _semiMajorAxis, _semiMinorAxis, _upAxisDir, Locator.GetActiveCamera().transform.position);
		float widthMultiplier = Mathf.Min(num2 * (_lineWidth / 1000f), _maxLineWidth);
		float num3 = (_fade ? (1f - Mathf.Clamp01((num2 - _fadeStartDist) / (_fadeEndDist - _fadeStartDist))) : 1f);
		_lineRenderer.widthMultiplier = widthMultiplier;
		_lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, num3 * num3);
	}

	private float CalcProjectedAngleToCenter(Vector3 foci, Vector3 semiMajorAxis, Vector3 semiMinorAxis, Vector3 point)
	{
		Vector3 lhs = point - foci;
		Vector3 vector = new Vector3(Vector3.Dot(lhs, semiMajorAxis.normalized), 0f, Vector3.Dot(lhs, semiMinorAxis.normalized));
		vector.x *= semiMinorAxis.magnitude / semiMajorAxis.magnitude;
		return Mathf.Atan2(vector.z, vector.x);
	}

	private float DistanceToEllipticalOrbitLine(Vector3 foci, Vector3 semiMajorAxis, Vector3 semiMinorAxis, Vector3 upAxis, Vector3 point)
	{
		float f = CalcProjectedAngleToCenter(foci, semiMajorAxis, semiMinorAxis, point);
		Vector3 b = foci + _semiMajorAxis * Mathf.Cos(f) + _semiMinorAxis * Mathf.Sin(f);
		return Vector3.Distance(point, b);
	}
}
