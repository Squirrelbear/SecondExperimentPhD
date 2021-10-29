using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Settings {

    private static Settings singleton = null;

    public struct SettingProperty
    {
        public string propertyName, propertyValue;

        public SettingProperty(string propertyName, string propertyValue)
        {
            this.propertyName = propertyName;
            this.propertyValue = propertyValue;
        }

        public void setValue(string newValue)
        {
            this.propertyValue = newValue;
        }
    }

    public List<SettingProperty> properties;

    public Settings()
    {
        loadSettings("Settings.txt");
    }

    public static Settings getSingleton()
    {
        if (singleton == null)
        {
            singleton = new Settings();
        }
        return singleton;
    }

    public bool loadSettings(string fileName)
    {
        properties = new List<SettingProperty>();
        try
        {
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
               new System.IO.StreamReader(fileName);
            while ((line = file.ReadLine()) != null)
            {
                string[] splitData = line.Split(new char[] { ' ' }, 2);
                setProperty(splitData[0], splitData[1]);
            }

            file.Close();
        }
        catch (Exception e)
        {
            ErrorLog.logData("ERROR LOADING SETTINGS: " + fileName + ": " + e.Message);
            generateDefaultConfig(ExperimentState.GAMEVERSION, fileName);
            return false;
        }
        return true;
    }

    public void generateDefaultConfig(int versionID, string fName)
    {
        properties = new List<SettingProperty>();

        setProperty("GameVersion", versionID+"");
        setProperty("StartingResources", "100");
        setProperty("PreGameBonusResources", "20");
        setProperty("PostRoundBonusResources", "20");
        setProperty("BasicTowerBaseStats", "20,20,0.5");
        setProperty("FrostTowerBaseStats", "0.6,15,1");
        setProperty("SwarmTowerBaseStats", "20,20,0.2");
        setProperty("ExplosiveTowerBaseStats", "35,20,0.8");
        setProperty("WaveIntervalTime", "60");
        setProperty("TowerCosts", "30,40,50,50");

        // Wave definition structure:
        // timeTillSpawn:unitID; ... timeTillSpawn:unitID; -1:-1
        // -1:-1 = end of wave
        // 0 = basic, 1 = dangerous, 2 = fast, 3 = boss

        string FIRSTWAVEDEFINITION = "1:0;1:0;1:0;1:0;1:0;-1:-1;"                                              // 5 basic
                                                 + "1:0;1:0;1:2;1:2;1:0;1:0;1:2;1:2;-1:-1;"                                  // (2 basic, 2 fast) * 2
                                                 + "1:0;1:0;1:0;1:0;1:2;1:2;1:2;1:2;1:0;1:0;1:0;1:0;1:2;1:2;1:2;1:2;-1:-1";  // (4 basic, 4 fast) * 2

        string SECONDWAVEDEFINTION = "1:1;1:1;1:1;1:1;1:1;-1:-1;"                                              // 5 dangerous
                                                 + "1:2;1:2;1:1;1:2;1:2;1:1;1:2;1:2;1:1;-1:-1;"                              // (2 fast, 1 dangerous) * 3
                                                 + "1:0;1:1;1:2;1:0;1:1;1:2;1:0;1:1;1:2;-1:-1";                              // (1 basic, 1 dangerous, 1 fast) * 3

        string THIRDWAVEDEFINTION = "1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;-1:-1;"                   // 12 fast
                                                 + "1:0;1:0;1:2;1:2;1:2;1:2;1:0;1:0;1:2;1:2;1:2;1:2;1:1;1:1;1:1;1:1;-1:-1;"  // (2 basic, 4 fast) * 2, 5 dangerous
                                                 + "1:0;1:0;1:1;1:1;1:2;1:2;1:0;1:0;1:1;1:1;1:2;1:2;1:0;1:0;1:1;1:1;1:2;1:2;5:3;-1:-1";  // (2 basic, 2 dangerous, 2 fast) * 3, 1 boss

        // Boss, Dangerous, Boss, Dangerous, Boss, Boss, Dangerous, Boss, Dangerous, Boss
        string BOSSWAVEDEFINTION = "1.5:3;1.5:1;1.5:3;1.5:1;1.5:3;1.5:3;1.5:1;1.5:3;1.5:1;1.5:3;-1:-1";

        setProperty("FirstWaveDefinition", FIRSTWAVEDEFINITION);
        setProperty("SecondWaveDefinition", SECONDWAVEDEFINTION);
        setProperty("ThirdWaveDefinition", THIRDWAVEDEFINTION);
        setProperty("BossWaveDefinition", BOSSWAVEDEFINTION);

        setProperty("SkipToStage", "0");

        setProperty("ReplayFile", "null");

        if (fName != null)
        {
            saveAs(fName);
        }
    }

    public void saveAs(string fName)
    {
        try
        {
            if (fName == null || fName.Length == 0)
            {
                return;
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fName))
            {
                foreach (SettingProperty d in properties)
                {
                    file.WriteLine(d.propertyName + " " + d.propertyValue);
                }
            }
        }
        catch (System.Exception e)
        {
            ErrorLog.logData("Error saving config to: " + fName + "\n" + e.Message);
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

        properties.Add(new SettingProperty(propertyName, propertyValue));
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

    public static bool parseIntSetting(string settingName, ref int output)
    {
        Settings s = Settings.getSingleton();
        string settingData = s.getPropertyValue(settingName);

        if (settingData != null && settingData.Length != 0)
        {
            try
            {
                int amount = int.Parse(settingData);
                output = amount;
                return true;
            }
            catch (Exception)
            {
                ErrorLog.logData("Error loading setting: " + settingName + ". Invalid Format.");
            }
        }
        else
        {
            ErrorLog.logData("Error loading setting: " + settingName);
        }
        return false;
    }

    public static bool parseFloatSetting(string settingName, ref float output)
    {
        Settings s = Settings.getSingleton();
        string settingData = s.getPropertyValue(settingName);

        if (settingData != null && settingData.Length != 0)
        {
            try
            {
                float amount = float.Parse(settingData);
                output = amount;
                return true;
            }
            catch (Exception)
            {
                ErrorLog.logData("Error loading setting: " + settingName + ". Invalid Format.");
            }
        }
        else
        {
            ErrorLog.logData("Error loading setting: " + settingName);
        }
        return false;
    }

    public static bool parseFloatArray(string settingName, int expectedElements, ref float[] output)
    {
        Settings s = Settings.getSingleton();
        string settingData = s.getPropertyValue(settingName);

        string[] sSplit = settingData.Split(',');

        if (sSplit.Length != expectedElements)
        {
            ErrorLog.logData("Error loading setting: " + settingName + ". ElementCountMismatch");
            return false;
        }

        for (int i = 0; i < expectedElements; i++)
        {
            if (sSplit[i] != null && sSplit[i].Length != 0)
            {
                try
                {
                    float amount = float.Parse(sSplit[i]);
                    output[i] = amount;
                }
                catch (Exception)
                {
                    ErrorLog.logData("Error loading setting: " + settingName + ". Invalid Format. " + i);
                    return false;
                }
            }
            else
            {
                ErrorLog.logData("Error loading setting: " + settingName + " " + i);
                return false;
            }
        }
        return true;
    }

    public static bool parseIntArray(string settingName, int expectedElements, ref int[] output)
    {
        Settings s = Settings.getSingleton();
        string settingData = s.getPropertyValue(settingName);

        string[] sSplit = settingData.Split(',');

        if (sSplit.Length != expectedElements)
        {
            ErrorLog.logData("Error loading setting: " + settingName + ". ElementCountMismatch");
            return false;
        }

        for (int i = 0; i < expectedElements; i++)
        {
            if (sSplit[i] != null && sSplit[i].Length != 0)
            {
                try
                {
                    int amount = int.Parse(sSplit[i]);
                    output[i] = amount;
                }
                catch (Exception)
                {
                    ErrorLog.logData("Error loading setting: " + settingName + ". Invalid Format. " + i);
                    return false;
                }
            }
            else
            {
                ErrorLog.logData("Error loading setting: " + settingName + " " + i);
                return false;
            }
        }
        return true;
    }
}
