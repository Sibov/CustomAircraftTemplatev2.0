using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ObjectBuilderScript))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

       ObjectBuilderScript myScript = (ObjectBuilderScript)target;
        if (GUILayout.Button("LOAD"))
        {
           myScript.load();
        }

        if (GUILayout.Button("Save"))
        {
            myScript.save();
        }
    }
}