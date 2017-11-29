using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesterCamera : MonoBehaviour {

    /// <summary>
    /// Called from animation on this object
    /// </summary>
    public void FinishAnimation() {
        SimulationManager.instance.NextScenario();
    }
}
