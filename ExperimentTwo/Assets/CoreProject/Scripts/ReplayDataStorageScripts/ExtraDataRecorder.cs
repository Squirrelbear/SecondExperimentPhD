using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

public class ExtraDataRecorder
{
    public static ExtraDataRecorder singleton = null;

    public struct ExtraDataCollection
    {
        public List<string> dataEvents;

        public string collectionName;
        public int collectionID;

        public ExtraDataCollection(string collectionName, int collectionID)
        {
            dataEvents = new List<string>();

            this.collectionID = collectionID;
            this.collectionName = collectionName;
        }

        public void logData(string data, bool showDateTime)
        {
            if (showDateTime)
            {
                string now = DateTime.Now.ToString();
                logData(now + "," + data);
            }
            else
            {
                logData(data);
            }
        }

        public void logData(string data)
        {
            dataEvents.Add(data);
        }
    }

    public List<ExtraDataCollection> dataCollections;
    public string fName = null;

    public ExtraDataRecorder()
    {
        dataCollections = new List<ExtraDataCollection>();
    }

    public static ExtraDataRecorder getSingleton()
    {
        if (singleton != null)
        {
            return singleton;
        }
        else
        {
            singleton = new ExtraDataRecorder();
            return singleton;
        }
    }

    public int getDataCollectionID(string collectionName)
    {
        for(int i = 0; i < dataCollections.Count; i++)
        {
            if (dataCollections[i].collectionName.Equals(collectionName))
            {
                return i;
            }
        }

        ExtraDataCollection newCollection = new ExtraDataCollection(collectionName, dataCollections.Count);
        dataCollections.Add(newCollection);
        return newCollection.collectionID;
    }

    public ExtraDataCollection getDataCollection(string collectionName)
    {
        for (int i = 0; i < dataCollections.Count; i++)
        {
            if (dataCollections[i].collectionName.Equals(collectionName))
            {
                return dataCollections[i];
            }
        }

        ExtraDataCollection newCollection = new ExtraDataCollection(collectionName, dataCollections.Count);
        dataCollections.Add(newCollection);
        return newCollection;
    }

    public void logData(int collectionID, string data, bool showDateTime)
    {
        if (collectionID < dataCollections.Count)
        {
            dataCollections[collectionID].logData(data, showDateTime);
        }
        else
        {
            ErrorLog.logData("ERROR: CollectionID was out of valid range.\n" + collectionID + "\n" + data);
        }
    }

    public void setFileName(string fName)
    {
        this.fName = fName;
    }

    public void save()
    {
        if (fName == null || fName.Length == 0)
        {
            return;
        }

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(fName))
        {
            foreach (ExtraDataCollection d in dataCollections)
            {
                file.WriteLine("BEGIN " + d.collectionID + ": " + d.collectionName);
                foreach (string line in d.dataEvents)
                {
                    file.WriteLine(line);
                }
                file.WriteLine("END " + d.collectionID + ": " + d.collectionName);
                file.WriteLine();
                file.WriteLine();
            }
        }
    }

    /*void OnApplicationQuit()
    {
        GameObject oculus = GameObject.Find("OVRCameraRig");
        bool usingOculus;
        if (oculus == null)
        {
            usingOculus = false;
        }
        else
        {
            usingOculus = oculus.activeSelf;
        }
        if (saveFile.Equals("") || !usingOculus)
        {
            return;
        }

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(saveFile))
        {
            foreach (string line in replayEvents)
            {
                file.WriteLine(line);
            }
        }
    }*/
}
