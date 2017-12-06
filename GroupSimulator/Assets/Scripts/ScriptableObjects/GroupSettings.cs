using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(menuName = "Settings/GroupSettings")]
public class GroupSettings : ScriptableObject {

    [Header("Inner-Group Settings:")]
    [Tooltip("The amount of people within the groups")]
    [SerializeField, Range(2, 10)] protected int membersInGroups;

    [Tooltip("The variance from the origin orientation for the group")]
    [SerializeField, Range(-90f, 90f)] protected float orientationVariance;

    [Tooltip("The distance from the group origin, how big is the group?")]
    [SerializeField, Range(1, 5f)] protected float interGroupDistance;

    [Tooltip("The arc in degrees the group is standing in")]
    [SerializeField, Range(0, 360f)] protected float groupArc;

    [Tooltip("Enables the scale factor, which determine the orientation strenght depending on the oosition of the actor")]
    [SerializeField] protected bool scaleFactor;

    // The action to fire if we changed the values of this object, to be able to update the visuals live in the simulation
    public event Action OnValuesChanged;

    void Awake() {
        // We need to clear the action to avoid errors
        OnValuesChanged = null;
    }

    public void UpdateVisuals() {
        if (OnValuesChanged != null)
            OnValuesChanged();
    }

    /// <summary>
    /// The amount of people within the groups
    /// </summary>
    public int MembersInGroups { get { return membersInGroups; } }
    /// <summary>
    /// The variance from the origin orientation for the group
    /// </summary>
    public float OrientationVariance { get { return orientationVariance; } }
    /// <summary>
    /// The distance from the group origin, how big is the group?
    /// </summary>
    public float InterGroupDistance { get { return interGroupDistance; } }
    /// <summary>
    /// The arc in degrees the group is standing in
    /// </summary>
    public float GroupArc { get { return groupArc; } }
    /// <summary>
    /// Enables the scale factor, which determine the orientation strenght depending on the oosition of the actor
    /// </summary>
    public bool ScaleFactor { get { return scaleFactor; } }
}
