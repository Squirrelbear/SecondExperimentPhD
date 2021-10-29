using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ReplayEngine : MonoBehaviour {

    public enum ReplayMode { Record, Replay, None };

    // E - Control Tower
    // M - Move Tower
    // R - Repair Tower
    // D - Destroy Tower
    // 1-4 - spawn tower with id
    // F9 skip to last autosave
    // F12 skip state
    /*private KeyCode[] trackKeyCodes = new KeyCode[] { KeyCode.E, KeyCode.M, KeyCode.R, KeyCode.D, 
                                                      KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, 
                                                      KeyCode.F9, KeyCode.F12 };
    */
     
    public ReplayMode replayMode;
    public string replayFile;
    public int curFrameID;
    public ReplayDatabase replayDatabase;
    public ReplayEvent curFrame;

    public AutoSave autoSave;
    public AutoSave lastState;

    private bool loadedAutoSave;

    public ExtraDataRecorder extraDataRecorder;

    public const string saveFilePath = "ExperimentData/";
    public const string saveFileFolderName = "ExperimentData";

	// Use this for initialization
	void Start () 
    {
        Settings s = Settings.getSingleton();
        string replayFileSetting = s.getPropertyValue("ReplayFile");
        if(replayFileSetting != null && replayFileSetting.Length != 0 && !replayFileSetting.Equals("null"))
        {
            replayMode = ReplayMode.Replay;
            replayFile = replayFileSetting;
        }

        autoSave = new AutoSave();
        autoSave.setProperty("GameVersion", ExperimentState.GAMEVERSION+"");
        lastState = new AutoSave();

        if (replayMode == ReplayMode.Replay && replayFile.Length == 0)
        {
            replayMode = ReplayMode.None;
        }
        else if (replayMode == ReplayMode.Replay)
        {
            replayDatabase = new ReplayDatabase();
            bool result = replayDatabase.loadDatabase(replayFile);
            bool resultAutoSave = lastState.loadAutoSave(replayFile.Replace(".dat", ".autosave"));
            if (!result)
            {
                replayMode = ReplayMode.None;
                ErrorLog.logData("Defaulting to no replay running.");
            }
            else if(!resultAutoSave)
            {
                replayMode = ReplayMode.None;
                ErrorLog.logData("Defaulting to no replay running (missing autosave).");
            }
            else
            {
                curFrame = replayDatabase.getNextEvent();
                loadedAutoSave = true;
                if (curFrame == null)
                {
                    ErrorLog.logData("Error: No first frame for replay.");
                    return;
                }
                else
                {
                    Camera.main.transform.rotation = curFrame.rotation;
                    curFrameID = 0;
                }
            }
        }
        else if(replayMode == ReplayMode.Record)
        {
            if (gameObject.GetComponent<CameraBehaviour>().clickDebugMode)
            {
                //print("Disabling record mode. Click debug is not supported for recording.");
                ErrorLog.logData("Disabling record mode. Click debug is not supported for recording.", true);
                replayMode = ReplayMode.None;
            }
            else
            {
                replayDatabase = new ReplayDatabase();
                ReplayEvent firstFrame = new ReplayEvent(0,
                                        Camera.main.transform.rotation,
                                        Camera.main.transform.position,
                                         Input.GetMouseButtonDown(0),
                                         ExperimentState.GameState.MainMenu,
                                         new List<KeyCode>());
                replayDatabase.addEvent(firstFrame);
                replayFile = saveFilePath + replayDatabase.getCreationData() + ".dat";
                curFrameID = 0;
                extraDataRecorder = ExtraDataRecorder.getSingleton();
                extraDataRecorder.setFileName(saveFilePath + replayDatabase.getCreationData() + ".csv");
                ErrorLog.setFileName(saveFilePath + replayDatabase.getCreationData() + ".log");
            }
        }
	}

    void OnApplicationQuit()
    {
        Directory.CreateDirectory(saveFileFolderName);

        if (replayMode == ReplayMode.Record)
        {
            replayDatabase.saveDatabase(saveFilePath + replayDatabase.getCreationData() + ".dat");
            autoSave.saveAs(saveFilePath + replayDatabase.getCreationData() + ".autosave");
        }

        if (extraDataRecorder != null)
        {
            extraDataRecorder.save();
        }

        ErrorLog.save();
    }
	
	// Update is called once per frame
	public void preUpdate () 
    {
        if(!loadedAutoSave)
        {
            lastState.loadLastAutoSave();
            loadedAutoSave = true;
        }

        if(replayMode == ReplayMode.Replay)
        {
            preReplayUpdate();
        }
        else
        {
            curFrame = new ReplayEvent(Time.deltaTime,
                                    Camera.main.transform.rotation,
                                    Camera.main.transform.position,
                                     Input.GetMouseButtonDown(0),
                                     Camera.main.GetComponent<ExperimentState>().gameState,
                                     new List<KeyCode>());
        }
	}

    public void postUpdate()
    {
        if(replayMode == ReplayMode.Record)
        {
            replayDatabase.addEvent(curFrame);
            curFrameID = replayDatabase.replayEvents.Count;
        }
    }

    public bool getTriggerState()
    {
        if (curFrame == null)
        {
            return false;
        }
        return curFrame.triggerEdge;
    }

    public float getDeltaTime()
    {
        if(curFrame == null)
        {
            return 0;
        }
        return curFrame.deltaTime;
    }

    public bool checkKey(KeyCode key)
    {
        if (replayMode == ReplayMode.Replay)
        {
            if(curFrame == null)
            {
                return false;
            }

            foreach (KeyCode testKey in curFrame.keyEvents)
            {
                if (key == testKey)
                {
                    return true;
                }
            }
            return false;
        }

        bool result = Input.GetKeyDown(key);

        if(replayMode == ReplayMode.Record && result)
        {
            curFrame.keyEvents.Add(key);
        }
        return result;
    }

    private void preReplayUpdate()
    {
        // replay event info
        curFrameID = replayDatabase.getCurFrameID();
        curFrame = replayDatabase.getNextEvent(); //new ReplayEvent();

        if (curFrame == null)
            return;

        // applying transformation to parent object should mean the 
        // data is applied even if the Oculus is connected.

        // This code WILL NOT work if the Oculus is connected
        Camera.main.transform.rotation = curFrame.rotation;
        Camera.main.transform.position = curFrame.position;
    }

    private void preRecordUpdate()
    {
        curFrame = new ReplayEvent(Time.deltaTime,
                                    Camera.main.transform.rotation,
                                    Camera.main.transform.position,
                                     Input.GetMouseButtonDown(0),
                                     Camera.main.GetComponent<ExperimentState>().gameState,
                                     new List<KeyCode>());
    }
    
    public void useLastAutoSave()
    {
        this.autoSave = this.lastState;
    }

}
