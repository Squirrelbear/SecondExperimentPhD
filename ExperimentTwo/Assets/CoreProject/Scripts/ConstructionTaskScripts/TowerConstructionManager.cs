using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TowerConstructionManager : MonoBehaviour {

    public float[,] towerBaseStats = new float[,] { { 20, 20, 0.5f }, { 0.6f, 15, 1 }, { 20, 20, 0.2f }, { 35, 20, 0.8f } };
    public static float[,] towerModStats = new float[,] { { 20, 20, 0.5f }, { 0.6f, 15, 1 }, { 20, 20, 0.2f }, { 35, 20, 0.8f } };

    public const int AMOUNTINCREASEDAMAGE = 5;
    public const int AMOUNTINCREASERANGE = 20;
    public const int AMOUNTDECREASEROF = 5;

    public int[] towerConfig;

    public GameObject[] towerDummyObjs;

    public Text[] towerTextFields;
    public Text[] towerActualTextFields;
    public Text[] blockCountTextFields;

    public GameObject constructionSnapObj;
    public ConstructionSnapBehaviour constructionSnap;

    private ExperimentState.GameState lastState;

    public ReplayEngine replayEngine;
    public ExperimentState experimentState;
    public int lastVersionID = 0;

    public PeripheryBehaviour peripheryBehaviour;

    public bool waitForEffect = false;
    public GameObject prefabEffect;
    public GameObject effectObject;

	// Use this for initialization
	void Start () {
        towerConfig = new int[4 * 3];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                towerConfig[i * 3 + j] = 0;
            }
        }

        constructionSnap = constructionSnapObj.GetComponent<ConstructionSnapBehaviour>();
        experimentState = Camera.main.GetComponent<ExperimentState>();
        peripheryBehaviour = Camera.main.GetComponent<PeripheryBehaviour>();
        replayEngine = Camera.main.GetComponent<ReplayEngine>();

        float[] resultFloatArr = new float[3];
        bool success = Settings.parseFloatArray("BasicTowerBaseStats", 3, ref resultFloatArr);
        if (success)
        {
            setTowerBaseValues(0, resultFloatArr);
        }

        success = Settings.parseFloatArray("FrostTowerBaseStats", 3, ref resultFloatArr);
        if (success)
        {
            setTowerBaseValues(1, resultFloatArr);
            
        }

        success = Settings.parseFloatArray("SwarmTowerBaseStats", 3, ref resultFloatArr);
        if (success)
        {
            setTowerBaseValues(2, resultFloatArr);
        }

        success = Settings.parseFloatArray("ExplosiveTowerBaseStats", 3, ref resultFloatArr);
        if (success)
        {
            setTowerBaseValues(3, resultFloatArr);
        }
	}
	
	// Update is called once per frame
    public void update(float deltaTime)
    {
	    if(lastState == ExperimentState.GameState.TowerConstruction_Intro2 && experimentState.gameState == ExperimentState.GameState.TowerConstruction_Test1)
        {
            towerDummyObjs[0].SetActive(true);
            updateTextFields(0);
        }
        else if (lastState == ExperimentState.GameState.TowerConstruction_Test1 && experimentState.gameState == ExperimentState.GameState.TowerConstruction_Test2)
        {
            towerDummyObjs[0].SetActive(false);
            towerDummyObjs[1].SetActive(true);
            constructionSnap.removeAll();
            updateTextFields(1);
            if(replayEngine.replayMode != ReplayEngine.ReplayMode.Replay) {
                replayEngine.autoSave.setProperty("ConstructionPreferences", serialiseTowerConfig());
            }
        }
        else if (lastState == ExperimentState.GameState.TowerConstruction_Test2 && experimentState.gameState == ExperimentState.GameState.TowerConstruction_Test3)
        {
            towerDummyObjs[1].SetActive(false);
            towerDummyObjs[2].SetActive(true);
            constructionSnap.removeAll();
            updateTextFields(2);
            if(replayEngine.replayMode != ReplayEngine.ReplayMode.Replay) {
                replayEngine.autoSave.setProperty("ConstructionPreferences", serialiseTowerConfig());
            }
        }
        else if (lastState == ExperimentState.GameState.TowerConstruction_Test3 && experimentState.gameState == ExperimentState.GameState.TowerConstruction_Test4)
        {
            towerDummyObjs[2].SetActive(false);
            towerDummyObjs[3].SetActive(true);
            constructionSnap.removeAll();
            updateTextFields(3);
            if(replayEngine.replayMode != ReplayEngine.ReplayMode.Replay) {
                replayEngine.autoSave.setProperty("ConstructionPreferences", serialiseTowerConfig());
            }
        }
        else if (lastState == ExperimentState.GameState.TowerConstruction_Test4 && experimentState.gameState == ExperimentState.GameState.TowerConstruction_Question1)
        {
            towerDummyObjs[3].SetActive(false);
            constructionSnap.removeAll();

            if(replayEngine.replayMode != ReplayEngine.ReplayMode.Replay) {
                replayEngine.autoSave.setProperty("ConstructionPreferences", serialiseTowerConfig());
            }

            loadFromAutoSave();
        }
        lastState = experimentState.gameState;

        // update UI if the data is desynchronised.
        if (lastVersionID != constructionSnap.getVersionID() && experimentState.gameState != ExperimentState.GameState.TowerConstruction_Question1)
        {
            int towerIndex = (int)experimentState.gameState - (int)ExperimentState.GameState.TowerConstruction_Test1;
            int[] data = constructionSnap.getComponentCounts();
            towerConfig[towerIndex * 3] = data[0];
            towerConfig[towerIndex * 3 + 1] = data[1];
            towerConfig[towerIndex * 3 + 2] = data[2];
            updateTextFields(towerIndex);
        }

        if (constructionSnap.towerComponents.Count == 6)
        {
            if (waitForEffect == false)
            {
                // initialise the effect
                waitForEffect = true;
                effectObject = (GameObject)Instantiate(prefabEffect, constructionSnap.transform.position, Quaternion.identity);
            }
            else if(effectObject == null)
            {
                // when the effect has despawned move to the next state
                waitForEffect = false;
                experimentState.nextState();
                // These should be applied by the next state action anyway
                //peripheryBehaviour.leftMenuDef = ExperimentState.TOWERCOMPONENTMENU;
                //peripheryBehaviour.rightMenuDef = ExperimentState.TOWERCOMPONENTMENU;
            }
        }
	}

    public void loadFromAutoSave()
    {
        string previousData = Camera.main.GetComponent<ReplayEngine>().autoSave.getPropertyValue("ConstructionPreferences");
        if (previousData.Length > 0)
        {
            float[,] data = deserialiseTowerConfig(previousData);
            if (data != null)
            {
                towerModStats = data;
            }
            // else do nothing, no data has changed
        }
    }

    private void updateTextFields(int towerIndex)
    {
        towerTextFields[towerIndex * 3].text = towerConfig[towerIndex * 3] * AMOUNTINCREASEDAMAGE + "%";
        towerTextFields[towerIndex * 3 + 1].text = towerConfig[towerIndex * 3 + 1] * AMOUNTINCREASERANGE + "%";
        towerTextFields[towerIndex * 3 + 2].text = towerConfig[towerIndex * 3 + 2] * AMOUNTDECREASEROF + "%";

        towerModStats[towerIndex, 0] = ((1 + towerConfig[towerIndex * 3] * AMOUNTINCREASEDAMAGE / 100.0f) * towerBaseStats[towerIndex, 0]);
        towerActualTextFields[towerIndex * 3].text = towerModStats[towerIndex, 0] + "";
        
        if(ExperimentState.GAMEVERSION > 4)
        {
            towerModStats[towerIndex, 1] = ((1 + towerConfig[towerIndex * 3 + 1] * AMOUNTINCREASERANGE / 100.0f) * towerBaseStats[towerIndex, 1]);
        }

        towerActualTextFields[towerIndex * 3 + 1].text = towerModStats[towerIndex, 1] + "";
        towerModStats[towerIndex, 2] = ((1 - towerConfig[towerIndex * 3 + 2] * AMOUNTDECREASEROF / 100.0f) * towerBaseStats[towerIndex, 2]);
        towerActualTextFields[towerIndex * 3 + 2].text = towerModStats[towerIndex, 2] + "s";

        blockCountTextFields[towerIndex].text = constructionSnap.towerComponents.Count +  "/6 Blocks";
    }

    public void setTowerConfig(float[,] towerData)
    {
        for (int i = 0; i < 4; i++)
        {
            towerModStats[i, 0] = towerData[i,0];
            towerModStats[i, 1] = towerData[i, 1];
            towerModStats[i, 2] = towerData[i, 2];
        }
    }

    public void setTowerBaseValues(int towerIndex, float[] values)
    {
        towerBaseStats[0, 0] = values[0];
        towerBaseStats[0, 1] = values[1];
        towerBaseStats[0, 2] = values[2];
        towerModStats[towerIndex, 0] = ((1 + towerConfig[towerIndex * 3] * AMOUNTINCREASEDAMAGE / 100.0f) * towerBaseStats[towerIndex, 0]);
        towerModStats[towerIndex, 1] = ((1 + towerConfig[towerIndex * 3 + 1] * AMOUNTINCREASERANGE / 100.0f) * towerBaseStats[towerIndex, 1]);
        towerModStats[towerIndex, 2] = ((1 - towerConfig[towerIndex * 3 + 2] * AMOUNTDECREASEROF / 100.0f) * towerBaseStats[towerIndex, 2]);
    }

    public string serialiseTowerConfig()
    {
        string result = "";

        for (int i = 0; i < 4; i++)
        {
            result += towerModStats[i, 0] + "," + towerModStats[i, 1] + "," + towerModStats[i, 2];
            if (i < 3)
            {
                result += ",";
            }
        }

        return result;
    }

    public float[,] deserialiseTowerConfig(string data)
    {
        string[] splitData = data.Split(',');
        if (splitData.Length != 4 * 3)
        {
            return null;
        }
        try
        {
            float[,] result = new float[4, 3];
            int k = 0;
            for (int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    result[i, j] = float.Parse(splitData[k]);
                    k++;
                }
            }
            return result;
        }
        catch (System.Exception)
        {
            return null;
        }
    }
}
