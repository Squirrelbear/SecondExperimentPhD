using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AISpawner : MonoBehaviour {

    public GameObject[] prefabAI;
    public bool spawning = false;
    public GameObject firstNavNode;

    private Transform spawnAt;
    public bool spawningWave = false;
    //public float timeBetweenSpawn = 0.3f;
    public float timeBetweenWave = 10;

    private const string SAMPLEWAVEDEFINTION = "1:0;1:0;1:0;1:0;1:0;1:0;1:0;-1:-1;"
                                  + "1.2:1;1.2:1;1.2:1;1.2:1;1.2:1;1.2:1;1.2:1;1:2;1:2;1:2;1:2;2:3;-1:-1";
        
        //"0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;0.01:3;";
        
        /*"1:0;1:0;1:0;1:0;1:0;1:0;1:0;-1:-1;"
                                  + "1.2:1;1.2:1;1.2:1;1.2:1;1.2:1;1.2:1;1.2:1;1:2;1:2;1:2;1:2;2:3;-1:-1";*/
    public int currentWave;
    public bool repeatWaves = true;
    public List<List<WaveCommand>> waveData;
    public int dataIndex;

    public List<GameObject> currentWaveEnemies;
    public List<GameObject> waitingSpawnEnemies;

    public GameObject spawnParticlePrefab;
    float doNothingTillZero = 0;

    private string repeatWaveDefinition = "";

	// Use this for initialization
	void Start () {
        spawnAt = transform;
        spawning = false;
        //parseWaveData(SAMPLEWAVEDEFINTION);
        //spawnWave();
        //StartCoroutine(beginSpawning());
	}

	// Update is called once per frame
    public void update(float deltaTime)
    {
        // Update all currently living enemy units
        foreach (GameObject enemy in currentWaveEnemies)
        {
            if (enemy != null)
            {
                enemy.GetComponent<AIWayfinder>().update(deltaTime);
            }
        }

        doNothingTillZero -= deltaTime;
        if (doNothingTillZero > 0) return;
        doNothingTillZero = 0;

        if (spawning)
        {
            if (currentWave >= waveData.Count) // End of all waves
            {
                ErrorLog.logData("End of Wave Detected",false);
                if (repeatWaves)
                {
                    ErrorLog.logData("Beginning Repeat from Beginning",false);
                    parseWaveData(repeatWaveDefinition);
                    currentWave = 0;
                    spawningWave = false;
                    spawnWave();
                }
                else
                {
                    ErrorLog.logData("Terminating Wave Spawning",false);
                    spawning = false;
                    spawningWave = false;
                }
            }
            else if (dataIndex >= waveData[currentWave].Count) // End of current wave
            {
                spawningWave = false;
                for (int i = 0; i < currentWaveEnemies.Count; i++)
                {
                    if (currentWaveEnemies[i] == null)
                    {
                        currentWaveEnemies.RemoveAt(i);
                    }
                }
                if (currentWaveEnemies.Count == 0 && waitingSpawnEnemies.Count == 0)
                {
                    currentWave++;
                    spawnWave();
                    doNothingTillZero = timeBetweenWave;
                }
            }
            else if (spawningWave)
            {
                if (waveData[currentWave][dataIndex].timeToWait > 0)
                {
                    doNothingTillZero = waveData[currentWave][dataIndex].timeToWait;
                    waveData[currentWave][dataIndex].timeToWait = 0;
                    return;
                }

                if (waveData[currentWave][dataIndex].unitIDToSpawn >= 0 && waveData[currentWave][dataIndex].unitIDToSpawn < prefabAI.Length)
                {
                    waitingSpawnEnemies[0].SetActive(true);
                    currentWaveEnemies.Add(waitingSpawnEnemies[0]);
                    waitingSpawnEnemies.RemoveAt(0);
                    if (spawnParticlePrefab != null)
                    {
                        //print("Spawning");
                        Instantiate(spawnParticlePrefab, spawnAt.position, spawnAt.rotation);
                    }
                }
                dataIndex++;
            }
        }
	}

    public void spawnWave(string waveDefinition)
    {
        ErrorLog.logData("Loading wave string: " + waveDefinition,false);
        parseWaveData(waveDefinition);
        spawnWave();
        spawning = true;
    }
    
    public string getWaveProgress()
    {
        if (spawning && currentWave < waveData.Count)
        {
            // Wave 1/4 Contains 10 Enemies
            return "Wave " + (currentWave + 1) + "/" + waveData.Count + " Contains " + waveData[currentWave].Count + " Enemies";
        }
        else
        {
            return "";
        }
    }

    private GameObject spawnUnit(int unitID)
    {
        if (prefabAI.Length > unitID && unitID >= 0)
        {
            GameObject aiUnit = (GameObject)Instantiate(prefabAI[unitID], spawnAt.position, spawnAt.rotation);
            AIWayfinder ai = aiUnit.GetComponent<AIWayfinder>();
            ai.wayPoint = firstNavNode;
            //print("Unit Spawned: " + ai.unitName);
            return aiUnit;
        }
        return null;
    }

    private void spawnWave()
    {
        currentWaveEnemies = new List<GameObject>();
        waitingSpawnEnemies = new List<GameObject>();

        if (currentWave >= waveData.Count)
        {
            return;
        }

        foreach(WaveCommand cmd in waveData[currentWave])
        {
            //print("Running command: " + cmd.unitIDToSpawn + "    " + cmd.timeToWait);
            GameObject newUnit = spawnUnit(cmd.unitIDToSpawn);
            if (newUnit != null)
            {
                newUnit.SetActive(false);
                waitingSpawnEnemies.Add(newUnit);
            }
        }

        spawningWave = true;
        dataIndex = 0;
    }

    private void parseWaveData(string waveDefinition)
    {
        repeatWaveDefinition = waveDefinition;
        waveData = new List<List<WaveCommand>>();
        currentWave = 0;
        if (waveDefinition.Length > 0)
        {
            string[] allEvents = waveDefinition.Split(';');
            //print("Events detected: " + allEvents.Length);
            List<WaveCommand> curWave = new List<WaveCommand>();
            foreach(string evnt in allEvents)
            {
                WaveCommand curEvent = WaveCommand.getWaveCommand(evnt);
                if (curEvent.timeToWait == -1 && curEvent.unitIDToSpawn == -1)
                {
                    waveData.Add(curWave);
                    curWave = new List<WaveCommand>();
                }
                else
                {
                    curWave.Add(curEvent);
                }
            }
            if (curWave.Count > 0)
            {
                waveData.Add(curWave);
            }
           // print("Waves detected: " + waveData.Count);
        }
        else
        {
            ErrorLog.logData("ERROR: Wave defintion not set!");
        }
    }

    // Original co-routine version. Do not use.
    private IEnumerator beginSpawning()
    {
        /*while(spawning)
        {
            if (spawningWave)
            {
                for (int i = 0; i < 5; i++)
                {
                    yield return new WaitForSeconds(timeBetweenSpawn);
                    spawnUnit(0);
                }
                spawningWave = false;
            }
            yield return new WaitForSeconds(timeBetweenWave);
            spawnWave();
        }*/

        while (spawning)
        {
            if (currentWave >= waveData.Count) // End of all waves
            {
                print("End of Wave Detected");
                if (repeatWaves)
                {
                    print("Beginning Repeat from Beginning");
                    currentWave = 0;
                    spawningWave = false;
                    spawnWave();
                }
                else
                {
                    print("Terminating Wave Spawning");
                    spawning = false;
                    spawningWave = false;
                }
            }
            else if (dataIndex >= waveData[currentWave].Count) // End of current wave
            {
                spawningWave = false;
                for (int i = 0; i < currentWaveEnemies.Count; i++)
                {
                    if (currentWaveEnemies[i] == null)
                    {
                        currentWaveEnemies.RemoveAt(i);
                    }
                }
                if (currentWaveEnemies.Count == 0 && waitingSpawnEnemies.Count == 0)
                {
                    currentWave++;
                    spawnWave();
                }
            }
            else if (spawningWave)
            {
                if (waveData[currentWave][dataIndex].timeToWait > 0)
                {
                    yield return new WaitForSeconds(waveData[currentWave][dataIndex].timeToWait);
                }

                if (waveData[currentWave][dataIndex].unitIDToSpawn >= 0 && waveData[currentWave][dataIndex].unitIDToSpawn < prefabAI.Length)
                {
                    waitingSpawnEnemies[0].SetActive(true);
                    currentWaveEnemies.Add(waitingSpawnEnemies[0]);
                    waitingSpawnEnemies.RemoveAt(0);
                    if (spawnParticlePrefab != null)
                    {
                        //print("Spawning");
                        Instantiate(spawnParticlePrefab, spawnAt.position, spawnAt.rotation);
                    }
                }
                dataIndex++;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
