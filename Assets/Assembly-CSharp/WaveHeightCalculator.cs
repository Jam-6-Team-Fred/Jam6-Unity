using System;
using UnityEngine;

public class WaveHeightCalculator : MonoBehaviour
{
	[SerializeField]
	private Transform _oceanTransform;

	[SerializeField]
	private Material _oceanMaterial;

	private static float s_fixedTime;

	private Vector4 _uvAngles;

	private Texture2D _heightmapMain;

	private Vector4 _waveScaleMain;

	private Vector4 _waveMovementMain;

	private Texture2D _heightmapMacro;

	private Vector4 _waveScaleMacro;

	private Vector4 _waveMovementMacro;

	private void Awake()
	{
		_uvAngles = _oceanMaterial.GetVector("_UVAngles");
		_heightmapMain = _oceanMaterial.GetTexture("_HeightmapMain") as Texture2D;
		_waveScaleMain = _oceanMaterial.GetVector("_WaveScaleMain");
		_waveMovementMain = _oceanMaterial.GetVector("_WaveMovementMain");
		_heightmapMacro = _oceanMaterial.GetTexture("_HeightmapMacro") as Texture2D;
		_waveScaleMacro = _oceanMaterial.GetVector("_WaveScaleMacro");
		_waveMovementMacro = _oceanMaterial.GetVector("_WaveMovementMacro");
	}

	private Vector4 CalculateUVs(Vector3 spherePos)
	{
		Vector4 vector = default(Vector4);
		vector.x = Mathf.Atan2(spherePos.z, spherePos.x);
		vector.y = Mathf.Asin(spherePos.y);
		vector.z = spherePos.x;
		vector.w = spherePos.z;
		vector /= 1.570796f;
		return vector * 0.5f + new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
	}

	private Vector4 CalculateBlendWeights(float angle)
	{
		Vector4 result = default(Vector4);
		result.x = Mathf.Clamp01((angle - (_uvAngles.x - _uvAngles.y * 0.5f)) / _uvAngles.y);
		result.w = Mathf.Clamp01((0f - angle - (_uvAngles.x - _uvAngles.y * 0.5f)) / _uvAngles.y);
		result.y = Mathf.Clamp01((angle + _uvAngles.z * 0.5f) / _uvAngles.z) * (1f - result.x);
		result.z = Mathf.Clamp01((0f - angle + _uvAngles.z * 0.5f) / _uvAngles.z) * (1f - result.w);
		return result;
	}

	private Vector4 Frac(Vector4 x)
	{
		x.x -= Mathf.Floor(x.x);
		x.y -= Mathf.Floor(x.y);
		x.z -= Mathf.Floor(x.z);
		x.w -= Mathf.Floor(x.w);
		return x;
	}

	private Vector4 SmoothstepFast(Vector4 x)
	{
		x.x = x.x * x.x * (3f - 2f * x.x);
		x.y = x.y * x.y * (3f - 2f * x.y);
		x.z = x.z * x.z * (3f - 2f * x.z);
		x.w = x.w * x.w * (3f - 2f * x.w);
		return x;
	}

	private Vector4 CalculatePhase(float timeScale, float offset, float smoothFactor)
	{
		Vector4 vector = new Vector4(s_fixedTime, s_fixedTime, s_fixedTime, s_fixedTime) * timeScale + new Vector4(0f, 1f, 2f, 3f) + new Vector4(offset, offset, offset, offset);
		Vector4 vector2 = Frac(vector / 4f) * 4f - new Vector4(2f, 2f, 2f, 2f);
		vector2.x = 1f - Mathf.Clamp01(Mathf.Abs(vector2.x));
		vector2.y = 1f - Mathf.Clamp01(Mathf.Abs(vector2.y));
		vector2.z = 1f - Mathf.Clamp01(Mathf.Abs(vector2.z));
		vector2.w = 1f - Mathf.Clamp01(Mathf.Abs(vector2.w));
		return Vector4.LerpUnclamped(vector2, SmoothstepFast(vector2), smoothFactor);
	}

	private Vector2 RotateUVs(Vector2 uv, float r)
	{
		float num = Mathf.Sin(r * (float)Math.PI);
		float num2 = Mathf.Cos(r * (float)Math.PI);
		uv -= new Vector2(0.5f, 0.5f);
		return new Vector2(uv.x * num2 - uv.y * num, uv.x * num + uv.y * num2) + new Vector2(0.5f, 0.5f);
	}

	private float GetHeight(Vector2 coord, Texture2D heightmap, Vector4 waveScale, Vector4 waveMovement)
	{
		Vector4 b = CalculatePhase(waveScale.z, Mathf.Sin(coord.x * waveMovement.z * (float)Math.PI * 2f), waveMovement.w);
		coord = coord / waveScale.x + new Vector2(waveMovement.x, waveMovement.y) * s_fixedTime;
		return (Vector4.Dot(heightmap.GetPixelBilinear(coord.x, coord.y), b) - 0.5f) * waveScale.y;
	}

	private float GetHeightRotational(Vector2 coord, Texture2D heightmap, Vector4 waveScale, Vector4 waveMovement)
	{
		Vector4 b = CalculatePhase(waveScale.z, Mathf.Sin(coord.x * waveMovement.z * (float)Math.PI * 2f), waveMovement.w);
		coord = RotateUVs(coord, Time.fixedTime * waveMovement.x * waveScale.x) / waveScale.x;
		return (Vector4.Dot(heightmap.GetPixelBilinear(coord.x, coord.y), b) - 0.5f) * waveScale.y;
	}

	private float GetCombinedHeight(Vector2 coord, float dir)
	{
		float height = GetHeight(coord, _heightmapMain, _waveScaleMain, Vector4.Scale(_waveMovementMain, new Vector4(dir, dir, 1f, 1f)));
		float height2 = GetHeight(coord, _heightmapMacro, _waveScaleMacro, Vector4.Scale(_waveMovementMacro, new Vector4(dir, dir, 1f, 1f)));
		return height + height2;
	}

	private float GetCombinedHeightRotational(Vector2 coord, float dir)
	{
		float heightRotational = GetHeightRotational(coord, _heightmapMain, _waveScaleMain, Vector4.Scale(_waveMovementMain, new Vector4(dir, dir, 1f, 1f)));
		float heightRotational2 = GetHeightRotational(coord, _heightmapMacro, _waveScaleMacro, Vector4.Scale(_waveMovementMacro, new Vector4(dir, dir, 1f, 1f)));
		return heightRotational + heightRotational2;
	}

	public Vector3 GetWavePosition(Vector3 worldPos)
	{
		s_fixedTime = Time.fixedTime;
		Vector3 normalized = _oceanTransform.InverseTransformPoint(worldPos).normalized;
		Vector4 vector = CalculateUVs(normalized);
		Vector4 vector2 = CalculateBlendWeights(vector.y * 2f - 1f);
		float num = 0f;
		num += GetCombinedHeightRotational(new Vector2(vector.z, vector.w), 1f) * vector2.x;
		num += GetCombinedHeight(new Vector2(vector.x, vector.y), 1f) * vector2.y;
		num += GetCombinedHeight(new Vector2(vector.x, vector.y), -1f) * vector2.z;
		num += GetCombinedHeightRotational(new Vector2(vector.z, vector.w), -1f) * vector2.w;
		Vector3 position = normalized + normalized * num;
		return _oceanTransform.TransformPoint(position);
	}

	public float GetWaveHeight(Vector3 worldPos)
	{
		s_fixedTime = Time.fixedTime;
		Vector3 normalized = _oceanTransform.InverseTransformPoint(worldPos).normalized;
		Vector4 vector = CalculateUVs(normalized);
		Vector4 vector2 = CalculateBlendWeights(vector.y * 2f - 1f);
		return (0f + GetCombinedHeightRotational(new Vector2(vector.z, vector.w), 1f) * vector2.x + GetCombinedHeight(new Vector2(vector.x, vector.y), 1f) * vector2.y + GetCombinedHeight(new Vector2(vector.x, vector.y), -1f) * vector2.z + GetCombinedHeightRotational(new Vector2(vector.z, vector.w), -1f) * vector2.w) * Mathf.Max(Mathf.Max(_oceanTransform.lossyScale.x, _oceanTransform.lossyScale.y), _oceanTransform.lossyScale.z);
	}

	public bool IsPointBelowWaves(Vector3 worldPos)
	{
		s_fixedTime = Time.fixedTime;
		Vector3 vector = _oceanTransform.InverseTransformPoint(worldPos);
		Vector3 normalized = vector.normalized;
		Vector4 vector2 = CalculateUVs(normalized);
		Vector4 vector3 = CalculateBlendWeights(vector2.y * 2f - 1f);
		float num = 0f;
		num += GetCombinedHeightRotational(new Vector2(vector2.z, vector2.w), 1f) * vector3.x;
		num += GetCombinedHeight(new Vector2(vector2.x, vector2.y), 1f) * vector3.y;
		num += GetCombinedHeight(new Vector2(vector2.x, vector2.y), -1f) * vector3.z;
		num += GetCombinedHeightRotational(new Vector2(vector2.z, vector2.w), -1f) * vector3.w;
		Vector3 vector4 = normalized + normalized * num;
		return vector.sqrMagnitude < vector4.sqrMagnitude;
	}

	public float GetOceanRadius(Vector3 worldPos)
	{
		s_fixedTime = Time.fixedTime;
		Vector3 normalized = _oceanTransform.InverseTransformPoint(worldPos).normalized;
		Vector4 vector = CalculateUVs(normalized);
		Vector4 vector2 = CalculateBlendWeights(vector.y * 2f - 1f);
		float num = 0f;
		num += GetCombinedHeightRotational(new Vector2(vector.z, vector.w), 1f) * vector2.x;
		num += GetCombinedHeight(new Vector2(vector.x, vector.y), 1f) * vector2.y;
		num += GetCombinedHeight(new Vector2(vector.x, vector.y), -1f) * vector2.z;
		num += GetCombinedHeightRotational(new Vector2(vector.z, vector.w), -1f) * vector2.w;
		Vector3 position = normalized + normalized * num;
		return Vector3.Distance(_oceanTransform.TransformPoint(position), _oceanTransform.position);
	}
}
