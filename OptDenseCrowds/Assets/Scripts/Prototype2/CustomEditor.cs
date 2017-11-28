using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Main)), CanEditMultipleObjects]
public class PropertyHolderEditor : Editor {

	public SerializedProperty
		agentMaxSpeed_Prop,
		agentPrefab_Prop,
		agentPrefabGroup_Prop,
		alpha_Prop,
		avoidanceRadius_Prop,
		circleRadius_Prop,
		continousSpawnRate_Prop,
		gridPrefab_Prop,
		handleCollison_Prop,
		lcpsolver_Prop,
		lcpsolverEpsilon_Prop,
		mapGen_Prop,
		neighbourBins_Prop,
		numberOfAgents_Prop,
		numberOfCells_Prop,
		numberOfDiscRows_Prop,
		percentOfGroupedAgents_Prop,
		planePrefab_Prop,
		planeSize_Prop,
		roadNode_Prop,
		rowAmount_Prop,
		rowLength_Prop,
		skip_Prop,
		solverIterations_Prop,
		spawnerPrefab_Prop,
		spawnMethod_Prop,
		timeStep_Prop,
		visualizeDensity_Prop,
		visualizeVelocity_Prop,
		visibleMap_Prop,
		walkBack_Prop;
		

		

	
	void OnEnable () {
		// Setup the SerializedProperties
		agentMaxSpeed_Prop = serializedObject.FindProperty ("agentMaxSpeed");
		agentPrefab_Prop = serializedObject.FindProperty ("agentPrefab");
		agentPrefabGroup_Prop = serializedObject.FindProperty("agentPrefabGroup");
		alpha_Prop = serializedObject.FindProperty ("alpha");
		avoidanceRadius_Prop = serializedObject.FindProperty ("agentAvoidanceRadius");
		circleRadius_Prop = serializedObject.FindProperty ("circleRadius");
		continousSpawnRate_Prop = serializedObject.FindProperty ("continousSpawnRate");
		gridPrefab_Prop = serializedObject.FindProperty ("gridPrefab");
		handleCollison_Prop = serializedObject.FindProperty ("handleCollision");
		lcpsolver_Prop = serializedObject.FindProperty ("solver");
		lcpsolverEpsilon_Prop = serializedObject.FindProperty ("epsilon");
		mapGen_Prop = serializedObject.FindProperty ("mapGen");
		neighbourBins_Prop = serializedObject.FindProperty ("neighbourBins");
		numberOfAgents_Prop = serializedObject.FindProperty ("numberOfAgents");
		numberOfCells_Prop = serializedObject.FindProperty ("cellsPerRow");
		numberOfDiscRows_Prop = serializedObject.FindProperty ("numberOfDiscRows");
		percentOfGroupedAgents_Prop = serializedObject.FindProperty ("percentOfGroupedAgents");
		planePrefab_Prop = serializedObject.FindProperty ("plane");
		planeSize_Prop = serializedObject.FindProperty ("planeSize");
		roadNode_Prop = serializedObject.FindProperty ("roadNodeAmount");
		rowAmount_Prop = serializedObject.FindProperty ("rows");
		rowLength_Prop = serializedObject.FindProperty ("rowLength");
		solverIterations_Prop = serializedObject.FindProperty ("solverMaxIterations");
		skip_Prop = serializedObject.FindProperty ("skipNodeIfSeeNext");
		spawnerPrefab_Prop = serializedObject.FindProperty ("spawnerPrefab");
		spawnMethod_Prop = serializedObject.FindProperty ("spawnMethod");
		timeStep_Prop = serializedObject.FindProperty ("timeStep");
		visualizeDensity_Prop = serializedObject.FindProperty ("showSplattedDensity");
		visualizeVelocity_Prop = serializedObject.FindProperty ("showSplattedVelocity");
		visibleMap_Prop = serializedObject.FindProperty ("visibleMap");
		walkBack_Prop = serializedObject.FindProperty ("walkBack");
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update ();
		EditorGUILayout.PropertyField(planeSize_Prop);
		EditorGUILayout.PropertyField(roadNode_Prop);
		EditorGUILayout.PropertyField(numberOfCells_Prop);
		EditorGUILayout.PropertyField (neighbourBins_Prop);
		EditorGUILayout.PropertyField(agentMaxSpeed_Prop);
		EditorGUILayout.PropertyField(timeStep_Prop);
		EditorGUILayout.PropertyField(alpha_Prop);
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(spawnMethod_Prop);

		Main.Method m = (Main.Method)spawnMethod_Prop.enumValueIndex;
		switch( m ) {
		case Main.Method.uniformSpawn:
			EditorGUILayout.PropertyField (numberOfAgents_Prop);
			break;
		case Main.Method.circleSpawn:        
			EditorGUILayout.PropertyField (circleRadius_Prop);   
			EditorGUILayout.IntSlider (numberOfAgents_Prop, 0, (int)(2*Mathf.PI*circleRadius_Prop.floatValue/(avoidanceRadius_Prop.floatValue*2f)));
			break;
			
		case Main.Method.discSpawn:  
			EditorGUILayout.PropertyField(circleRadius_Prop); 
			EditorGUILayout.IntSlider (numberOfDiscRows_Prop, 0, (int)((planeSize_Prop.floatValue*5-circleRadius_Prop.floatValue)/(avoidanceRadius_Prop.floatValue*2f)));
			break;

		case Main.Method.continuousSpawn:     
			EditorGUILayout.PropertyField (numberOfAgents_Prop);
			EditorGUILayout.PropertyField (continousSpawnRate_Prop);
			EditorGUILayout.PropertyField (percentOfGroupedAgents_Prop);
			break;

		case Main.Method.areaSpawn:
			EditorGUILayout.PropertyField (rowAmount_Prop);
			EditorGUILayout.PropertyField (rowLength_Prop);
			EditorGUILayout.PropertyField (percentOfGroupedAgents_Prop);
			break;
		}

		EditorGUILayout.PropertyField(lcpsolver_Prop);
		EditorGUILayout.PropertyField (solverIterations_Prop);
		EditorGUILayout.PropertyField (lcpsolverEpsilon_Prop);
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(visualizeDensity_Prop);
		EditorGUILayout.PropertyField(visualizeVelocity_Prop);
		EditorGUILayout.PropertyField(visibleMap_Prop);
		EditorGUILayout.PropertyField(walkBack_Prop);
		EditorGUILayout.PropertyField(skip_Prop);
		EditorGUILayout.PropertyField (handleCollison_Prop);
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(avoidanceRadius_Prop);
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(agentPrefab_Prop);
		EditorGUILayout.PropertyField (agentPrefabGroup_Prop);
		EditorGUILayout.PropertyField(gridPrefab_Prop);
		EditorGUILayout.PropertyField(mapGen_Prop);
		EditorGUILayout.PropertyField(planePrefab_Prop);
		EditorGUILayout.PropertyField(spawnerPrefab_Prop);


		serializedObject.ApplyModifiedProperties ();
	}
}