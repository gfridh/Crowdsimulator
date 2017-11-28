using UnityEngine;
using System.Collections;

public class CustomNode : MonoBehaviour {

	public bool isSpawn = false;
	public bool isGoal = false;

	public virtual float getThreshold() {
		return 0.5f * Mathf.Min (transform.localScale.x, transform.localScale.z); //A circle
	}

	public virtual Vector3 getTargetPoint(Vector3 origin) {
		return transform.position;

	}
}
