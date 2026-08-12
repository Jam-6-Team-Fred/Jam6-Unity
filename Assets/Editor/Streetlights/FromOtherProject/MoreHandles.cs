using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public static class MoreHandles
{
    public static void DrawWireSphere(Vector3 pos, float radius)
    {
#if UNITY_EDITOR
        Handles.RadiusHandle(Quaternion.identity, pos, radius);
#endif
    }
    public static void DrawWireSphereBasic(Vector3 pos, float radius)
    {
#if UNITY_EDITOR
        Handles.DrawWireDisc(pos, Vector3.up, radius);
        Handles.DrawWireDisc(pos, Vector3.right, radius);
        Handles.DrawWireDisc(pos, Vector3.forward, radius);
#endif
    }

    public static void Matrix(Transform t)
    {
#if UNITY_EDITOR
        Handles.matrix = t.localToWorldMatrix;
#endif
    }

    public static void Reset()
    {
        ResetColor();
        ResetMatrix();
        ResetZTest();
    }
    public static void ResetColor()
    {
#if UNITY_EDITOR
        Handles.color = Color.white;
#endif
    }
    public static void ResetMatrix()
    {
#if UNITY_EDITOR
        Handles.matrix = Matrix4x4.identity;
#endif
    }
    public static void ResetZTest()
    {
#if UNITY_EDITOR
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
#endif
    }
    public static void ZTestNormal()
    {
#if UNITY_EDITOR
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
#endif
    }
    public static void ZTestInverted()
    {
#if UNITY_EDITOR
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
#endif
    }
    public static void Bezier(Transform transform1, Transform transform2, Color color, float bendScaling = 1f, float thickness = 2f)
    {
#if UNITY_EDITOR
        Vector3 pos0 = transform1.position;
        Vector3 pos1 = transform2.position;
        float dist = (pos0 - pos1).magnitude * bendScaling;

        Vector3 f0 = transform1.forward * dist;
        Vector3 f1 = transform2.forward * dist;

        Handles.DrawBezier(pos0, pos1, pos0 + f0, pos1 + f1, color, Texture2D.whiteTexture, thickness);
#endif
    }
    public static void Bezier(Transform transform1, Transform transform2, float bendReducing = 0.6f, float thickness = 2f)
        => Bezier(transform1, transform2, Color.white, bendReducing, thickness);
}
