using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GroupSettings))]
public class GroupSettingsEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        GroupSettings settings = (GroupSettings)target;
        GUI.backgroundColor = Color.gray;
        if (GUILayout.Button("Generate Seed")) {

            Undo.RecordObject(settings, "SettingSeedChange");
            settings.GeneratorSeed = Random.Range(int.MinValue, int.MaxValue);
            
        }
    }
}
