using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SubgroupAgent : Agent {

	public enum state {
		abreast, 
		river,
		vformation
	}
	public struct companions {
		internal List<SubgroupAgent> comp; //Should be max 4
		internal List<Vector3> slots;
		internal List<Vector3> desiredSlots;
		internal List<bool> assigned;
		internal bool calculated;
		internal int toCalculate;
		internal float mo, mf;
		internal int leaderNumber;
		internal state st;
		internal float s;
		internal List<GameObject> balls;
		internal string tag;
		internal bool noLeader;
		public companions(List<SubgroupAgent> group, int leaderNumber, string tag) {
			comp = group;
			this.tag = tag;
			this.noLeader = false;
			slots = new List<Vector3>();
			desiredSlots = new List<Vector3>();
			assigned = new List<bool>();
			balls = new List<GameObject>(); 
			calculated = false;
			toCalculate = group.Count;
			mo = 5.0f; mf = 10.0f;
			this.leaderNumber = leaderNumber;
			st = state.abreast;
			s = 0.0f;
			for(int i = 0; i < 5; ++i) {
				slots.Add(Vector3.zero);
				desiredSlots.Add(Vector3.zero);
				assigned.Add(false);
			}
			Vector3 pi = comp [leaderNumber].transform.position;
			Vector3 vf = comp [leaderNumber].transform.forward;
			Vector3 vr = comp [leaderNumber].transform.right;
			desiredSlots [0] = pi + 0.0f * vf + 0.0f * vr;
			desiredSlots [1] = pi + 0.0f * vf + 0.6f * vr;
			desiredSlots [2] = pi + 0.0f * vf + -0.6f * vr;
			desiredSlots [3] = pi + 0.0f * vf + 1.2f * vr;
			desiredSlots [4] = pi + 0.0f * vf + -1.2f * vr;
			for(int i = 0; i < slots.Count; ++i) {
				slots[i] = desiredSlots[i];
			//	balls.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
			//	balls[i].GetComponent<Collider>().enabled = false;
			//	balls[i].transform.position = slots[i];
			}
				
		}
	}

	~SubgroupAgent() {
		if (c.slots != null && c.slots.Count > 0) { //Instansiation check on struct value
			if (isLeader) {
				c.noLeader = true;
			}
			c.comp.RemoveAt(groupMemberNumber);
			c.leaderNumber = c.comp.Count > 0 ? 0 : -1;
		}

	}
	internal int number;
	internal int groupMemberNumber;
	internal bool isLeader = false;

	internal companions c;

	internal override void calculatePreferredVelocity(ref MapGen.map map) {
		if (c.noLeader) {
			base.calculatePreferredVelocity (ref map);
		} else {
			if (c.calculated) {
				if (isLeader) {
					base.calculatePreferredVelocity (ref map);

					if (done) {
						//Should be close enough to goal..
						c.noLeader = true;
						return;
					} else {
						c.toCalculate -= 1;
					}
				} else {
					base.calculatePreferredVelocity (ref map); // to get path indexing right
					float adjustment = 1.5f;
					if ((c.slots [number] - transform.position).magnitude < 10.0f) {
						if ((c.slots [number] - transform.position).magnitude > 1.5f) {
							
							if (Vector3.Dot (transform.position - c.slots [number], transform.forward) > 0)
								adjustment = 0.5f;
							preferredVelocity = ((c.slots [number] - transform.position).normalized) * adjustment * Grid.instance.agentMaxSpeed;
						}
					} else {
						preferredVelocity = preferredVelocity.normalized * adjustment*Grid.instance.agentMaxSpeed;
					}


					preferredVelocity.y = 0f;
					c.toCalculate -= 1;
				}
				if (c.toCalculate == 0)
					c.calculated = false;
			} else {
				//Do the implementation
				groupCalculation();
				c.calculated = true;
				c.toCalculate = c.comp.Count;
				calculatePreferredVelocity (ref map);
			//	for (int i = 0; i < c.balls.Count; ++i) {
				//	c.balls [i].transform.position = c.slots [i];
			// }
			}
		}
	}

	internal void setLeader() {
		isLeader = true; //Change later
	}

	private void assignSlots() {
		for (int i = 0; i < c.slots.Count; ++i) {
			c.assigned [i] = false;
		}
		for (int i = 0; i < c.comp.Count; ++i) {
			if (c.comp [i] != null) {
				int closestIndex = -1;
				float closest = -1.0f;
				for (int j = 0; j < c.slots.Count; ++j) {
					if (c.assigned [j])
						continue;
					float dist = (c.comp [i].transform.position - c.slots [j]).magnitude;
					if (closestIndex == -1 || dist < closest) {
						closestIndex = j;
						closest = dist;
					}
				}
				c.comp [i].number = closestIndex;
				c.assigned [closestIndex] = true;
			}
		}
	}

	private float rayOneCast(Vector3 o, Vector3 d, float dis) {
		RaycastHit hit = new RaycastHit ();
		if (Physics.Raycast (o, d, out hit, dis)) {
			dis = (o - hit.point).magnitude;
		}
		return dis;
	}

	private void rayCasting() {
		if (c.noLeader || c.comp[c.leaderNumber] == null)
			return;
		Vector3 forward = c.comp [c.leaderNumber].transform.forward;
		Vector3 leaderPos = c.comp [c.leaderNumber].transform.position;
		Vector3 right = c.comp [c.leaderNumber].transform.right;

		c.mf = 10.0f; 
		for (int i = 0; i < c.slots.Count; ++i) {
			c.mf = rayOneCast (c.slots [i], forward, c.mf); 
		}
		c.mo = Mathf.Min (5.0f, c.mf);
		c.mo = rayOneCast (leaderPos, right, c.mo);
		c.mo = rayOneCast (leaderPos, -right, c.mo);
		c.mo = rayOneCast (leaderPos, Quaternion.Euler(0, 45, 0)*-right, c.mo);
		c.mo = rayOneCast (leaderPos, Quaternion.Euler(0, -45, 0)*right, c.mo);
	//	Debug.Log ("mo: " + c.mo + " mf: " + c.mf);
	}

	private void checkState() {
		state s = state.abreast;
		if (c.mo < 1.0f || c.mf < 5.0f) {
			s = state.river;
		} else if (c.mo < 2.0f || c.mf < 10.0f) {
			s = state.vformation;
		}
		if (s != c.st) {
			c.s = 0.0f;
			c.st = s;
		}
	}

	private void calculateDesiredPositions() {
		if (c.noLeader || c.comp[c.leaderNumber] == null)
			return;
		Vector3 pi = c.comp [c.leaderNumber].transform.position; pi += 1.2f*c.comp [c.leaderNumber].transform.forward;
		Vector3 vf = c.comp [c.leaderNumber].transform.forward.normalized;
		Vector3 vr = c.comp [c.leaderNumber].transform.right.normalized;
		float scale = 1.0f + Grid.instance.agentAvoidanceRadius;
		switch (c.st) {
		case state.abreast:
			c.desiredSlots [0] = pi + 0.0f * vf +  scale*0.0f * vr;
			c.desiredSlots [1] = pi + 0.0f * vf +  scale*0.6f * vr;
			c.desiredSlots [2] = pi + 0.0f * vf +  scale*-0.6f * vr;
			c.desiredSlots [3] = pi + 0.0f * vf +  scale*1.2f * vr;
			c.desiredSlots [4] = pi + 0.0f * vf +  scale*-1.2f * vr;
			break;

		case state.river:

			c.desiredSlots [0] = pi + scale*-1.2f * vf + 0.0f * vr;
			c.desiredSlots [1] = pi + scale*-0.6f * vf + 0.0f * vr;
			c.desiredSlots [2] = pi + 0.0f * vf + 0.0f * vr;
			c.desiredSlots [3] = pi + scale*0.6f * vf + 0.0f * vr;
			c.desiredSlots [4] = pi + scale*1.2f * vf + 0.0f * vr;
			break;

		case state.vformation:

			c.desiredSlots [0] = pi + scale*-0.2f * vf + 0.0f * vr;
			c.desiredSlots [1] = pi + 0.0f * vf + scale*0.5f * vr;
			c.desiredSlots [2] = pi + 0.0f * vf + scale*-0.5f * vr;
			c.desiredSlots [3] = pi + scale*0.3f * vf +scale* 0.9f * vr;
			c.desiredSlots [4] = pi + scale*0.3f * vf + scale*-0.9f * vr;
			break;
		}
	}
	private void calculateNewSlotPositions() {
		for (int i = 0; i < c.slots.Count; ++i) {
			c.slots [i] =  (1 - c.s) * c.slots[i] + c.s * c.desiredSlots[i];
		}
	}
	internal void groupCalculation() {
		//1
		assignSlots();
		//2 3
		rayCasting ();
		//4
		c.s += 0.001f;
		if (c.s > 1.0f)
			c.s = 0.9999f; //Keep it below 1
		//5 6
		checkState();
		//7 8
		calculateDesiredPositions();
		//9
		calculateNewSlotPositions();

	}

	void Start () {
		base.Start ();
	}
	

}
