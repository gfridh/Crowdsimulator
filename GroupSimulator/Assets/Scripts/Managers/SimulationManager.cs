using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour {

    // We set up singleton behaviour
    public static SimulationManager instance = null;

    // This is the list of scenarios our test will go through in a testing session
    [SerializeField] private List<GroupSettings> scenarios = new List<GroupSettings>();
    [SerializeField] private Transform testerCamera;

    // Internal references:
    private GroupGenerator groupGenerator;
    private RandomGenerator randomGenerator;
    private GroupSettings currentSettings;
    private CsvWriter dataWriter;
    private bool testIsRunning;


    public bool TestIsRunning { get { return testIsRunning; } }

    void Awake() {
        // This prevents multiple SimulationManagers to be active at the same time
        if (instance == null) {
            instance = this;

            randomGenerator = new RandomGenerator(126789);
            dataWriter = new CsvWriter();
            dataWriter.CreateHeaders();
            testIsRunning = true;
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

    /// <summary>
    /// This method starts the simulation process
    /// </summary>
    public void StartSimulation() {
        testerCamera.transform.position = new Vector3(0f, 0f, -5f);
        int index = randomGenerator.Range(0, scenarios.Count);
        groupGenerator.GenerateGroups(scenarios[index]);
        currentSettings = scenarios[index];
        scenarios.RemoveAt(index);
    }

    /// <summary>
    /// Called when the camera animation has finished playing
    /// </summary>
    public void NextScenario(float distance, int welcomeFactor) {
        dataWriter.SaveRow(currentSettings, distance, welcomeFactor);

        if (scenarios.Count > 0) {
            // First we clear then we create new groups
            groupGenerator.ClearGroups();
            StartSimulation();

        } else {
            // The simulation is finished
            Debug.Log("Testing is DONE. Writing data to file");
            dataWriter.WriteToFile();
            testIsRunning = false;
        }
    }
}