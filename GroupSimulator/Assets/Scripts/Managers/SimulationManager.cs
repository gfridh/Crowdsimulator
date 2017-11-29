using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour {

    // We set up singleton behaviour
    public static SimulationManager instance = null;

    // This is the list of scenarios our test will go through in a testing session
    [SerializeField] private List<GroupSettings> scenarios = new List<GroupSettings>();



    // Internal references:
    private GroupGenerator groupGenerator;




    void Awake() {
        // This prevents multiple SimulationManagers to be active at the same time
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        groupGenerator = GetComponent<GroupGenerator>();
        if (groupGenerator == null) {
            Debug.LogError("Could not find the GroupGenerator on this gameobject");
        }


        StartSimulation();
    }


    public void StartSimulation() {
        groupGenerator.GenerateGroups(scenarios[0]);
    }



    /// <summary>
    /// Called when the camera animation has finished playing
    /// </summary>
    public void NextScenario() {
        if (scenarios.Count > 0) {
            scenarios.RemoveAt(0);

            // First we clear then we create new groups
            groupGenerator.ClearGroups();
            groupGenerator.GenerateGroups(scenarios[0]);

        } else {
            Debug.LogError("Trying to remove the first intex of an empty array!");
        }
    }
}