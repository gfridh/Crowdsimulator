using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

public class Grid : MonoBehaviour {

	public Cell cellPrefab;
	public VelocityNode velocityNodePrefab;

	internal static Grid instance; //Alive instance of this grid (should only be one)

	internal static float maxDensity; 
	internal float cellLength;
	internal float agentMaxSpeed;
	internal float agentAvoidanceRadius;
	internal float dt;
	internal float ringDiameter;
	internal int cellsPerRow;
	internal Main.LCPSolutioner solver;
	internal LCPSolver mprgpSolver; //LCP solver instance
	internal LCPSolverMIC mprgpmicSolver;
	internal LCPSolverProj psorSolver;
	internal int solverMaxIterations;
	internal double solverEpsilon;

	public Cell[,] cellMatrix;
	public VelocityNode[,] xEdgeVelocityNodeMatrix; //On vertical faces
	public VelocityNode[,] zEdgeVelocityNodeMatrix; //On horizontal faces
	internal float[,] density; //Per cell center 
	public float[,] xEdgeDensity;
	public float[,] zEdgeDensity;
	public float[,] xEdgeVelocity;
	public float[,] zEdgeVelocity;
	public float[,] xMeanVelocity;
	public float[,] zMeanVelocity;

	public double[,] matrixArray; //Helping array to create A of LCP
	public double[] xArray, lArray, bArray; //LCP arrays
	public List<List<LCPSolver.denseMatrixNode>> AArray; //Actual sparse matrix carrier of A
	internal List<List<List<int>>> neighMatrix; //Neighbourhood grid for pair-wise collision
	internal int neighbourBins; //Length of neighMatrix
	internal float lenOfBin; //Length of each "bin" in neighMatrix


	internal MapGen mapGen; //Reference to mapgen in main

	//Flags
	internal bool showSplattedDensity;
	internal bool showSplattedVelocity;
	internal bool walkBack;
	internal bool skipNodeIfSeeNext;
	internal bool colHandler;

	/**
	 * Initialize this grid with variables set from Main as well as variables concerning the grid
	 * Note the initializations of the matrices/vectors and their variable sizes
	 **/ 
	public void initGrid(Vector2 xMinMax, Vector2 zMinMax, float alpha, float agentAvoidanceRadius) {
		//Calculation of maximum density based on min-d for agents
		maxDensity = (2f*alpha)/(Mathf.Sqrt(3f)*(Mathf.Pow(agentAvoidanceRadius, 2)));
	
		float gridLength = Mathf.Max(xMinMax.y - xMinMax.x, zMinMax.y - zMinMax.x);
		cellLength = gridLength / cellsPerRow;
		float cellScale = showSplattedDensity ? cellLength : 0f;

		cellMatrix = new Cell[cellsPerRow, cellsPerRow]; //Square grid
		xEdgeVelocityNodeMatrix = new VelocityNode[cellsPerRow,cellsPerRow+1];
		zEdgeVelocityNodeMatrix = new VelocityNode[cellsPerRow+1,cellsPerRow];
		density = new float[cellsPerRow, cellsPerRow]; //Hold current density over all cells
		xEdgeDensity = new float[cellsPerRow, cellsPerRow+1];
		zEdgeDensity = new float[cellsPerRow+1, cellsPerRow];
		xEdgeVelocity = new float[cellsPerRow, cellsPerRow+1];
		zEdgeVelocity = new float[cellsPerRow+1, cellsPerRow];
		xMeanVelocity = new float[cellsPerRow, cellsPerRow];
		zMeanVelocity = new float[cellsPerRow, cellsPerRow];
		AArray = new List<List<LCPSolver.denseMatrixNode>> ();
		for (int i = 0; i < cellsPerRow * cellsPerRow; ++i) {
			AArray.Add (new List<LCPSolver.denseMatrixNode> ());
		}
		xArray = new double[cellsPerRow * cellsPerRow];
		bArray = new double[cellsPerRow * cellsPerRow];
		lArray = new double[cellsPerRow * cellsPerRow];

		//Create cells in this grid
		Vector3 cellPos = new Vector3(xMinMax.x + 0.5f*cellLength, 0.01f, zMinMax.x + 0.5f*cellLength);
		for (int i = 0; i < cellsPerRow; ++i) {
			for (int j = 0; j < cellsPerRow; ++j) {
				Cell cell = Instantiate (cellPrefab) as Cell;
				cell.transform.position = new Vector3 (cellPos.x + j * cellLength, cellPos.y, cellPos.z + i * cellLength);
				cell.transform.parent = transform;
				cell.transform.localScale = new Vector3(cellScale, 0.01f, cellScale);
				cell.setProperties (i, j, cellLength);
				cell.setVelocityNodes();
				cell.calculateAvailableArea ();
				cellMatrix [i, j] = cell;
			}
		}

		//Create neighmatrix
		neighMatrix = new List<List<List<int>>> ();

		for (int i = 0; i < neighbourBins; ++i) {
			neighMatrix.Add (new List<List<int>>());
			for (int j = 0; j < neighbourBins; ++j) {
				neighMatrix [i].Add (new List<int> ());
			}
		}
		lenOfBin = Mathf.Max((Main.xMinMax.y - Main.xMinMax.x), (Main.zMinMax.y - Main.zMinMax.x)) / neighbourBins;

		//LCP solver instances
		mprgpSolver = new LCPSolver (); 
		mprgpmicSolver = new LCPSolverMIC();
		psorSolver = new LCPSolverProj ();
	}
		
	/**
	 * Make each cell accumelate the density from agents and convert them to a continous form
	 **/ 
	internal void updateCellDensity () {
		for (int i = 0; i < cellsPerRow; ++i) {
			for (int j = 0; j < cellsPerRow; ++j) {
				cellMatrix [i, j].splatDensity ();
				if (showSplattedDensity) {
					cellMatrix [i, j].renderer.enabled = true;
					cellMatrix[i, j].setColor ();
				} else if(cellMatrix[i, j].renderer.enabled) {
					cellMatrix [i, j].renderer.enabled = false;
				}
			}
		}
	}

	/**
	 * Make each velocity contribute their density / velocity values to the continous form
	 **/ 
	internal void updateVelocityNodes() {
		for (int i = 0; i < cellsPerRow; ++i) {
			for (int j = 0; j < cellsPerRow; ++j) {
				xEdgeVelocityNodeMatrix [i, j].updateValues ();
				zEdgeVelocityNodeMatrix [i, j].updateValues ();

				if(i == cellsPerRow -1)
					zEdgeVelocityNodeMatrix [i+1, j].updateValues ();
				if(j == cellsPerRow -1)
					xEdgeVelocityNodeMatrix [i, j+1].updateValues ();
			}
		}
		if (showSplattedVelocity) {
			for (int i = 0; i < cellsPerRow; ++i) {
				for (int j = 0; j < cellsPerRow; ++j) {
					cellMatrix[i, j].drawVelocityField ();
				}
			}
		}
	}


	/**
	 * Set the mean velocity for each cell
	 **/ 
	internal void setMeanVelocities() {
		for (int i = 0; i < cellsPerRow; ++i) {
			for (int j = 0; j < cellsPerRow; ++j) {
				cellMatrix [i, j].setMeanVelocity ();
			}
		}
	}

	/**
	 * Solve the linear constraint problem, with an option "clamped" if b-values should be clamped to non-negatives
	 **/ 
	public void solveLCP(bool clamped) {
		//Refresh containers
		for (int i = 0; i < AArray.Count; ++i) {
			AArray [i].Clear ();
		}
		matrixArray = new double[cellsPerRow*cellsPerRow, cellsPerRow*cellsPerRow];

		//Calculate A and B matrices
		for (int i = 0; i < cellsPerRow; ++i) {
			for (int j = 0; j < cellsPerRow; ++j) {
				constructB (i, j, clamped);
				constructA (i, j, j + i * cellsPerRow);
			}
		}


		switch (solver) {
		case Main.LCPSolutioner.mprgp:
			xArray = mprgpSolver.LCPSolve (AArray, matrixArray, bArray, xArray, lArray);
			break;
		case Main.LCPSolutioner.mprgpmic0:
			xArray = mprgpmicSolver.LCPSolve (AArray, matrixArray, bArray, xArray, lArray);
			break;
		case Main.LCPSolutioner.psor:
			xArray = psorSolver.LCPSolve (AArray, matrixArray, bArray, xArray, lArray);
			break;

		default:
			Debug.LogError ("Error: Invalid solver selected");
			break;
		}
	}

	/**
	 * Construct the b-matrix index at n m with values optionally clamped to non-negative values
	 **/ 
	internal void constructB(int n, int m, bool clamped){
		double temp = (double)(cellMatrix [n, m].availableArea * maxDensity - density [n, m] 
				+ ((xEdgeDensity[n, m+1]*xEdgeVelocity[n,m+1]
				+ zEdgeDensity[n+1, m]*zEdgeVelocity[n+1, m]
				- xEdgeDensity[n, m]*xEdgeVelocity[n, m]
				- zEdgeDensity[n, m]*zEdgeVelocity[n, m])/cellLength)*dt);

		if (clamped && temp < 0) {
			temp = 0; 
		}

		bArray [n * cellsPerRow + m] = temp;
	}

	/**
	 * Construct the A-matrix index at n m. Do this for one puppet-matrix and one sparse matrix (same cost).
	 **/ 
	internal void constructA(int n, int m, int currentRow) {
		int startIndex = n * cellsPerRow + m;
		double cls = Mathf.Pow (cellLength, 2);
		double[] elem = new double[5]; 

		//Coeff for P_{n-1,m}
		if (n > 0) {
			elem[0] = (double)-(dt*zEdgeDensity[n,m]/cls);
		} else {
			elem[0] = 0;	
		}

		//Coeff for P_{n,m-1}
		if (m > 0) {
			elem[1] = (double)-(dt*xEdgeDensity[n,m]/cls); 
		} else {
			elem[1] = 0;	
		}

		//Coeff for P_{n,m}
		elem[2] = (double)(dt*(xEdgeDensity[n,m] + xEdgeDensity[n,m+1] + zEdgeDensity[n,m] + zEdgeDensity[n+1,m]))/cls;

		//Coeff for P_{n,m+1}
		if (m >= cellsPerRow-1) {
			elem[3] = 0;
		} else {
			elem[3] = (double)-(dt*xEdgeDensity[n,m+1]/cls);
		}

		//Coeff for P_{n+1,m}
		if (n >= cellsPerRow-1) {
			elem[4] = 0;
		} else {
			elem[4] = (double)-(dt*zEdgeDensity[n+1,m]/cls);
		} 

		if (elem [0] != 0) {
			if (matrixArray[startIndex-cellsPerRow, currentRow] != 0) {
				matrixArray[currentRow, startIndex-cellsPerRow] = matrixArray[startIndex-cellsPerRow, currentRow];
			} else {
				matrixArray[currentRow, startIndex-cellsPerRow] = elem [0];
			}
			LCPSolver.denseMatrixNode node = new LCPSolver.denseMatrixNode ();
			node.value = matrixArray [currentRow, startIndex - cellsPerRow]; node.colIndex = startIndex - cellsPerRow;
			AArray [currentRow].Add (node);
		}

		if (elem [1] != 0) {
			if (matrixArray[startIndex-1, currentRow] != 0) {
				matrixArray[currentRow, startIndex-1] = matrixArray[startIndex-1, currentRow];
			} else {
				matrixArray[currentRow, startIndex-1] = elem [1];
			}
			LCPSolver.denseMatrixNode node = new LCPSolver.denseMatrixNode ();
			node.value = matrixArray [currentRow, startIndex - 1]; node.colIndex = startIndex - 1;
			AArray [currentRow].Add (node);
		}

		matrixArray[currentRow, startIndex] = elem [2];
		LCPSolver.denseMatrixNode nn = new LCPSolver.denseMatrixNode ();
		nn.value = matrixArray [currentRow, startIndex]; nn.colIndex = startIndex;
		AArray [currentRow].Add (nn);

		if (elem [3] != 0) {
			if (matrixArray[startIndex+1, currentRow] != 0) {
				matrixArray[currentRow, startIndex+1] = matrixArray[startIndex+1, currentRow];
			} else {		
				matrixArray[currentRow, startIndex+1] = elem [3];
			}
			LCPSolver.denseMatrixNode node = new LCPSolver.denseMatrixNode ();
			node.value = matrixArray [currentRow, startIndex + 1]; node.colIndex = startIndex + 1;
			AArray [currentRow].Add (node);
		}

		if (elem [4] != 0) {
			if (matrixArray[startIndex+cellsPerRow, currentRow] != 0) {
				matrixArray[currentRow, startIndex+cellsPerRow] = matrixArray[startIndex+cellsPerRow, currentRow];
			} else {
				matrixArray[currentRow, startIndex+cellsPerRow] = elem [4];
			}
			LCPSolver.denseMatrixNode node = new LCPSolver.denseMatrixNode ();
			node.value = matrixArray [currentRow, startIndex + cellsPerRow]; node.colIndex = startIndex + cellsPerRow;
			AArray [currentRow].Add (node);
		}
	}

	/**
	 * Perform solution of LCP.
	 **/ 
	internal void PsolveRenormPsolve() {
		
		Grid.instance.solveLCP (true);

		for (int n = 0; n < cellsPerRow; n++) {
			for (int m = 0; m < cellsPerRow; m++) {
				zEdgeVelocityNodeMatrix[n,m].calculatePressureGradient();
				zEdgeVelocityNodeMatrix[n,m].pSolve();
				xEdgeVelocityNodeMatrix[n,m].calculatePressureGradient();
				xEdgeVelocityNodeMatrix[n,m].pSolve();
				if (n == cellsPerRow - 1) {
					zEdgeVelocityNodeMatrix[n+1,m].calculatePressureGradient();
					zEdgeVelocityNodeMatrix[n+1,m].pSolve();
				}
				if (m == cellsPerRow - 1) {
					xEdgeVelocityNodeMatrix[n,m+1].calculatePressureGradient();
					xEdgeVelocityNodeMatrix[n,m+1].pSolve();
				}
			}
		}
			
		for (int n = 0; n < cellsPerRow; n++) {
			for (int m = 0; m < cellsPerRow; m++) {
				zEdgeVelocityNodeMatrix [n, m].renorm ();
				xEdgeVelocityNodeMatrix[n,m].renorm ();
				if (n == cellsPerRow - 1) {
					zEdgeVelocityNodeMatrix[n+1,m].renorm ();
				}
				if (m == cellsPerRow - 1) {
					xEdgeVelocityNodeMatrix[n,m+1].renorm ();
				}
			}
		}

		Grid.instance.solveLCP (false); //Solve again with corrected, normalized velocities.

		for (int n = 0; n < cellsPerRow; n++) {
			for (int m = 0; m < cellsPerRow; m++) {
				zEdgeVelocityNodeMatrix[n,m].calculatePressureGradient();
				zEdgeVelocityNodeMatrix[n,m].pSolve();
				xEdgeVelocityNodeMatrix[n,m].calculatePressureGradient();
				xEdgeVelocityNodeMatrix[n,m].pSolve();
				if (n == cellsPerRow - 1) {
					zEdgeVelocityNodeMatrix[n+1,m].calculatePressureGradient();
					zEdgeVelocityNodeMatrix[n+1,m].pSolve();
				}
				if (m == cellsPerRow - 1) {
					xEdgeVelocityNodeMatrix[n,m+1].calculatePressureGradient();
					xEdgeVelocityNodeMatrix[n,m+1].pSolve();
				}
			}
		}
	}

	/**
	 * Handle pair-wise collision for a set of agents with given agent.
	 **/ 
	internal void handleCollision(int agent, int row, int col, ref List<Agent> agentList) {
		if (row < 0 || col < 0 || row >= neighbourBins || col >= neighbourBins)
			return;
		for(int i = 0; i < neighMatrix[row][col].Count; ++i) {
			int otherAgent = neighMatrix [row] [col] [i];
			if (agent == otherAgent)
				continue;

			Vector3 dis = agentList [agent].transform.position - agentList [otherAgent].transform.position;
			if (dis.magnitude < ringDiameter) {
				if (agent is SubgroupAgent && otherAgent is SubgroupAgent && (agent as SubgroupAgent).c.tag.Equals ((otherAgent as SubgroupAgent).c.tag) && dis.magnitude < ringDiameter / 2) {
					agentList [agent].collisionAvoidanceVelocity += dis.normalized * (ringDiameter/4 - dis.magnitude) * agentMaxSpeed; //Push away groupie a little
				} else {
					agentList [agent].collisionAvoidanceVelocity += dis.normalized * (ringDiameter - dis.magnitude) * agentMaxSpeed; //Push away
				}
			}
		}
	}

	/**
	 * Do pair-wise collision avoidance for a set of agents, with respect to surrounding columns and rows.
	 **/ 
	internal void collisionHandling(ref List<Agent> agentList) {

		calculateNeighborList (ref agentList);
		for (int i = 0; i < agentList.Count; ++i) {
			int row = (int)((agentList [i].transform.position.z - Main.zMinMax.x) / lenOfBin); 
			int column = (int)((agentList[i].transform.position.x - Main.xMinMax.x) / lenOfBin); 
			if (row < 0)
				row = 0; 
			if (column < 0)
				column = 0;
			if (row > neighbourBins - 1) {
				row = neighbourBins - 1;
			}
			if (column > neighbourBins - 1) {
				column = neighbourBins - 1;
			}
			handleCollision (i, row, column, ref agentList); //center
			handleCollision (i, row+1, column, ref agentList); //up
			handleCollision (i, row+1, column+1, ref agentList); //top right
			handleCollision (i, row, column+1, ref agentList); //right
			handleCollision (i, row-1, column+1, ref agentList); //bottom right
			handleCollision (i, row-1, column, ref agentList); //bottom
			handleCollision (i, row-1, column-1, ref agentList); //bottom left
			handleCollision (i, row, column-1, ref agentList); //left
			handleCollision (i, row-1, column-1, ref agentList); //top left
		}
	}

	/**
	 * For each agent, calculate its position in a neighborhood bin.
	 **/ 
	internal void calculateNeighborList(ref List<Agent> agents) {
		for (int i = 0; i < neighMatrix.Count; ++i) {
			for (int j = 0; j < neighMatrix [i].Count; ++j) {
				neighMatrix [i] [j].Clear ();
			}
		}

		for (int i = 0; i < agents.Count; ++i) {
			int row = (int)((agents[i].transform.position.z - Main.zMinMax.x)/lenOfBin); 
			int column = (int)((agents[i].transform.position.x - Main.xMinMax.x)/lenOfBin); 
			if (row < 0)
				row = 0; 
			if (column < 0)
				column = 0;
			if (row > neighbourBins - 1) {
				row = neighbourBins - 1;
			}
			if (column > neighbourBins- 1) {
				column = neighbourBins - 1;
			}
			neighMatrix [row] [column].Add (i);
		}
	}
}
