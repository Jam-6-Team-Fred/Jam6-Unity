using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Struct containing some preset colors. </summary>
public struct Colors
{
    public static Color dullGray = Set(255, 255, 255, 25);
    public static Color greenSelected = Set(120, 255, 150, 255);
    public static Color greenFaded = Set(120, 255, 150, 255) * 0.5f;

    public static Color gray1 = Set(150, 150, 155);
    public static Color gray2 = Set(230, 230, 235);

    public static Color blue1 = Set(180, 210, 255);
    public static Color green1 = Set(170, 255, 170);
    public static Color orange1 = Set(255, 180, 100);
    public static Color orange2 = Set(255, 200, 120);

    public static Color yellow = Set(255, 230, 100);

    public static Color red1 = Set(255, 130, 120);
    public static Color red2 = Set(255, 150, 160);
    public static Color red3 = Set(255, 220, 210);

    public static Color pink = Set(255, 165, 190);

    public static Color zero => new Color(0,0,0,0);


    /// <summary>
    /// Set colors using 0-255 values.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Color Set(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f);
    public static Color Set(int r, int g, int b, int a) => new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    public static Color FromDirection(Vector3 color) => (new Color(color.x, color.y, color.z) + Color.white) * 0.5f;
    public static Color Brightness(int rgb) => new Color(rgb / 255f, rgb / 255f, rgb / 255f);
    public static Color Alpha(float alpha) => new Color(0f, 0f, 0f, alpha);


    public static float ColorBrightness(Color c) => (c.r * 33.3333f) + (c.g * 50f) + (c.b * 16.6666f);
    public static float LinearBrightness(Color c) => c.r + c.g + c.b;
    public static float ActualBrightness(Color c) => (c.r * 30f) + (c.g * 59f) + (c.b * 11f);

}
