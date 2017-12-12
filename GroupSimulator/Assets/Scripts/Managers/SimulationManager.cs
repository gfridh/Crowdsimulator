using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimulationManager : MonoBehaviour {

    // We set up singleton behaviour
    public static SimulationManager instance = null;

    // This is the list of scenarios our test will go through in a testing session
    [SerializeField] private List<GroupSettings> scenarios = new List<GroupSettings>();
    [SerializeField] private Transform testerCamera;
    [SerializeField] private LatinSquare latin;

    // Internal references:
    private GroupGenerator groupGenerator;
    private RandomGenerator randomGenerator;
    private GroupSettings currentSettings;
    private CsvWriter dataWriter;
    private bool testHasStarted;
    private bool testIsRunning;

    private DateTime scenarioStarts;

    public bool TestIsRunning { get { return testIsRunning; } }

    void Awake() {
        // This prevents multiple SimulationManagers to be active at the same time
        if (instance == null) {
            instance = this;

            randomGenerator = new RandomGenerator(126789);
            dataWriter = new CsvWriter();
            dataWriter.CreateHeaders();
            testIsRunning = false;
            testHasStarted = false;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        groupGenerator = GetComponent<GroupGenerator>();
        if (groupGenerator == null) {
            Debug.LogError("Could not find the GroupGenerator on this gameobject");
        }

        latin.GenerateLatinSquare();


        Debug.Log("Session has started. Press SPACEBAR to start the scenarios");
    }

    void Update() {
        if (!testHasStarted && !testIsRunning && Input.GetKeyDown(KeyCode.Space)) {
            StartSimulation();
            testHasStarted = true;
            testIsRunning = true;
        }
    }

    /// <summary>
    /// This method starts the simulation process
    /// </summary>
    public void StartSimulation() {
        testerCamera.transform.position = new Vector3(0f, 0f, -5f);
        scenarioStarts = DateTime.Now;
        int index = randomGenerator.Range(0, scenarios.Count);

        var settings = latin.GetNextScenario();
        groupGenerator.GenerateGroups(settings);
        currentSettings = settings;
    }

    /// <summary>
    /// Called when the camera animation has finished playing
    /// </summary>
    public void NextScenario(float distance, int welcomeFactor) {
        dataWriter.SaveRow(currentSettings, distance, welcomeFactor, (float)(DateTime.Now - scenarioStarts).TotalSeconds);

        if (latin.TestLeft() > 0) {
            // First we clear then we create new groups
            groupGenerator.ClearGroups();
            StartSimulation();

        } else {
            // The simulation is finished
            Debug.Log("Testing is DONE. Writing data to file");
            dataWriter.WriteToFile();
            testIsRunning = false;
			testerCamera.transform.position = new Vector3(testerCamera.transform.position.x, testerCamera.transform.position.y, -5f);
            groupGenerator.ClearGroups();
        }
    }
}