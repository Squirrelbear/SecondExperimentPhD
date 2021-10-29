using System.Collections;
using System;

public class WaveCommand {

    public float timeToWait;
    public int unitIDToSpawn;
    
    public WaveCommand(int unitIDToSpawn, float timeToWait)
    {
        this.unitIDToSpawn = unitIDToSpawn;
        this.timeToWait = timeToWait;
    }

    public static WaveCommand getWaveCommand(string command)
    {
        string[] splitData = command.Split(':');

        float timeToWait;
        try
        {
            timeToWait = float.Parse(splitData[0]);
        }
        catch (Exception)
        {
            timeToWait = -2;
        }

        int unitIDToSpawn;
        try
        {
            unitIDToSpawn = int.Parse(splitData[1]);
        }
        catch (Exception)
        { 
            unitIDToSpawn = -2; 
        }
        

        return new WaveCommand(unitIDToSpawn, timeToWait);
    }
}
