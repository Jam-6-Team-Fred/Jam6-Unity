using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtentions
{
    public static Vector3[] Directions()
    {
        return new Vector3[6]
        {
            Vector3.right, Vector3.up, Vector3.forward,
            Vector3.left, Vector3.down, Vector3.back
        };
    }

    public static Vector2 Round(this Vector2 v)
    {
        Vector2 r;
        r.x = Mathf.Round(v.x);
        r.y = Mathf.Round(v.y);
        return r;
    }
    public static Vector3 Round(this Vector3 v)
    {
        Vector3 r;
        r.x = Mathf.Round(v.x);
        r.y = Mathf.Round(v.y);
        r.z = Mathf.Round(v.z);
        return r;
    }

    /// <summary> Rotate a point around another point. </summary>
    public static Vector3 RotateAround(this Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 vector = point - pivot;
        vector = rotation * vector;
        return pivot + vector;
    }
    /// <summary> Rotate a vector by a rotation. </summary>
    public static Vector3 RotateVector(this Vector3 vector, Quaternion rotation) => rotation * vector;

    /// <summary> Rotate a vector around an axis. </summary>
    public static Vector3 RotateVector(this Vector3 vector, float angle, Vector3 axis)
        => Quaternion.AngleAxis(angle, axis) * vector;

    public static bool InRangeSphere(this Vector3 centre, Vector3 compareTo, float range)
    {
        float distance = (centre - compareTo).sqrMagnitude;
        range *= range;
        return (distance < range);
    }

    public static Vector3[] GetAllPerpendicularDirections(this Vector3 v, Vector3 r)
    {
        Vector3[] dirs = new Vector3[6];

        //-------- Up, right and forward --------//
        dirs[0] = v;
        dirs[1] = Vector3.Cross(v, r);
        dirs[2] = Vector3.Cross(v, dirs[1]);

        //-------- Others just inverse of first --------//
        dirs[3] = -dirs[0];
        dirs[4] = -dirs[1];
        dirs[5] = -dirs[2];

        return dirs;
    }
    public static bool ApproxZero(this float f) => f.IsInRange(0f, 0.0001f);

    /// <summary> Useful to check if a vector is normalized or not. Returns: v.sqrMagnitude less than 0.3f </summary>
    public static bool ApproxZero(this Vector3 v) => v.sqrMagnitude < 0.3f;
    public static bool AnyZero(this Vector3 v)
    {
        if (v.x.ApproxZero()) return true;
        if (v.y.ApproxZero()) return true;
        if (v.z.ApproxZero()) return true;
        
        return false;
    }


    /// <summary> Returns a tangent of this vector. </summary>
    public static Vector3 Tangent(this Vector3 v)
    {
        Vector3 tangent = Vector3.Cross(v, Vector3.up);
        if (tangent.ApproxZero()) tangent = Vector3.Cross(v, Vector3.forward);
        
        return tangent;
    }
    /// <summary> Returns a random tangent of this vector. </summary>
    public static Vector3 RandomTangent(this Vector3 v)
    {
        Vector3 tangent = Vector3.Cross(v, Random.onUnitSphere);
        if (tangent.ApproxZero()) tangent = v.Tangent();	//If failed to give a random tangent, give regular tangent

        return tangent;
    }


    public static Vector3[] GetDirections()
    {
        return new Vector3[]
        {
            Vector3.right, Vector3.up, Vector3.forward,
            Vector3.left, Vector3.down, Vector3.back
        };
    }

    public static float DistanceToPlane(this Vector3 pos, Vector3 planePos, Vector3 planeNormal)
    {
        return Vector3.Dot(pos - planePos, planeNormal);
    }
    public static Vector3 NearestPointOnPlane(this Vector3 pos, Vector3 planePos, Vector3 planeNormal)
    {
        float dist = pos.DistanceToPlane(planePos, planeNormal);
        return pos - planeNormal * dist;
    }
    public static Vector3 NearestPointOnDisc(this Vector3 pos, Vector3 planePos, Vector3 planeNormal, float radius)
    {
        Vector3 posOnPlane = pos.NearestPointOnPlane(planePos, planeNormal);
        
        Vector3 offset = posOnPlane - planePos;
        offset = Vector3.ClampMagnitude(offset, radius);
        return planePos + offset;
    }
    public static Vector3 NearestPointOnTorus(this Vector3 pos, Vector3 planePos, Vector3 planeNormal, float radius)
    {
        Vector3 posOnPlane = pos.NearestPointOnPlane(planePos, planeNormal);

        Vector3 offset = posOnPlane - planePos;
        return planePos + offset.normalized * radius;
    }

    public static Vector2Int RoundToInt(this Vector2 v) => new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));


}
