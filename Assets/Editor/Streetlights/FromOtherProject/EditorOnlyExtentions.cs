using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;
using System.Reflection;

public static class EditorOnlyExtentions
{
    
    //--------------------------------------------- Input ---------------------------------------------//
    public static bool LeftMouse(this Event e) => e.button == 0;
    public static bool RightMouse(this Event e) => e.button == 1;
    public static bool MiddleMouse(this Event e) => e.button == 2;

    public static bool MouseDown(this Event e) => e.type == EventType.MouseDown;
    public static bool MouseDrag(this Event e) => e.type == EventType.MouseDrag;
    public static bool MouseUp(this Event e) => e.type == EventType.MouseUp;

}
