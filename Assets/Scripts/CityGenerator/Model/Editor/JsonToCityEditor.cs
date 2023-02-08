using System.Collections;
using System.Collections.Generic;
using CityGen;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(JsonToCity))]
public class JsonToCityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        JsonToCity myScript = (JsonToCity)target;
        if(GUILayout.Button("Clear"))
        {
            myScript.Clear();
            SceneDataUtil.ClearData("Meshes");
        }

        if(GUILayout.Button("Generate"))
        {
            myScript.Generar();
        }

        if(GUILayout.Button("Minify"))
        {
            myScript.Minify();
        }
    }
}
