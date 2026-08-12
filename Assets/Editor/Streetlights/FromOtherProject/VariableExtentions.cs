using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protostellar;

public static class VariableExtentions
{
    public static bool IsInRange(this float value, float compareTo, float range) => value < compareTo + range && value > compareTo - range;
    public static bool IsBetween(this float value, float min, float max) => min < value && value < max;
    public static bool IsBetweenInclusive(this float value, float min, float max) => min <= value && value <= max;


    public static float Clamped(this float f, float min, float max) => Mathf.Clamp(f, min, max);
    public static int Clamped(this int i, int min, int max) => Mathf.Clamp(i, min, max);

    public static float Clamped01(this float f) => Mathf.Clamp01(f);
    public static float Clamped(this float f, Range r) => Mathf.Clamp(f, r.min, r.max);
    public static float Wrapped(this float value, float bottom, float top)
    {
        if (value > top && value < bottom) return value;
        if (value > top) value = bottom + (value - top);
        if (value < bottom) value = top - (value + bottom);
        return value;
    }

    public static float RoundedTo(this float f, int decimals)
    {
        if (decimals < 1) return Mathf.Round(f);

        float power = Mathf.Pow(10, decimals);
        return Mathf.Round(f * power) / power;
    }

    public static float Snapped(this float f, float increment)
    {
        if (increment < 0.000001f) return f;
        return Mathf.Round(f / increment) * increment;
    }
    public static int Snapped(this int i, float increment)
    {
        if (increment < 0.000001f) return i;
        return (int)(Mathf.Round(i / increment) * increment);
    }

    public static float Remap01(this float value, float iMin, float iMax) => Mathf.InverseLerp(iMin, iMax, value);
    public static float Remap(this float value, float iMin, float iMax, float oMin, float oMax) => Mathf.Lerp(oMin, oMax, Mathf.InverseLerp(iMin, iMax, value));
    public static float RandomSign(this float value) => (UnityEngine.Random.Range(0, 2) == 1) ? value : -value;

    public static float Abs(this float f) => Mathf.Abs(f);
    public static int Abs(this int i)
    {
        if (i < 0) return -i;
        else return i;
    }
    public static float PositiveOnly(this float f) => f < 0f ? 0f : f;


    public static float Squared(this float f) => f * f;


    /// <summary> Returns 1 if true, -1 if false </summary>
    public static float Dir(this bool b) => b ? 1f : -1f;
    /// <summary> Returns -1 if true, 1 if false </summary>
    public static float InvDir(this bool b) => b ? -1f : 1f;

    /// <summary> Returns 1 if true, 0 if false </summary>
    public static float To01(this bool b) => b ? 1f : 0f;
    /// <summary> Returns -1 if true, 0 if false </summary>
    public static float Dir0N1(this bool b) => b ? -1f : 0f;


    /// <summary> Returns if the int is equal to any int in ints. </summary>
    public static bool IsAny(this int i, params int[] ints)
    {
        for (int j = 0; j < ints.Length; j++)
        {
            if (i == ints[j]) return true;
        }
        return false;
    }

    public static float Limit(this float f, float extent)
    {
        if (f > extent) return extent;      //Greater than extent, set to
        if (f < -extent) return -extent;    //Less than negative extent, set to
        
        return f;   //Within limits
    }

    public static bool IsEven(this int i) => i % 2 == 0;
    public static bool IsOdd(this int i) => i % 2 == 1;

}
