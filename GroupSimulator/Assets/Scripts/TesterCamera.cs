using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesterCamera : MonoBehaviour {

    [SerializeField, Range(1f, 10f)] private float movingSpeed = 5f;
    [SerializeField] private Transform groupParent;

    void Update() {
        transform.position += new Vector3(0f, 0f, (Input.GetAxis("Vertical") / 250f) * movingSpeed);

        if (SimulationManager.instance.TestIsRunning) {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                SimulationManager.instance.NextScenario(Vector3.Distance(transform.position, groupParent.position), 1);
            } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                SimulationManager.instance.NextScenario(Vector3.Distance(transform.position, groupParent.position), 2);
            } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                SimulationManager.instance.NextScenario(Vector3.Distance(transform.position, groupParent.position), 3);
            } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
                SimulationManager.instance.NextScenario(Vector3.Distance(transform.position, groupParent.position), 4);
            } else if (Input.GetKeyDown(KeyCode.Alpha5)) {
                SimulationManager.instance.NextScenario(Vector3.Distance(transform.position, groupParent.position), 5);
			} else if (Input.GetKeyDown(KeyCode.Alpha0)) {
				SimulationManager.instance.NextScenario(Vector3.Distance(transform.position, groupParent.position), 0);
			}
        }
    }
}