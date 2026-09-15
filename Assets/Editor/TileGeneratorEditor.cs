using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileGenerator))]
public class TileGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileGenerator generator = (TileGenerator)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generate Tiles", GUILayout.Height(32)))
        {
            generator.Generate();
        }

        if (GUILayout.Button("Clear Tiles", GUILayout.Height(28)))
        {
            generator.Clear();
        }
    }
}