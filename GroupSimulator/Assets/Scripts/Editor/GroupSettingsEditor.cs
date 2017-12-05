using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(GroupSettings))]
public class GroupSettingsEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        GroupSettings settings = (GroupSettings)target;

        if (GUI.changed && Application.isPlaying) {
            settings.UpdateVisuals();
        }
    }
}