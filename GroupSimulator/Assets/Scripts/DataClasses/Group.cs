using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group {

    public int memberCount;
    public float orientationVariance;
    public float interGroupDistance;
    public Vector3 groupOrigin;

    /// <summary>
    /// The data class for the group of humans
    /// </summary>
    /// <param name="memberCount"></param>
    /// <param name="orientationVariance"></param>
    /// <param name="interGroupDistance"></param>
    public Group(int memberCount, float orientationVariance, float interGroupDistance, Vector3 groupOrigin) {
        this.memberCount = memberCount;
        this.orientationVariance = orientationVariance;
        this.interGroupDistance = interGroupDistance;
    }
}
