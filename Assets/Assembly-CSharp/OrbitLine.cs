using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitLine : MonoBehaviour
{
	protected LineRenderer _lineRenderer;

	[SerializeField]
	protected AstroObject _astroObject;

	[Space]
	[SerializeField]
	protected int _numVerts = 128;

	[SerializeField]
	protected float _arcDegrees = 360f;

	[SerializeField]
	protected Color _color = Color.white;

	[SerializeField]
	protected float _lineWidth = 10f;

	[SerializeField]
	protected float _maxLineWidth = 100f;

	[Space]
	[SerializeField]
	protected bool _fade;

	[SerializeField]
	protected float _fadeStartDist = 10000f;

	[SerializeField]
	protected float _fadeEndDist = 20000f;

	protected virtual void InitializeLineRenderer()
	{
		LineRenderer component = GetComponent<LineRenderer>();
		Vector3[] array = new Vector3[_numVerts];
		for (int i = 0; i < _numVerts; i++)
		{
			float f = (float)i / (float)(_numVerts - 1) * _arcDegrees * ((float)Math.PI / 180f);
			array[i] = new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
		}
		component.positionCount = _numVerts;
		component.SetPositions(array);
	}

	protected virtual void Reset()
	{
		InitializeLineRenderer();
	}

	protected virtual void OnValidate()
	{
		if (_numVerts < 0 || _numVerts > 1024)
		{
			_numVerts = Mathf.Clamp(_numVerts, 0, 1024);
		}
		if (GetComponent<LineRenderer>().positionCount != _numVerts)
		{
			InitializeLineRenderer();
		}
	}

	protected virtual void Awake()
	{
		_lineRenderer = GetComponent<LineRenderer>();
		_lineRenderer.startColor = _color;
		_lineRenderer.endColor = new Color(_color.r, _color.g, _color.b, 0f);
		_lineRenderer.startWidth = _lineWidth;
		_lineRenderer.endWidth = _lineWidth;
	}

	protected virtual void Start()
	{
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		base.enabled = false;
	}

	protected virtual void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
	}

	private void OnEnable()
	{
		_lineRenderer.enabled = true;
	}

	private void OnDisable()
	{
		_lineRenderer.enabled = false;
	}

	protected virtual void OnEnterMapView()
	{
		base.enabled = true;
	}

	protected virtual void OnExitMapView()
	{
		base.enabled = false;
	}

	protected virtual void Update()
	{
		AstroObject astroObject = ((_astroObject != null) ? _astroObject.GetPrimaryBody() : null);
		if (astroObject == null)
		{
			base.enabled = false;
			return;
		}
		Vector3 vector = _astroObject.transform.position - astroObject.transform.position;
		Vector3 normalized = Vector3.Cross(astroObject.GetAttachedOWRigidbody().GetRelativeVelocity(_astroObject.GetAttachedOWRigidbody()), vector).normalized;
		float magnitude = vector.magnitude;
		base.transform.position = astroObject.transform.position;
		base.transform.rotation = Quaternion.LookRotation(vector, normalized);
		base.transform.localScale = Vector3.one * magnitude;
		float num = DistanceToOrbitLine(astroObject.transform.position, normalized, magnitude, Locator.GetActiveCamera().transform.position);
		float widthMultiplier = Mathf.Min(num * (_lineWidth / 1000f), _maxLineWidth);
		float num2 = CalcFade(num);
		_lineRenderer.widthMultiplier = widthMultiplier;
		_lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, _color.a * num2 * num2);
	}

	protected virtual float CalcFade(float distance)
	{
		if (!_fade)
		{
			return 1f;
		}
		return 1f - Mathf.Clamp01((distance - _fadeStartDist) / (_fadeEndDist - _fadeStartDist));
	}

	private float DistanceToOrbitLine(Vector3 orbitCenter, Vector3 orbitUp, float radius, Vector3 point)
	{
		Vector3 b = orbitCenter + Vector3.ProjectOnPlane(point - orbitCenter, orbitUp).normalized * radius;
		return Vector3.Distance(point, b);
	}
}
