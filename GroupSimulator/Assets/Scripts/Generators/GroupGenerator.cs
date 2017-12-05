using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupGenerator : MonoBehaviour {

    [SerializeField] private Transform groupParent;
    [SerializeField] private GameObject humanPrefab;

    public void GenerateGroups(GroupSettings settings) {

        // First we create the humans in their correct posisions
        for (int i = 0; i < settings.MembersInGroups; i++) {

            GameObject newHuman = (GameObject)Instantiate(humanPrefab, groupParent);

            // The humans should fill the half of the circle
            newHuman.transform.localPosition = Quaternion.Euler(0, (180f / (settings.MembersInGroups - 1)) * i, 0) * (Vector3.left * settings.InterGroupDistance);

            // Setting up the correct human rotations, making all humans face the circle origin
            Vector3 rot = new Vector3(0f, Quaternion.FromToRotation(Vector3.forward, (groupParent.transform.position - newHuman.transform.position)).eulerAngles.y, 0f);

            // Then we make the rotations correct with regards to orientation variance
            // 1: We scale the effects depending on the humans position in the circle
            // 2: Then we also check the direction to see if we should invert the angle on the opposite sides of the circle
            float scaleFactor = Vector3.Angle(Vector3.forward, (newHuman.transform.position - groupParent.transform.position)) / 90f;
            float directionFactor = (Quaternion.FromToRotation(Vector3.forward, (groupParent.transform.position - newHuman.transform.position)).eulerAngles.y < 180) ? -1f : 1f;

            rot.y += (settings.OrientationVariance * scaleFactor * directionFactor);

            // We apply our rotation changes            
            newHuman.transform.localRotation = Quaternion.Euler(rot);
        }

    }

    public void ClearGroups() {
        foreach (Transform child in groupParent) {
            Destroy(child.gameObject);
        }
    }
}
