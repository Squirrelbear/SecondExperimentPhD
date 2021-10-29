using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

[Serializable]
public class AutoSave {

    //public ExperimentState.GameState gameState;
    [Serializable]
    public struct AutoSaveProperty
    {
        //[NonSerializedAttribute]
        public string propertyName, propertyValue;

        public AutoSaveProperty(string propertyName, string propertyValue) 
        {
            this.propertyName = propertyName;
            this.propertyValue = propertyValue;
        }

        public void setValue(string newValue)
        {
            this.propertyValue = newValue;
        }

        /*public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("PropertyName",propertyName);
            info.AddValue("PropertyValue", propertyValue);
        }

        public AutoSaveProperty(SerializationInfo info, StreamingContext ctxt)
        {
            propertyName = (string)info.GetValue("PropertyName", typeof(string));
            propertyValue = (string)info.GetValue("PropertyValue", typeof(string));
        }*/
    }

    [NonSerializedAttribute]
    public List<AutoSaveProperty> properties;

	public AutoSave()
    {
        properties = new List<AutoSaveProperty>();
    }

    public void loadLastAutoSave()
    {
        loadAutoSave("last.autosave");
    }

    public bool loadAutoSave(string fileName)
    {
        try
        {
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();

                List<AutoSaveProperty> autosave = (List<AutoSaveProperty>)bin.Deserialize(stream);
                if (autosave != null)
                {
                    properties = autosave;
                    ErrorLog.logData("AutoSave successfully loaded: " + fileName,false);
                }
                else
                {
                    ErrorLog.logData("ERROR: Null autosave.");
                }
            }
        }
        catch (Exception e)
        {
            ErrorLog.logData("ERROR LOADING AUTOSAVE: " + fileName + ": " + e.Message);
            properties = new List<AutoSaveProperty>();
            return false;
        }
        return true;
    }

    public static void saveAutoSave(AutoSave autoSave)
    {
        try
        {
            using (Stream stream = File.Open("last.autosave", FileMode.Create))
            {
                //XmlSerializer x = new XmlSerializer(typeof(ReplayEvent));
                //x.Serialize(stream, replayEvents);

                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, autoSave.properties);
                ErrorLog.logData("Autosave Created.",false);
                foreach(AutoSaveProperty p in autoSave.properties)
                {
                    ErrorLog.logData(p.propertyName + ": " + p.propertyValue,false);
                }
            }
        }
        catch (Exception e)
        {
            ErrorLog.logData("Error generating Auto Save: " + e.Message);
        }
    }

    public void saveAs(string filename)
    {
        try
        {
            using (Stream stream = File.Open(filename, FileMode.Create))
            {
                //XmlSerializer x = new XmlSerializer(typeof(ReplayEvent));
                //x.Serialize(stream, replayEvents);

                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, properties);

            }
        }
        catch (IOException)
        {
            ErrorLog.logData("Error saving auto log to: " + filename);
        }
    }

    public void setProperty(string propertyName, string propertyValue)
    {
        //bool found = false;
        for (int i = 0; i < properties.Count; i++)
        {
            if (properties[i].propertyName.Equals(propertyName))
            {
                properties.RemoveAt(i);
                //found = true;
                break;
            }
        }

        properties.Add(new AutoSaveProperty(propertyName, propertyValue));
    }

    public string getPropertyValue(string propertyName)
    {
        for (int i = 0; i < properties.Count; i++)
        {
            if (properties[i].propertyName.Equals(propertyName))
            {
                return properties[i].propertyValue;
            }
        }
        return "";
    }
}
