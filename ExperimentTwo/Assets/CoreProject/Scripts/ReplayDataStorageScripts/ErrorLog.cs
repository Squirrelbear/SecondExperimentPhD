using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ErrorLog {
    private static List<string> errorLog = null;
    private static string fName = null;

    public ErrorLog()
    {
        if(errorLog == null) {
            errorLog = new List<string>();
        }
    }

    public static void logData(string data, bool showInUnity = true)
    {
        if(errorLog == null) {
            errorLog = new List<string>();
        } 

        string nowData = DateTime.Now.ToString() + " " + data;
        errorLog.Add(nowData);
        if (showInUnity)
        {
            MonoBehaviour.print(nowData);
        }
    }

    public static void setFileName(string newFName)
    {
        fName = newFName;
    }

    public static void save()
    {
        if (errorLog == null)
        {
            return;
        }

        using (System.IO.StreamWriter file = new System.IO.StreamWriter("lastlog.log"))
        {
            foreach (string line in errorLog)
            {
                file.WriteLine(line);
            }
        }

        if (fName == null || fName.Length == 0)
        {
            return;
        }

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(fName))
        {
            foreach (string line in errorLog)
            {
                file.WriteLine(line);
            }
        }
    }
}
