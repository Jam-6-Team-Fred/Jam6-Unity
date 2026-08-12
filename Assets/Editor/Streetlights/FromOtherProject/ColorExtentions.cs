using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class ColorExtentions
{
    /// <summary> Multiply RGB but not A. </summary>
    public static Color MultiplyRGB(this Color c, float f) => new Color(c.r * f, c.g * f, c.b * f, c.a);



}
