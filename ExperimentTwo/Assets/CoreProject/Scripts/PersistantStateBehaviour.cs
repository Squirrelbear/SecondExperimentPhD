using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PersistantStateBehaviour : MonoBehaviour
{

    private static bool created;
    public ReplayDatabase eventDatabase;
    public List<string> extraData = new List<string>();
    public bool databaseCreated;
    public int nextInputMode = 0;
    public bool saveExtra;

    void Awake()
    {
        if (!created)
        { // this is the first instance - make it persist 
            DontDestroyOnLoad(this.gameObject);
            created = true;
        }
        else
        { // this must be a duplicate from a scene reload - DESTROY! 
            Destroy(this.gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public ReplayDatabase configDatabaseNew()
    {
        if (databaseCreated) return eventDatabase;
        eventDatabase = new ReplayDatabase();
        databaseCreated = true;
        return eventDatabase;
    }

    public int getNextInputMode()
    {
        int nextMode = nextInputMode;
        nextInputMode++;
        return nextMode;
    }

    public void enableSaveExtra()
    {
        saveExtra = true;
    }

    public List<string> getExtraDataRef()
    {
        return extraData;
    }

    void OnApplicationQuit()
    {
        if (saveExtra)
        {
            using (StreamWriter file = new StreamWriter(eventDatabase.getCreationData() + "_extra.dat"))
            {
                foreach (string line in extraData)
                {
                    file.WriteLine(line);
                }
            }
        }
    }
}
