using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName="Settings/GroupSettings")]
public class GroupSettings : ScriptableObject {

    [Header("Inner-Group Settings:")]
    [Tooltip("The amount of people within the groups")]
    [SerializeField, Range(0, 10)] protected int membersInGroups;

    [Tooltip("The variance from the origin orientation for the group")]
    [SerializeField, Range(-50f, 50f)] protected float orientationVariance;

    [Tooltip("The distance from the group origin, how big is the group?")]
    [SerializeField, Range(1, 5f)] protected float interGroupDistance;

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
}
