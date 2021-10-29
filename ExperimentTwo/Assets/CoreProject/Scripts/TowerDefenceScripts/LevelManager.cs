using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LevelManager : MonoBehaviour {

    public enum LEVELSTATE { WaitForBegin = 0, InitialConstruction = 1, FirstWaveSet = 2, FirstDowntime = 3, SecondWaveSet = 4, SecondDownTime = 5, 
                             ThirdWaveSet = 6, ThirdDownTime = 7, BossWave = 8, Complete = 9 };

    // Wave definition structure:
    // timeTillSpawn:unitID; ... timeTillSpawn:unitID; -1:-1
    // -1:-1 = end of wave
    // 0 = basic, 1 = dangerous, 2 = fast, 3 = boss

    private const string FIRSTWAVEDEFINITION = "1:0;1:0;1:0;1:0;1:0;-1:-1;"                                              // 5 basic
                                             + "1:0;1:0;1:2;1:2;1:0;1:0;1:2;1:2;-1:-1;"                                  // (2 basic, 2 fast) * 2
                                             + "1:0;1:0;1:0;1:0;1:2;1:2;1:2;1:2;1:0;1:0;1:0;1:0;1:2;1:2;1:2;1:2;-1:-1";  // (4 basic, 4 fast) * 2

    private const string SECONDWAVEDEFINTION = "1:1;1:1;1:1;1:1;1:1;-1:-1;"                                              // 5 dangerous
                                             + "1:2;1:2;1:1;1:2;1:2;1:1;1:2;1:2;1:1;-1:-1;"                              // (2 fast, 1 dangerous) * 3
                                             + "1:0;1:1;1:2;1:0;1:1;1:2;1:0;1:1;1:2;-1:-1";                              // (1 basic, 1 dangerous, 1 fast) * 3

    private const string THIRDWAVEDEFINTION = "1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;-1:-1;"                   // 12 fast
                                             + "1:0;1:0;1:2;1:2;1:2;1:2;1:0;1:0;1:2;1:2;1:2;1:2;1:1;1:1;1:1;1:1;-1:-1;"  // (2 basic, 4 fast) * 2, 5 dangerous
                                             + "1:0;1:0;1:1;1:1;1:2;1:2;1:0;1:0;1:1;1:1;1:2;1:2;1:0;1:0;1:1;1:1;1:2;1:2;5:3;-1:-1";  // (2 basic, 2 dangerous, 2 fast) * 3, 1 boss

    // Boss, Dangerous, Boss, Dangerous, Boss, Boss, Dangerous, Boss, Dangerous, Boss
    private const string BOSSWAVEDEFINTION = "1.5:3;1.5:1;1.5:3;1.5:1;1.5:3;1.5:3;1.5:1;1.5:3;1.5:1;1.5:3;-1:-1";        


    private string[] stateData = new string[]{ "",  // Wait for begin 
                                "60", // Initial Construction
                                FIRSTWAVEDEFINITION,   // First Wave Set
                                "60",   // First DownTime
                                SECONDWAVEDEFINTION,   // Second Wave Set
                                "60",   // Second Down Time
                                THIRDWAVEDEFINTION,   // Third Wave Set
                                "60",   // Third DownTime
                                BOSSWAVEDEFINTION,    // BossWave
                                ""};  // Complete

    private List<GameObject> towerSnapPoints;
    private List<GameObject> towers;

    private ExperimentState experimentState;
    private PeripheryBehaviour peripheryBehaviour;
    private AISpawner spawner;
    private CameraBehaviour cameraRef;

    public LEVELSTATE levelState = LEVELSTATE.WaitForBegin;

    private bool loaded = false;

    public GameObject levelStatusDialogObj;
    public LevelStatusDialogBehaviour levelStatusDialog;

    public int[] TOWERCOSTS = new int[]{ 30, 40, 50, 50 };
    public int currency = 100;
    public int preGameBonus = 20;
    public int postRoundBonus = 20;

    public GameObject dialogSelectingObject, dialogSelectedObject;
    private TowerStatusDialog dialogSelecting, dialogSelected;

    public GameObject[] tutorialObjects;
    private ExperimentState.GameState lastExperimentState = ExperimentState.GameState.MainMenu;

    public ExtraDataRecorder extraDataRecorder;
    public ExtraDataRecorder.ExtraDataCollection tdDataLog;
                              // TowerBasic=0,TowerFrost=1,TowerSwarm=2,TowerExplosive=3,TowerMoved=4,TowerRepaired=5,TowerDestroyed=6,TowerDeselected=7,UnitsKilled=8,UnitsNotKilled=9
                              // InsufficientTowerPurchase=10, InsufficientRepair=11, CameraSnapJumps=12
    public int[] loggingStats = new int[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

	// Use this for initialization
	void Start () {
        towerSnapPoints = new List<GameObject>();
        towers = new List<GameObject>();
        spawner = null;
        cameraRef = null;

        extraDataRecorder = ExtraDataRecorder.getSingleton();
        tdDataLog = extraDataRecorder.getDataCollection("TowerDefenceStats");
        tdDataLog.logData("Timestamp,LevelState,TowerBasic,TowerFrost,TowerSwarm,TowerExplosive,TowerMoved,TowerRepaired,TowerDestroyed,TowerDeselected,UnitsKilled,UnitsNotKilled,FailedTowerBuy,FailedRepair,CameraSnapsUsed,BaseHealth", false);
	}
	
	// Update is called once per frame
    public void update(float deltaTime)
    {
        if(!loaded) {
            firstUpdateInit();
            loaded = true;
        }

        if (experimentState.gameState == ExperimentState.GameState.TowerDefence_Test)
        {
            int action = peripheryBehaviour.menuBehaviour.menuAction;
            if (action >= 1000 && action < 1004)
            {
                int towerID = action % 1000;

                if (currency - TOWERCOSTS[towerID] >= 0)
                {
                    ErrorLog.logData("Spawning Tower: " + (action % 1000),false);
                    // Spawn tower first to ensure the menu is updated.
                    cameraRef.spawnTower(towerID);
                    addCurrency(-TOWERCOSTS[towerID]);
                    loggingStats[towerID]++;
                }
                else
                {
                    //print("Insufficent Funds. TODO Show UI Feedback.");
                    loggingStats[10]++;
                }
            }
            else if (action == 3000)
            {
                // Attempt to move tower
                ErrorLog.logData("Moving tower.", false);
                cameraRef.moveSelectedTower();
                loggingStats[4]++;
            }
            else if (action == 3001)
            {
                // Repair tower
                ErrorLog.logData("Repairing tower.", false);
                repairSelectedTower();
            }
            else if (action == 3002)
            {
                // Destroy tower
                ErrorLog.logData("Destroying tower.", false);
                destroySelectedTower();
                loggingStats[6]++;
            }
            else if (action == 3003)
            {
                // Deselect Tower
                ErrorLog.logData("Deselecting tower.", false);
                cameraRef.forceDeselectImmediately();
                loggingStats[7]++;
            }

            foreach (GameObject tower in towers)
            {
                if (tower != null)
                {
                    tower.GetComponent<TowerBehaviour>().update(deltaTime);
                }
            }

            spawner.update(deltaTime);
            levelStatusDialog.update(deltaTime);
            updateMenuLeftText();
            dialogSelecting.update();
            dialogSelected.update();

            updateLevelState();
        }

        if (lastExperimentState != experimentState.gameState)
        {
            if (experimentState.gameState == ExperimentState.GameState.TowerDefence_Intro01)
            {
                tutorialObjects[0].SetActive(true);
            }
            else if (experimentState.gameState == ExperimentState.GameState.TowerDefence_Intro02)
            {
                tutorialObjects[0].SetActive(false);
                tutorialObjects[1].SetActive(true);
            }
            else if (experimentState.gameState == ExperimentState.GameState.TowerDefence_Intro03)
            {
                tutorialObjects[1].SetActive(false);
                tutorialObjects[2].SetActive(true); // This needs to stay active for two states worth
            }
            else if (experimentState.gameState == ExperimentState.GameState.TowerDefence_Intro05)
            {
                tutorialObjects[2].SetActive(false);
                tutorialObjects[3].SetActive(true);
            }
            else if(experimentState.gameState == ExperimentState.GameState.TowerDefence_Intro06)
            {
                tutorialObjects[3].SetActive(false);
            }
        }

        lastExperimentState = experimentState.gameState;
	}

    public void updateLevelState()
    {
        switch(levelState)
        {
            case LEVELSTATE.FirstWaveSet:
            case LEVELSTATE.SecondWaveSet:
            case LEVELSTATE.ThirdWaveSet:
            case LEVELSTATE.BossWave:
                if (!spawner.spawning)
                {
                    nextState();
                }
                break;
            case LEVELSTATE.InitialConstruction:
            case LEVELSTATE.FirstDowntime:
            case LEVELSTATE.SecondDownTime:
            case LEVELSTATE.ThirdDownTime:
                if(levelStatusDialog.countdownClock <= 0)
                {
                    nextState();
                }
                break;
        }
    }

    public void nextState()
    {
        if(levelState == LEVELSTATE.Complete)
        {
            return;
        }

        string logData = levelState.ToString();
        for(int i = 0; i < 13; i++) 
        {
            logData += "," + loggingStats[i];
        }
        logData += "," + GameObject.Find("EndPoint").GetComponent<EndPointBehaviour>().unitHealth;
        tdDataLog.logData(logData, true);
        loggingStats = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        levelState++;

        switch (levelState)
        {
            case LEVELSTATE.FirstWaveSet:
                spawner.spawnWave(stateData[(int)levelState]);
                levelStatusDialog.setDisplayState(LevelStatusDialogBehaviour.DisplayState.ShowWaveStatus, 0);
                break;
            case LEVELSTATE.SecondWaveSet:
                spawner.spawnWave(stateData[(int)levelState]);
                levelStatusDialog.setDisplayState(LevelStatusDialogBehaviour.DisplayState.ShowWaveStatus, 0);
                break;
            case LEVELSTATE.ThirdWaveSet:
                spawner.spawnWave(stateData[(int)levelState]);
                levelStatusDialog.setDisplayState(LevelStatusDialogBehaviour.DisplayState.ShowWaveStatus, 0);
                break;
            case LEVELSTATE.BossWave:
                spawner.spawnWave(stateData[(int)levelState]);
                levelStatusDialog.setDisplayState(LevelStatusDialogBehaviour.DisplayState.ShowWaveStatus, 0);
                break;
            case LEVELSTATE.InitialConstruction:
            case LEVELSTATE.FirstDowntime:
            case LEVELSTATE.SecondDownTime:
            case LEVELSTATE.ThirdDownTime:
                if (cameraRef.clickDebugMode)
                {
                    levelStatusDialog.setDisplayState(LevelStatusDialogBehaviour.DisplayState.WaitForTime, 10);
                    ErrorLog.logData("Overriding default pause timer because ClickDebugMode to: 10 seconds",false);
                }
                else
                {
                    if (levelState == LEVELSTATE.InitialConstruction)
                    {
                        addCurrency(preGameBonus);
                    }
                    else
                    {
                        addCurrency(postRoundBonus);
                    }
                    levelStatusDialog.setDisplayState(LevelStatusDialogBehaviour.DisplayState.WaitForTime, int.Parse(stateData[(int)levelState]));
                }
                break;
            case LEVELSTATE.Complete:
                levelStatusDialog.setDisplayState(LevelStatusDialogBehaviour.DisplayState.WaitForInstruction, 0);
                break;

        }

        ErrorLog.logData("TowerDefence: Level State Set To: " + levelState, false);
    }

    public void addCurrency(int amount)
    {
        currency += amount;

        updateMenuRightText();
        updateMenuLeftText();
    }

    public void updateMenuRightText()
    {
        if(cameraRef.selectedTower != null && cameraRef.selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
        {
            peripheryBehaviour.rightMenuDef = "";
        }
        else
        {
            string tempRightMenu = ExperimentState.TOWERDEFENCEMENURIGHT;
            tempRightMenu = tempRightMenu.Replace("%B%", (currency - TOWERCOSTS[0] >= 0) ? "Basic Tower [" + TOWERCOSTS[0] + "]" : "Insufficient Funds"); // Basic Cost
            tempRightMenu = tempRightMenu.Replace("%F%", (currency - TOWERCOSTS[1] >= 0) ? "Frost Tower [" + TOWERCOSTS[1] + "]" : "Insufficient Funds"); // Frost Cost
            tempRightMenu = tempRightMenu.Replace("%S%", (currency - TOWERCOSTS[2] >= 0) ? "Swarm Tower [" + TOWERCOSTS[2] + "]" : "Insufficient Funds"); // Swarm Cost
            tempRightMenu = tempRightMenu.Replace("%E%", (currency - TOWERCOSTS[3] >= 0) ? "Explosive Tower [" + TOWERCOSTS[3] + "]" : "Insufficient Funds"); // Explosive Cost
            peripheryBehaviour.rightMenuDef = tempRightMenu;
        }
    }

    public void updateMenuLeftText()
    {
        if (cameraRef.selectedTower == null || cameraRef.selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
        {
            peripheryBehaviour.leftMenuDef = "";
        }
        else
        {
            string tempLeftMenu = ExperimentState.TOWERDEFENCEMENULEFT;
            int repairCost = getTowerRepairCost(cameraRef.selectedTower.towerType, cameraRef.selectedTower.durability);
            if (repairCost == 0)
            {
                tempLeftMenu = tempLeftMenu.Replace("Repair Tower [-%R%]", "No Repair Required");
            }
            else if (currency - repairCost < 0)
            {
                tempLeftMenu = tempLeftMenu.Replace("Repair Tower [-%R%]", "Insufficient [-" + repairCost + "]");
            }
            else
            {
                tempLeftMenu = tempLeftMenu.Replace("%R%", repairCost+""); // Repair Cost
            }
            int destroyGain = getTowerDestroyGain(cameraRef.selectedTower.towerType, cameraRef.selectedTower.durability);
            tempLeftMenu = tempLeftMenu.Replace("%D%", destroyGain+""); // Destroy Gain

            if (stateData[(int)levelState].Length > 2)
            {
                tempLeftMenu = tempLeftMenu.Replace("Move Tower,3000", "Move Tower Unavailable,3100");
            }

            peripheryBehaviour.leftMenuDef = tempLeftMenu;
        }
    }

    public void repairSelectedTower()
    {
        if (cameraRef.selectedTower != null)
        {
            int repairCost = getTowerRepairCost(cameraRef.selectedTower.towerType, cameraRef.selectedTower.durability);
            if (currency - repairCost >= 0)
            {
                addCurrency(-repairCost);
                cameraRef.selectedTower.repairTower();

                loggingStats[5]++;
            }
            else
            {
                ErrorLog.logData("Insufficient Funds. Tower not repaired.",false);

                loggingStats[11]++;
            }
        }
    }

    public void destroySelectedTower()
    {
        if (cameraRef.selectedTower != null)
        {
            int destroyGain = getTowerDestroyGain(cameraRef.selectedTower.towerType, cameraRef.selectedTower.durability);
            addCurrency(destroyGain);
            TowerBehaviour script = cameraRef.selectedTower;
            script.destroyTower();
            foreach (GameObject snap in towerSnapPoints)
            {
                if (snap.GetComponent<TowerSnapBehaviour>().associatedTower == script.gameObject)
                {
                    snap.GetComponent<TowerSnapBehaviour>().associatedTower = null;
                }
            }
            towers.Remove(script.gameObject);
            cameraRef.forceDeselectImmediately();
            Destroy(script.gameObject);
        }
    }

    // Deprecated
/*    public void destroySelectedTower()
    {
        if (cameraRef.selectedTower != null)
        {
            int destroyGain = getTowerDestroyGain(cameraRef.selectedTower.towerType, cameraRef.selectedTower.durability);
            addCurrency(destroyGain);
            foreach (TowerSnapBehaviour snap in cameraRef.towerSnapPoints)
            {
                if (snap.associatedTower == this.gameObject)
                {
                    snap.associatedTower = null;
                }
            }
            DestroyObject(cameraRef.selectedTower.gameObject);
        }
    }*/

    public int getTowerRepairCost(TowerBehaviour.TowerType towerType, float durability)
    {
        int modCode = 0;
        switch (towerType)
        {
            case TowerBehaviour.TowerType.Basic:
                modCode = 0;
                break;
            case TowerBehaviour.TowerType.Frost:
                modCode = 1;
                break;
            case TowerBehaviour.TowerType.Swarm:
                modCode = 2;
                break;
            case TowerBehaviour.TowerType.Explosive:
                modCode = 3;
                break;
        }
        return (int)(TOWERCOSTS[modCode] * (0.5f * (1 - durability)));
    }

    public int getTowerDestroyGain(TowerBehaviour.TowerType towerType, float durability)
    {
        int modCode = 0;
        switch (towerType)
        {
            case TowerBehaviour.TowerType.Basic:
                modCode = 0;
                break;
            case TowerBehaviour.TowerType.Frost:
                modCode = 1;
                break;
            case TowerBehaviour.TowerType.Swarm:
                modCode = 2;
                break;
            case TowerBehaviour.TowerType.Explosive:
                modCode = 3;
                break;
        }
        return (int)(TOWERCOSTS[modCode] * (0.3f) * (1+durability));// + 0.25f * (1 - durability)));
    }

    public void addTower(GameObject tower)
    {
        towers.Add(tower);
    }

    private void firstUpdateInit()
    {
        applySettings();

        levelState = LEVELSTATE.WaitForBegin;
        GameObject[] snapPoints = GameObject.FindGameObjectsWithTag("TowerSnap");
        foreach(GameObject snapPoint in snapPoints) {
            towerSnapPoints.Add(snapPoint);
        }

        GameObject[] prespawnedTowers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (GameObject tower in prespawnedTowers)
        {
            towers.Add(tower);
        }

        spawner = GameObject.FindGameObjectWithTag("AISpawner").GetComponent<AISpawner>();
        cameraRef = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraBehaviour>();
        peripheryBehaviour = cameraRef.GetComponent<PeripheryBehaviour>();
        experimentState = cameraRef.GetComponent<ExperimentState>();
        levelStatusDialog = levelStatusDialogObj.GetComponent<LevelStatusDialogBehaviour>();
        levelStatusDialog.showMenu();
        levelStatusDialog.setDisplayState(LevelStatusDialogBehaviour.DisplayState.WaitForWave, 0);
        dialogSelected = dialogSelectedObject.GetComponent<TowerStatusDialog>();
        dialogSelecting = dialogSelectingObject.GetComponent<TowerStatusDialog>();
    }

    private void applySettings()
    {
        int resultInt = 0;
        bool success = Settings.parseIntSetting("StartingResources", ref resultInt);
        if (success)
        {
            currency = resultInt;
        }

        success = Settings.parseIntSetting("PreGameBonusResources", ref resultInt);
        if (success)
        {
            preGameBonus = resultInt;
        }

        success = Settings.parseIntSetting("PostRoundBonusResources", ref resultInt);
        if (success)
        {
            postRoundBonus = resultInt;
        }

        success = Settings.parseIntSetting("WaveIntervalTime", ref resultInt);
        if (success)
        {
            stateData[1] = resultInt + "";
            stateData[3] = resultInt + "";
            stateData[5] = resultInt + "";
            stateData[7] = resultInt + "";
        }

        int[] resultIntArr = new int[4];
        success = Settings.parseIntArray("TowerCosts", 4, ref resultIntArr);
        if (success)
        {
            TOWERCOSTS = new int[] { resultIntArr[0], resultIntArr[1], resultIntArr[2], resultIntArr[3] };
        }

        Settings s = Settings.getSingleton();
        string firstWave = s.getPropertyValue("FirstWaveDefinition");
        if (firstWave.Length != 0)
        {
            stateData[2] = firstWave;
        }

        string secondWave = s.getPropertyValue("SecondWaveDefinition");
        if (secondWave.Length != 0)
        {
            stateData[4] = secondWave;
        }

        string thirdWave = s.getPropertyValue("ThirdWaveDefinition");
        if (thirdWave.Length != 0)
        {
            stateData[6] = thirdWave;
        }

        string bossWave = s.getPropertyValue("BossWaveDefinition");
        if (bossWave.Length != 0)
        {
            stateData[8] = bossWave;
        }
    }
}
