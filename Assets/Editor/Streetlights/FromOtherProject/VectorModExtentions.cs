using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorModExtentions
{
    #region QuickVector
    /// <summary> Makes a Vector2 of (f, 0). </summary>
    public static Vector2 X2(this float f) { Vector2 result; result.x = f; result.y = 0f; return result; }
    /// <summary> Makes a Vector2 of (0, f). </summary>
    public static Vector2 Y2(this float f) { Vector2 result; result.x = 0f; result.y = f; return result; }

    /// <summary> Makes a Vector3 of (f, 0, 0). </summary>
    public static Vector3 X(this float f) { Vector3 result; result.x = f; result.y = 0f; result.z = 0f; return result; }
    /// <summary> Makes a Vector3 of (0, f, 0). </summary>
    public static Vector3 Y(this float f) { Vector3 result; result.x = 0f; result.y = f; result.z = 0f; return result; }
    /// <summary> Makes a Vector3 of (0, 0, f). </summary>
    public static Vector3 Z(this float f) { Vector3 result; result.x = 0f; result.y = 0f; result.z = f; return result; }
    /// <summary> Makes a Vector4 of (0, 0, 0, f). </summary>
    public static Vector4 W(this float f) { Vector4 result; result.x = 0f; result.y = 0f; result.z = 0f; result.w = f; return result; }

    /// <summary> Fills a Vector3 with this float. </summary>
    public static Vector3 XYZ(this float f) { Vector3 result; result.x = f; result.y = f; result.z = f; return result; }
    
    //public static Vector3 XYZ(this float f) => new Vector3(f, f, f);

    #endregion

    #region Conversions
    /// <summary> Converts a Vector2 to Vector3 with y put into z, y left as 0. </summary>
    public static Vector3 X0Y(this Vector2 v) => new Vector3(v.x, 0f, v.y);

    public static Vector3 ToV3XY(this Vector2Int v2i) => new Vector3(v2i.x, v2i.y);
    public static Vector3 ToV3XZ(this Vector2Int v2i) => new Vector3(v2i.x, 0f, v2i.y);


    public static Vector2 XY(this Vector3 v) => new Vector2(v.x, v.y);
    public static Vector2 YX(this Vector3 v) => new Vector2(v.y, v.x);

    public static Vector2 YZ(this Vector3 v) => new Vector2(v.y, v.z);
    public static Vector2 ZY(this Vector3 v) => new Vector2(v.z, v.y);

    public static Vector2 XZ(this Vector3 v) => new Vector2(v.x, v.z);
    public static Vector2 ZX(this Vector3 v) => new Vector2(v.z, v.x);
    #endregion

    #region Swizels
    /// <summary> Returns: (x, z, 0) </summary>
    public static Vector3 XZ0(this Vector3 v) => new Vector3(v.x, v.z, 0f);
    /// <summary> Returns: (x, 0, z) </summary>
    public static Vector3 X0Z(this Vector3 v) => new Vector3(v.x, 0f, v.z);
    /// <summary> Returns: (x, 0, y) </summary>
    public static Vector3 X0Y(this Vector3 v) => new Vector3(v.x, 0f, v.y);
    /// <summary> Returns: (z, y, 0) </summary>
    public static Vector3 ZY0(this Vector3 v) => new Vector3(v.z, v.y, 0f);

    /// <summary> Returns: (x, y, 0) </summary>
    public static Vector3 XY0(this Vector3 v) => new Vector3(v.x, v.y, 0f);
    #endregion


    #region VectorSet
    /// <summary> Set the X value only if it's one of those difficult ones. NOTE: YOU STILL HAVE TO ASSIGN IT. </summary>
    public static Vector3 WithX(this Vector3 v, float x) { Vector3 r; r.x = x; r.y = v.y; r.z = v.z; return r; }
    /// <summary> Set the Y value only if it's one of those difficult ones. NOTE: YOU STILL HAVE TO ASSIGN IT. </summary>
    public static Vector3 WithY(this Vector3 v, float y) { Vector3 r; r.x = v.x; r.y = y; r.z = v.z; return r; }
    /// <summary> Set the Z value only if it's one of those difficult ones. NOTE: YOU STILL HAVE TO ASSIGN IT. </summary>
    public static Vector3 WithZ(this Vector3 v, float z) { Vector3 r; r.x = v.x; r.y = v.y; r.z = z; return r; }
    #endregion

    #region VectorLerp
    public static Vector3 LerpX(this Vector3 v, float b, float t) { Vector3 r; r.x = Mathf.Lerp(v.x, b, t); r.y = v.y; r.z = v.z; return r; }
    public static Vector3 LerpY(this Vector3 v, float b, float t) { Vector3 r; r.x = v.x; r.y = Mathf.Lerp(v.y, b, t); r.z = v.z; return r; }
    public static Vector3 LerpZ(this Vector3 v, float b, float t) { Vector3 r; r.x = v.x; r.y = v.y; r.z = Mathf.Lerp(v.z, b, t); return r; }
    public static Vector3 Lerp(this Vector3 a, Vector3 b, float tX, float tY, float tZ)
    {
        Vector3 r;
        r.x = Mathf.Lerp(a.x, b.x, tX);
        r.y = Mathf.Lerp(a.y, b.y, tY);
        r.z = Mathf.Lerp(a.z, b.z, tZ);
        return r;
    }



    #endregion

    #region VectorMult
    public static Vector3 MultiplyX(this Vector3 v, float x) { Vector3 r; r.x = v.x * x; r.y = v.y; r.z = v.z; return r; }
    public static Vector3 MultiplyY(this Vector3 v, float y) { Vector3 r; r.x = v.x; r.y = v.y * y; r.z = v.z; return r; }
    public static Vector3 MultiplyZ(this Vector3 v, float z) { Vector3 r; r.x = v.x; r.y = v.y; r.z = v.z * z; return r; }
    #endregion

    #region Operations
    /// <summary> Multiply each component of a Vector3 with the same component of the other Vector3. </summary>
    public static Vector3 Times(this Vector3 v, Vector3 v3) => new Vector3(v.x * v3.x, v.y * v3.y, v.z * v3.z);
    /// <summary> Multiply each component of a Vector3 with the same component of the Vector3Int. </summary>
    public static Vector3 Times(this Vector3 v, Vector3Int v3Int) => new Vector3(v.x * v3Int.x, v.y * v3Int.y, v.z * v3Int.z);
    /// <summary> Divide each component of a Vector3 by the same component of the other Vector3. </summary>
    public static Vector3 DivideBy(this Vector3 v, Vector3 v3) => new Vector3(v.x / v3.x, v.y / v3.y, v.z / v3.z);

    public static Vector2 Abs(this Vector2 v) { Vector2 result; result.x = Mathf.Abs(v.x); result.y = Mathf.Abs(v.y); return result; }
    public static Vector3 Abs(this Vector3 v) { Vector3 result; result.x = Mathf.Abs(v.x); result.y = Mathf.Abs(v.y); result.z = Mathf.Abs(v.z); return result; }

    public static Vector3 Clamp3(this Vector3 v, float min, float max)
    {
        Vector3 r = v;

        if (v.x < min) r.x = min;
        else if (v.x > max) r.x = max;

        if (v.y < min) r.y = min;
        else if (v.y > max) r.y = max;

        if (v.z < min) r.z = min;
        else if (v.z > max) r.z = max;

        return r;
    }
    public static Vector4 Clamp4(this Vector4 v, float min, float max)
    {
        Vector4 r = v;

        if (v.x < min) r.x = min;
        else if (v.x > max) r.x = max;

        if (v.y < min) r.y = min;
        else if (v.y > max) r.y = max;

        if (v.z < min) r.z = min;
        else if (v.z > max) r.z = max;

        if (v.w < min) r.w = min;
        else if (v.w > max) r.w = max;

        return r;
    }
    public static Vector3 Clamp3(this Vector3 v, Vector3 min, Vector3 max)
    {
        Vector3 r = v;

        r.x = Mathf.Clamp(v.x, min.x, max.x);
        r.y = Mathf.Clamp(v.y, min.y, max.y);
        r.z = Mathf.Clamp(v.z, min.z, max.z);

        return r;
    }


    #endregion

    /// <summary> Snaps a vector to the increments. </summary>
    public static Vector3 Snapped(this Vector3 v, float increment)
    {
        if (increment < 0.000001f) return v;
        return (v / increment).Round() * increment;
    }
    public static Vector2 Snap(this Vector2 v, float increment)
    {
        if (increment < 0.000001f) return v;
        return (v / increment).Round() * increment;
    }

    public static Vector3 Limit(this Vector3 v, Vector3 extents)
    {
        v.x = v.x.Limit(extents.x);
        v.y = v.y.Limit(extents.y);
        v.z = v.z.Limit(extents.z);
        return v;
    }

    public static Vector4 Sin(this Vector4 v)
    {
        Vector4 r;
        r.x = Mathf.Sin(v.x);
        r.y = Mathf.Sin(v.y);
        r.z = Mathf.Sin(v.z);
        r.w = Mathf.Sin(v.w);
        return r;
    }
    public static Vector4 Cos(this Vector4 v)
    {
        Vector4 r;
        r.x = Mathf.Cos(v.x);
        r.y = Mathf.Cos(v.y);
        r.z = Mathf.Cos(v.z);
        r.w = Mathf.Cos(v.w);
        return r;
    }
    public static Vector4 Clamp01(this Vector4 v)
    {
        Vector4 r;
        r.x = Mathf.Clamp01(v.x);
        r.y = Mathf.Clamp01(v.y);
        r.z = Mathf.Clamp01(v.z);
        r.w = Mathf.Clamp01(v.w);
        return r;
    }
    public static Vector4 RemapN1P1_01(this Vector4 v) => (v + Vector4.one) * 0.5f;


}
