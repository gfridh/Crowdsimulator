﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

	public enum Method{
		uniformSpawn,
		circleSpawn,
		discSpawn,
		continuousSpawn,
		areaSpawn
	}

	public enum LCPSolutioner {
		mprgp,
		mprgpmic0,
		psor
	}
	public float epsilon;
	public int solverMaxIterations;
	public LCPSolutioner solver;
	public Method spawnMethod;

	public float planeSize;
	public int numberOfAgents;
	[Range(0.00f, 1.00f)]
	public float percentOfGroupedAgents;
	public float agentAvoidanceRadius;
	public float agentMaxSpeed;

	public float circleRadius;
	public int numberOfDiscRows;

	public int rows;
	public int rowLength;

	public Agent agentPrefab;
	public SubgroupAgent agentPrefabGroup;
	public Grid gridPrefab;
	public Spawner spawnerPrefab;
	public MapGen mapGen;
	public Plane plane;
	internal static Vector2 xMinMax;
	internal static Vector2 zMinMax;
	internal MapGen.map roadmap;

	public int cellsPerRow;
	public int neighbourBins;
	public int roadNodeAmount;
	public bool visibleMap;
	internal float ringDiameter;

	public float continousSpawnRate;

	[Range(0.01f, 0.05f)]
	public float timeStep; 

	[Range(0.01f, 1f)]
	public float alpha; 

	List<Agent> agentList = new List<Agent>();

	public bool showSplattedDensity = false;
	public bool showSplattedVelocity = false;
	public bool walkBack = false;
	public bool skipNodeIfSeeNext = false;
	public bool handleCollision = false;

	/**
	 * Initialize simulation by taking the user's options into consideration and spawn agents.
	 * Then create the Staggered Grid along with all cells and velocity nodes.
	**/
	void Start () {
		plane.transform.localScale = new Vector3 (planeSize, 1.0f, planeSize);
		Vector3 planeLength = plane.getLengths (); //Staggered grid length
		xMinMax = new Vector2 (plane.transform.position.x - planeLength.x / 2, 
			                   plane.transform.position.x + planeLength.x / 2);
		zMinMax = new Vector2 (plane.transform.position.z - planeLength.z / 2, 
							  plane.transform.position.z + planeLength.z / 2);
		
		ringDiameter = agentAvoidanceRadius * 2; //Prefered distance between two agents

		//Creates roadmap / pathfinding for agents based on map
		MapGen m = Instantiate (mapGen) as MapGen; 
		roadmap = m.generateRoadMap (roadNodeAmount, xMinMax, zMinMax, visibleMap);


		Grid grid = Instantiate (gridPrefab) as Grid;
		grid.showSplattedDensity = showSplattedDensity;
		grid.showSplattedVelocity = showSplattedVelocity;
		grid.cellsPerRow = cellsPerRow;
		grid.agentMaxSpeed = agentMaxSpeed;
		grid.ringDiameter = ringDiameter;
		grid.mapGen = mapGen;
		grid.dt = timeStep; 
		grid.neighbourBins = neighbourBins;
		grid.solver = solver;
		grid.solverEpsilon = epsilon;
		grid.solverMaxIterations = solverMaxIterations;
		grid.colHandler = handleCollision;
		grid.agentAvoidanceRadius = agentAvoidanceRadius;
		Grid.instance = grid;
		Grid.instance.initGrid (xMinMax, zMinMax, alpha, agentAvoidanceRadius);

		for (int i = 0; i < roadmap.spawns.Count; ++i)
			roadmap.spawns [i].spawner.init (ref agentPrefab, ref agentPrefabGroup, ref roadmap, ref agentList, xMinMax, zMinMax, agentAvoidanceRadius);
		
		switch(spawnMethod) {
		case Method.uniformSpawn:
			agentList.AddRange (roadmap.spawns [0].spawner.spawnRandomAgents (numberOfAgents));
			break;
		case Method.areaSpawn:
			for (int i = 0; i < roadmap.spawns.Count; ++i)
				agentList.AddRange (roadmap.spawns [i].spawner.spawnAreaAgents (rows, rowLength, roadmap.spawns [i].node, percentOfGroupedAgents));
			break;
		case Method.circleSpawn:
			agentList = spawnerPrefab.circleSpawn (numberOfAgents, circleRadius, planeSize);
			break;
		case Method.discSpawn:
			agentList = spawnerPrefab.discSpawn (planeSize, circleRadius, numberOfDiscRows);
			Debug.Log ("Spawned: " + agentList.Count + " agents");
			break;
		case Method.continuousSpawn:
			for (int i = 0; i < roadmap.spawns.Count; ++i)
				roadmap.spawns [i].spawner.continousSpawn (roadmap.spawns [i].node, numberOfAgents, continousSpawnRate, percentOfGroupedAgents);
			break;
		default:
			agentList = new List<Agent> (); 
			break;
		}

	}
	

	/**
	 * Main simulation loop which is called every frame
	**/
	void Update () {

		Grid.instance.solver = solver;
		Grid.instance.solverEpsilon = epsilon;
		Grid.instance.solverMaxIterations = solverMaxIterations;

	
		// Update grid with new density and velocity values
		Grid.instance.updateCellDensity ();
		Grid.instance.updateVelocityNodes ();

		//Solve linear constraint problem
		Grid.instance.PsolveRenormPsolve ();

		//Move agents
		for (int i = 0; i < agentList.Count; ++i) {
			if (agentList [i].done) {

				Destroy (agentList [i].gameObject);
				agentList.RemoveAt (i);
				continue;
			}
			agentList [i].move(ref roadmap);
		}
		//Pair-wise collision handling between agents

		Grid.instance.collisionHandling(ref agentList);

		//flags
		Grid.instance.showSplattedDensity = showSplattedDensity;
		Grid.instance.showSplattedVelocity = showSplattedVelocity;
		Grid.instance.walkBack = walkBack;
		Grid.instance.skipNodeIfSeeNext = skipNodeIfSeeNext;
	}
}
