using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName="Settings/GroupSettings")]
public class GroupSettings : ScriptableObject {

    [Header("Group Generation Settings:")]
    [Tooltip("The number of groups to spawn in the scene")]
    [SerializeField, Range(0, 10)] protected int numberOfGroups;

    [Tooltip("The intensity of noise in the scene, e.g. the amount of people not within a group")]
    [SerializeField, Range(0, 10)] protected int noiseIntensity;

    [Header("Inner-Group Settings:")]
    [Tooltip("The amount of people within the groups")]
    [SerializeField, Range(0, 10)] protected int membersInGroups;

    [Tooltip("The variance from the origin orientation for the group")]
    [SerializeField, Range(0, 180f)] protected float orientationVariance;

    [Tooltip("The distance from the group origin, how big is the group?")]
    [SerializeField, Range(0, 180f)] protected float interGroupDistance;

    [Header("Generator Seed:")]

    [Tooltip("The random seed for the Generator, manages the ranom placements for the groups and so on")]
    [SerializeField] protected int generatorSeed;


    /// <summary>
    /// The number of groups to spawn in the scene
    /// </summary>
    public int NumberOfGroups { get { return numberOfGroups; } }
    /// <summary>
    /// The intensity of noise in the scene, e.g. the amount of people not within a group
    /// </summary>
    public int NoiseIntensity { get { return noiseIntensity; } }
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
    /// The random seed for the Generator
    /// </summary>
    public int GeneratorSeed { get { return generatorSeed; } set { generatorSeed = value; } }
}
