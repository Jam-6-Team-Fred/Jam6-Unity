using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Protostellar.EditorStuff
{
    //% = Control, # = Shift
	public static class ProtostellarTools
	{
        const string folder = "PROTOSTELLAR";
        static Vector3 CamPivot => SceneView.lastActiveSceneView.pivot;
        static bool playing => Application.isPlaying;

        //--------------------------------------------- Selection ---------------------------------------------//
        [MenuItem("Streetlights/Selection/To Pivot _q", validate = true)]
        public static bool V_MoveSelectedToPivot() => ValidatePresets.NotPlaying && ValidatePresets.ObjectSelected;

        [MenuItem("Streetlights/Selection/To Pivot _q")]
        public static void MoveSelectedToPivot()
        {
            Transform TF = Selection.activeGameObject.transform;
            Undo.RecordObject(TF, "Move Object To Pivot");
            TF.position = CamPivot;
        }
    }

    public static class ValidatePresets
    {
        public static bool ObjectSelected => Selection.activeGameObject != null;
        public static bool SelectedHasParent
        {
            get
            {
                if (Selection.activeGameObject == null) return false;
                if (Selection.activeGameObject.transform.parent == null) return false;
                return true;
            }
        }
        public static bool SelectedHasChildren
        {
            get
            {
                if (Selection.activeGameObject == null) return false;
                if (Selection.activeGameObject.transform.childCount == 0) return false;
                return true;
            }
        }
        public static bool NotPlaying => !Application.isPlaying;
    }

}
