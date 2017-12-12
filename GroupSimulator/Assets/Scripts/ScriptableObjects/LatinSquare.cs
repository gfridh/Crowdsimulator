using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName =("Data/Latin Square"))]
public class LatinSquare : ScriptableObject {

    [SerializeField] private List<ListWrapper> groups = new List<ListWrapper>();
    private List<ListWrapper> tempGroup;

    [SerializeField, Range(1, 5)] private int multiplyGroups = 1;

    private int[,] grid;

    private int matrixIndex = 0;

    public void GenerateLatinSquare() {
        matrixIndex = 0;
        tempGroup = new List<ListWrapper>();

        // multiply groups here
        var groupCount = groups.Count;
        for (int i = 0; i < groupCount; i++) {
            var group = groups[i].list;
            var listCount = group.Count;
            tempGroup.Add(new ListWrapper());

            for (int r = 0; r < multiplyGroups; r++) {
                for (int j = 0; j < listCount; j++) {
                    tempGroup[i].list.Add(group[j]);
                }
            }
        }

        // calculate latin square 
        int row = 0, col = 0;
        grid = new int[tempGroup[0].list.Count, tempGroup.Count];
        // setting up rows and columns
        for (row = 0; row < grid.GetLength(0); row++) {
            for (col = 0; col < grid.GetLength(1); col++) {

                while (true) {
                    grid[row, col] = Random.Range(0, 4);

                    if (col == 0 && row > 0 && grid[row, col] == grid[row - 1, grid.GetLength(1) - 1]) {
                        continue;
                    } else {
                        break;
                    }
                }

                //for loop to check rows for repeats
                for (int c = 0; c < col; c++) {
                    // if there is repeat go back a column
                    if (grid[row, col] == grid[row, c]) {
                        col--;
                        break;
                    }
                }
            }
        }
    }

    public GroupSettings GetNextScenario() {
        // Here we take the index
        var index = grid[Mathf.FloorToInt(matrixIndex / grid.GetLength(1)), matrixIndex % grid.GetLength(1)];

        // Get it from the list and remove it 
        var otherIndex = Random.Range(0, tempGroup[index].list.Count);

        var settings = tempGroup[index].list[otherIndex];

        // Remove it here
        tempGroup[index].list.RemoveAt(otherIndex);

        matrixIndex++;

        // Return the element
        return settings;
    }

    public int TestLeft() {
        return (tempGroup.Count * groups[0].list.Count * multiplyGroups) - matrixIndex;
    }
}
