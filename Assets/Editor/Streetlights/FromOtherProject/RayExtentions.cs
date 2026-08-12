using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RayExtentions
{
    public static float DistanceAlongRay(this Ray ray, Vector3 pos)
    {
        Vector3 vector = pos - ray.origin;   //Vector to ray
        return Vector3.Dot(vector, ray.direction); //Length along direction from origin
    }

    public static Vector3 NearestPointOnAxis(this Vector3 pos, Vector3 rayDir)
    {
        float lengthAlongRay = Vector3.Dot(pos, rayDir);    //Length along direction from origin
        return rayDir * lengthAlongRay;
    }
    public static Vector3 NearestPointOnRay(this Vector3 pos, Vector3 rayOrigin, Vector3 rayDir)
    {
        Vector3 vector = pos - rayOrigin;    //Vector to ray
        float lengthAlongRay = Vector3.Dot(vector, rayDir);    //Length along direction from origin
        Vector3 nearestPoint = rayOrigin + rayDir * lengthAlongRay;
        return nearestPoint;
    }
    public static Vector3 NearestPointOnRay(this Ray ray, Vector3 pos)
    {
        Vector3 vector = pos - ray.origin;    //Vector to ray
        float lengthAlongRay = Vector3.Dot(vector, ray.direction);    //Length along direction from origin
        Vector3 nearestPoint = ray.origin + ray.direction * lengthAlongRay;
        return nearestPoint;
    }
    public static Vector3 NearestPointOnRay(this Ray ray, Vector3 pos, out float lengthAlongRay)
    {
        Vector3 vector = pos - ray.origin;    //Vector to ray
        lengthAlongRay = Vector3.Dot(vector, ray.direction);    //Length along direction from origin
        Vector3 nearestPoint = ray.origin + ray.direction * lengthAlongRay;
        return nearestPoint;
    }
    public static bool PointInRayRange(this Ray ray, Vector3 pos, float range)
    {
        Vector3 nearestPoint = ray.NearestPointOnRay(pos);

        float dist = (nearestPoint - pos).sqrMagnitude;
        return dist < (range * range);

    }

    public static float DistanceToRay(this Ray ray, Vector3 pos)
    {
        Vector3 vector = pos - ray.origin;    //Vector to ray
        float lengthAlongRay = Vector3.Dot(vector, ray.direction);    //Length along direction from origin
        Vector3 nearestPoint = ray.origin + ray.direction * lengthAlongRay;
        return (nearestPoint - pos).magnitude;
    }
    public static float DistanceToRay(this Ray ray, Vector3 pos, out float lengthAlongRay)
    {
        Vector3 vector = pos - ray.origin;    //Vector to ray
        lengthAlongRay = Vector3.Dot(vector, ray.direction);    //Length along direction from origin
        Vector3 nearestPoint = ray.origin + ray.direction * lengthAlongRay;
        return (nearestPoint - pos).magnitude;
    }


    /// <summary> Returns the distance to the plane from ray origin. </summary>
    public static float DistanceToPlane(this Ray ray, Vector3 planePos, Vector3 planeNormal)
    {
        //-------- Get the dot product between ray and normal --------//
        float dot = Vector3.Dot(ray.direction, planeNormal);

        //-------- If 0, parallel, avoid --------//
        if (Mathf.Abs(dot) < 0.00001f) return 1000000f;

        //-------- Get Distance --------//
        return Vector3.Dot(planePos - ray.origin, planeNormal) / dot;
    }
    /// <summary> Point where the ray intersects the plane at position with normal. </summary>
    public static Vector3 PointIntersectsPlane(this Ray ray, Vector3 planePos, Vector3 planeNormal)
        => ray.GetPoint(ray.DistanceToPlane(planePos, planeNormal));

    /// <summary> Point on ray that is closest to other ray? </summary>
    public static Vector3 ClosestApproach(this Ray ray, Ray line)
    {
        Vector3 rayDir = (line.origin - ray.origin).normalized;

        Vector3 planePos = ray.PointIntersectsPlane(line.origin, rayDir); // ray.direction);

        return line.NearestPointOnRay(planePos);
    }

    /// <summary> Uses the distance function supplied to raymarch. Returns the hit position. </summary>
    public static Vector3 Raymarch(this Ray ray, DistanceFunction distanceFunction, ref float minDist, int maxSteps = 15, float maxDist = 50f)
    {
        Vector3 pos = Vector3.zero;
        float distance = 0f;

        for (int i = 0; i < maxSteps; i++)
        {
            pos = ray.GetPoint(distance);
            float distTo = distanceFunction(pos);
            distance += distTo;

            if (distTo < minDist)
            {
                minDist = distTo;
                break;
            }
        }

        return pos;
    }

    /// <summary> Transforms ray from local space to world space. </summary>
    public static Ray TransformRay(this Ray ray, Transform transform)
    {
        ray.direction = transform.TransformDirection(ray.direction);
        ray.origin = transform.TransformPoint(ray.origin);
        return ray;
    }
    /// <summary> Transforms ray from world space to local space. </summary>
    public static Ray InverseTransformRay(this Ray ray, Transform transform)
    {
        ray.direction = transform.InverseTransformDirection(ray.direction);
        ray.origin = transform.InverseTransformPoint(ray.origin);
        return ray;
    }

    /*
    bool intersect(this Ray r, Vector3 min, Vector3 max)
    {
        Vector3 orig = r.origin;
        Vector3 dir = r.direction;

        float tmin = (min.x - orig.x) / dir.x; 
        float tmax = (max.x - orig.x) / dir.x; 
 
        if (tmin > tmax) swap(tmin, tmax); 
 
        float tymin = (min.y - orig.y) / dir.y; 
        float tymax = (max.y - orig.y) / dir.y; 
 
        if (tymin > tymax) swap(tymin, tymax); 
 
        if ((tmin > tymax) || (tymin > tmax)) return false;
 
        if (tymin > tmin) tmin = tymin; 
 
        if (tymax < tmax) tmax = tymax; 
 
        float tzmin = (min.z - orig.z) / dir.z; 
        float tzmax = (max.z - orig.z) / dir.z; 
 
        if (tzmin > tzmax) swap(tzmin, tzmax); 
        if ((tmin > tzmax) || (tzmin > tmax)) return false; 
        if (tzmin > tmin) tmin = tzmin; 
        if (tzmax < tmax) tmax = tzmax; 
 
        return true; 
    }
    */
}

public delegate float DistanceFunction(Vector3 pos);

