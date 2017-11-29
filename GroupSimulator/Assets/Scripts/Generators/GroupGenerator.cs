using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupGenerator : MonoBehaviour {

    [SerializeField] private Transform groupParent;
    [SerializeField] private List<GameObject> humanPrefabs = new List<GameObject>();
    [SerializeField] private BoxCollider spawnArea;

    private RandomGenerator randomGenerator;



    public void GenerateGroups(GroupSettings settings) {
        randomGenerator = new RandomGenerator(settings.GeneratorSeed);


        for (int i = 0; i < settings.NumberOfGroups; i++) {
            // We pick a random point on the map and create the group
            float xPos = spawnArea.transform.position.x + randomGenerator.Range(-spawnArea.size.x / 2, spawnArea.size.x / 2);
            float yPos = spawnArea.transform.position.y + randomGenerator.Range(-spawnArea.size.y / 2, spawnArea.size.y / 2);

            Group group = new Group(settings.MembersInGroups, settings.OrientationVariance, settings.InterGroupDistance, new Vector3(xPos, yPos, 0f));

            // Spawn in circle

            
        }
    }

    public void ClearGroups() {
        foreach (Transform child in groupParent) {
            Destroy(child.gameObject);
        }
    }
}
