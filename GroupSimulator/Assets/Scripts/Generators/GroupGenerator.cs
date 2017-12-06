using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupGenerator : MonoBehaviour {

    [SerializeField] private Transform groupParent;
    [SerializeField] private List<GameObject> humanPrefab;

    [SerializeField] private GroupSettings settingsReference;

    private RandomGenerator randomGenerator;


    public void Start () {
        randomGenerator = new RandomGenerator(123123);
    }

    public void GenerateGroups(GroupSettings settings) {
        //if(Application.isEditor)
            //randomGenerator = new RandomGenerator(123123);


        settingsReference = settings;

        // First we create the humans in their correct posisions
        for (int i = 0; i < settings.MembersInGroups; i++) {

            GameObject newHuman = (GameObject)Instantiate(humanPrefab[randomGenerator.Range(0, humanPrefab.Count)], groupParent);

            // The humans should fill the group arc
            newHuman.transform.localPosition = Quaternion.Euler(0, (-(settings.GroupArc / 2f) + (settings.GroupArc / (settings.MembersInGroups )) * i)  + settings.GroupArc / (2 * settings.MembersInGroups), 0) * (Vector3.forward * settings.InterGroupDistance);

            // Setting up the correct human rotations, making all humans face the circle origin
            Vector3 rot = new Vector3(0f, Quaternion.FromToRotation(Vector3.forward, (groupParent.transform.position - newHuman.transform.position)).eulerAngles.y, 0f);

            // Then we make the rotations correct with regards to orientation variance
            // 1: We scale the effects depending on the humans position in the circle
            // 2: Then we also check the direction to see if we should invert the angle on the opposite sides of the circle
            float scaleFactor = (settings.ScaleFactor) ? Mathf.Clamp(Vector3.Angle(Vector3.forward, (newHuman.transform.position - groupParent.transform.position)) / 90f, 0, 1f) : 1f;
            float directionFactor = (Quaternion.FromToRotation(Vector3.forward, (groupParent.transform.position - newHuman.transform.position)).eulerAngles.y < 180) ? -1f : 1f;

            rot.y += (settings.OrientationVariance * scaleFactor * directionFactor);

            // We apply our rotation changes            
            newHuman.transform.localRotation = Quaternion.Euler(rot);
        }

        // We add a listener to check for updates
        settings.OnValuesChanged -= UpdateVisualsToMatchData;
        settings.OnValuesChanged += UpdateVisualsToMatchData;
    }

    public void ClearGroups(bool clearListenerSubscription = true) {
        foreach (Transform child in groupParent) {
            Destroy(child.gameObject);
        }

        if (clearListenerSubscription) {
            settingsReference.OnValuesChanged -= UpdateVisualsToMatchData;
            settingsReference = null;
        }
    }

    private void UpdateVisualsToMatchData() {
        ClearGroups(false);
        GenerateGroups(settingsReference);
    }
}
