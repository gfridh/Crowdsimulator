using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class CsvWriter {

    private List<string[]> rowData = new List<string[]>();


    public void CreateHeaders() {
        string[] rowDataHeaders = new string[4];
        rowDataHeaders[0] = "GroupSize";
        rowDataHeaders[1] = "GroupOrientation";
        rowDataHeaders[2] = "DistanceFromGroup";
        rowDataHeaders[3] = "WelcomedFactor";
        rowData.Add(rowDataHeaders);
    }

    public void SaveRow(GroupSettings settings, float distance, int welcomedFactor) {
        Debug.Log("Recording Data: GroupSize " + settings.InterGroupDistance + ", GroupOrientation " + settings.OrientationVariance + ", Distance " + distance + ",  WelcomedFactor " + welcomedFactor);

        string[] rowData = new string[4];
        rowData[0] = settings.InterGroupDistance.ToString();
        rowData[1] = settings.OrientationVariance.ToString();
        rowData[2] = distance.ToString();
        rowData[3] = welcomedFactor.ToString();
        this.rowData.Add(rowData);
    }

    public void WriteToFile() {
        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++) {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ";";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));


        string filePath = getPath();

        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }


    private string getPath() {
#if UNITY_EDITOR
        return Application.dataPath + "/CSV/" + "testData" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".csv";
#elif UNITY_ANDROID
        return Application.persistentDataPath+"Saved_data.csv";
#elif UNITY_IPHONE
        return Application.persistentDataPath+"/"+"Saved_data.csv";
#else
        return Application.dataPath +"/"+"Saved_data.csv";
#endif
    }
}
