using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupGenerator : MonoBehaviour {

    [SerializeField] private Transform groupParent;
    [SerializeField] private List<GameObject> humanPrefab;

    [SerializeField] private GroupSettings settingsReference;

    private RandomGenerator randomGenerator;

    public void Start () {
        randomGenerator = new RandomGenerator(Random.Range(int.MinValue, int.MaxValue));
    }

    public void GenerateGroups(GroupSettings settings, bool reGenerateModels = true) {
        if(!reGenerateModels)
            randomGenerator = new RandomGenerator(123123);

        settingsReference = settings;

        Dictionary<int, int> prefabCount = new Dictionary<int, int>();

        // First we create the humans in their correct posisions
        for (int i = 0; i < settings.MembersInGroups; i++) {
            int index;
            while (true) {
                index = randomGenerator.Range(0, humanPrefab.Count);

                if (!prefabCount.ContainsKey(index)) {
                    prefabCount.Add(index, 1);
                    break;
                } else if (prefabCount.Count == humanPrefab.Count){
                    int lowest = 1000;

                    for (int j = 0; j < prefabCount.Count; j++) {
                        lowest = (prefabCount[j] < lowest) ? prefabCount[j] : lowest;
                    }

                    if (prefabCount[index] == lowest) {
                        prefabCount[index]++;
                        break;
                    }
                }
            }

            GameObject newHuman = (GameObject)Instantiate(humanPrefab[index], groupParent);

            // The humans should fill the group arc
            newHuman.transform.localPosition = Quaternion.Euler(0, (-(settings.GroupArc / 2f) + (settings.GroupArc / (settings.MembersInGroups - 1)) * i)  , 0) * (Vector3.forward * settings.InterGroupDistance);

            // Setting up the correct human rotations, making all humans face the circle origin
            Vector3 rot = new Vector3(0f, Quaternion.FromToRotation(Vector3.forward, (groupParent.transform.position - newHuman.transform.position)).eulerAngles.y, 0f);

            // Then we make the rotations correct with regards to orientation variance
            // 1: We scale the effects depending on the humans position in the circle
            // 2: Then we also check the direction to see if we should invert the angle on the opposite sides of the circle
            float normalizeMiddle = (settings.NormalizeMiddle && i == Mathf.FloorToInt(settings.MembersInGroups / 2f)) ? Mathf.Clamp(Vector3.Angle(Vector3.forward, (newHuman.transform.position - groupParent.transform.position)) / 90f, 0, 1f) : 1f;
            float directionFactor = (Quaternion.FromToRotation(Vector3.forward, (groupParent.transform.position - newHuman.transform.position)).eulerAngles.y < 180) ? -1f : 1f;

            rot.y += (settings.OrientationVariance * normalizeMiddle * directionFactor);

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
        GenerateGroups(settingsReference,  false);
    }
}
