using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuSensitivityTest : MonoBehaviour {

    public enum TaskState { InitFirstTask, CompletingTask, ReturnToCentre, ReturnToCentreFailedTask, AllTasksComplete };

    public List<Vector3> taskOrderData;
    public int taskID = -1;
    System.Random gen;

    public GameObject menuLeftObj, menuMiddleObj, menuRightObj;

    // Cached
    public SensitivityMenuBehaviour menuLeft, menuMiddle, menuRight;

    public PeripheryBehaviour peripheryBehaviour;

    public TaskState taskState = TaskState.InitFirstTask;
    public ExperimentState experimentState;

    public ExtraDataRecorder extraDataRecorder;
    public ExtraDataRecorder.ExtraDataCollection taskData;
    public float timeSinceLastState = 0;

    // left, right, too left, too right
    private int[] failureCount = new int[]{ 0, 0, 0, 0 };

	// Use this for initialization
	void Start () {
        gen = new System.Random();
        generateTaskOrder();
        menuLeft = menuLeftObj.GetComponent<SensitivityMenuBehaviour>();
        menuRight = menuRightObj.GetComponent<SensitivityMenuBehaviour>();
        menuMiddle = menuMiddleObj.GetComponent<SensitivityMenuBehaviour>();
        menuLeft.hideMenu();
        menuRight.hideMenu();
        menuMiddle.hideMenu();
        menuLeftObj.GetComponent<Canvas>().enabled = false;
        menuRightObj.GetComponent<Canvas>().enabled = false;
        menuMiddleObj.GetComponent<Canvas>().enabled = false;

        extraDataRecorder = ExtraDataRecorder.getSingleton();
        taskData = extraDataRecorder.getDataCollection("Sensitivity Test Data");
        // left/right num, trigger/no trigger num, sensitivity num, left/right text, Menu/NoMenu text, Sensitivity Text, Task Time, 
        // Failed Left Revealed, Failed Right Revealed, Failed Left No Reveal, Failed Right No Reveal
        taskData.logData("Timestamp,Left/Right_Num,Menu/NoMenu_Num,Sensitivity_Num,Left/Right,Menu/NoMenu,Sensitivity,Task Time,LeftMenuRevealed,RightMenuRevealed,LeftPanel,RightPanel", false);
	}
	
	// Update is called once per frame
	public void update (float deltaTime) 
    {
        if(experimentState.gameState != ExperimentState.GameState.MenuSensitivityTest_Test)
        {
            menuLeftObj.GetComponent<Canvas>().enabled = false;
            menuRightObj.GetComponent<Canvas>().enabled = false;
            menuMiddleObj.GetComponent<Canvas>().enabled = false;
            return;
        }
        //print("Bam");

        if (taskState == TaskState.InitFirstTask)
        {
            initFirstTask();
        }

        //print("Boom");

        // Reference Note:
        // taskOrderData[taskID].x = right/left, 0 = left, 1 = right
        // taskOrderData[taskID].y = trigger state; 0 = no, 1 = yes
        // taskOrderData[taskID].z = sensitivity; 0 = low, 1 = medium, 2 = high

        menuLeft.update(deltaTime);
        menuRight.update(deltaTime);
        menuMiddle.update(deltaTime);
        //print(menuLeft.menuAction + " " + menuMiddle.menuAction + " " + menuRight.menuAction);
        
        if(taskState == TaskState.CompletingTask || taskState == TaskState.ReturnToCentreFailedTask)
        {
            timeSinceLastState += deltaTime;
        }

        if (taskState == TaskState.CompletingTask)
        {
            updateCompletingTask();
        }
        else if(taskState == TaskState.ReturnToCentre)
        {
            if (menuMiddle.menuAction == -1)
            {
                setTaskState(TaskState.CompletingTask);
            }
        }
        else if (taskState == TaskState.ReturnToCentreFailedTask)
        {
            if (menuMiddle.menuAction == -1)
            {
                taskID--;
                setTaskState(TaskState.CompletingTask);
            }
        }
	}

    private void initFirstTask()
    {
        string previousData = "";
        ReplayEngine replayEngine = Camera.main.GetComponent<ReplayEngine>();
        if (replayEngine.replayMode == ReplayEngine.ReplayMode.Replay)
        {
            previousData = replayEngine.lastState.getPropertyValue("SensitivityTaskOrder");
        }
        else
        {
            previousData = replayEngine.autoSave.getPropertyValue("SensitivityTaskOrder");
        }
        if(previousData.Length > 0)
        {
            List<Vector3> data = getSerialisedSensitivityData(previousData);
            if(data != null)
            {
                taskOrderData = data;
            }
        }

        //menuLeft = menuLeftObj.GetComponent<SensitivityMenuBehaviour>();
        //menuRight = menuRightObj.GetComponent<SensitivityMenuBehaviour>();
        //menuMiddle = menuMiddleObj.GetComponent<SensitivityMenuBehaviour>();
        menuLeft.showMenu();
        menuRight.showMenu();
        menuMiddle.showMenu();
        setTaskState(TaskState.ReturnToCentre);
    }

    private void updateCompletingTask()
    {
        PeripheryBehaviour.RotationResult rotationTrigger = peripheryBehaviour.curFrameResult;
        bool leftTrigger = (menuLeft.menuAction == -1);
        bool rightTrigger = (menuRight.menuAction == -1);

        bool resultTrigger = (taskOrderData[taskID].y == 1);
        bool resultLeft = (taskOrderData[taskID].x == 0);

        // TODO: Consider if there should be a timer before task is autofailed

        // Show Menu At Left Side
        if (resultTrigger && resultLeft)
        {
            if (rotationTrigger == PeripheryBehaviour.RotationResult.Left)
            {
                ErrorLog.logData("Success: " + resultTrigger + " " + resultLeft + " " + rotationTrigger + " " + leftTrigger + " " + rightTrigger,false);
                // left/right num, trigger/no trigger num, sensitivity num, left/right text, Menu/NoMenu text, Sensitivity Text, Task Time, 
                // Failed Left Revealed, Failed Right Revealed, Failed Left No Reveal, Failed Right No Reveal
                string stageResultData = taskOrderData[taskID].x + "," + taskOrderData[taskID].y + "," + taskOrderData[taskID].z + ","
                    + (resultLeft ? "Left" : "Right") + "," + (resultTrigger ? "Menu" : "No Menu") + "," 
                    + ((taskOrderData[taskID].z == 0) ? "Low" : ((taskOrderData[taskID].z == 1) ? "Medium" : "High")) + ","
                    + timeSinceLastState + "," + failureCount[0] + "," + failureCount[1] + "," + failureCount[2] + "," + failureCount[3];
                taskData.logData(stageResultData, true);
                failureCount = new int[] { 0, 0, 0, 0 };
                timeSinceLastState = 0;
                setTaskState(TaskState.ReturnToCentre);
            }
            else if (rotationTrigger == PeripheryBehaviour.RotationResult.Right)
            {
                // Looked wrong way to trigger menu
                ErrorLog.logData("Looked wrong way to trigger menu",false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[1]++;
            }
            else if (leftTrigger)
            {
                // Too far with no trigger
                ErrorLog.logData("Too far with no trigger",false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[2]++;
            }
            else if (rightTrigger)
            {
                // Looked too far in wrong way
                ErrorLog.logData("Looked too far in wrong way",false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[3]++;
            }
            // else no result found yet
        }
        // Show no menu left Side
        else if (!resultTrigger && resultLeft) 
        {
            if (rotationTrigger == PeripheryBehaviour.RotationResult.Left)
            {
                // Turned too fast
                ErrorLog.logData("Turned too fast", false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[0]++;
            }
            else if (rotationTrigger == PeripheryBehaviour.RotationResult.Right)
            {
                // Looked wrong way to trigger menu
                ErrorLog.logData("Looked wrong way to trigger menu", false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[1]++;
            }
            else if (leftTrigger)
            {
                ErrorLog.logData("Success: " + resultTrigger + " " + resultLeft + " " + rotationTrigger + " " + leftTrigger + " " + rightTrigger, false);
                // left/right num, trigger/no trigger num, sensitivity num, left/right text, Menu/NoMenu text, Sensitivity Text, Task Time, 
                // Failed Left Revealed, Failed Right Revealed, Failed Left No Reveal, Failed Right No Reveal
                string stageResultData = taskOrderData[taskID].x + "," + taskOrderData[taskID].y + "," + taskOrderData[taskID].z + ","
                    + (resultLeft ? "Left" : "Right") + "," + (resultTrigger ? "Menu" : "No Menu") + ","
                    + ((taskOrderData[taskID].z == 0) ? "Low" : ((taskOrderData[taskID].z == 1) ? "Medium" : "High")) + ","
                    + timeSinceLastState + "," + failureCount[0] + "," + failureCount[1] + "," + failureCount[2] + "," + failureCount[3];
                taskData.logData(stageResultData, true);
                failureCount = new int[] { 0, 0, 0, 0 };
                timeSinceLastState = 0;
                setTaskState(TaskState.ReturnToCentre);
            }
            else if (rightTrigger)
            {
                // Looked too far in wrong way
                ErrorLog.logData("Looked too far in wrong way",false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[3]++;
            }
            // else no result found yet
        }
        // Show menu right side
        else if (resultTrigger && !resultLeft)
        {
            if (rotationTrigger == PeripheryBehaviour.RotationResult.Left)
            {
                // Looked wrong way to trigger menu
                ErrorLog.logData("Looked wrong way to trigger menu",false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[0]++;
            }
            else if (rotationTrigger == PeripheryBehaviour.RotationResult.Right)
            {
                ErrorLog.logData("Success: " + resultTrigger + " " + resultLeft + " " + rotationTrigger + " " + leftTrigger + " " + rightTrigger, false);
                // left/right num, trigger/no trigger num, sensitivity num, left/right text, Menu/NoMenu text, Sensitivity Text, Task Time, 
                // Failed Left Revealed, Failed Right Revealed, Failed Left No Reveal, Failed Right No Reveal
                string stageResultData = taskOrderData[taskID].x + "," + taskOrderData[taskID].y + "," + taskOrderData[taskID].z + ","
                    + (resultLeft ? "Left" : "Right") + "," + (resultTrigger ? "Menu" : "No Menu") + ","
                    + ((taskOrderData[taskID].z == 0) ? "Low" : ((taskOrderData[taskID].z == 1) ? "Medium" : "High")) + ","
                    + timeSinceLastState + "," + failureCount[0] + "," + failureCount[1] + "," + failureCount[2] + "," + failureCount[3];
                taskData.logData(stageResultData, true);
                failureCount = new int[] { 0, 0, 0, 0 };
                timeSinceLastState = 0;
                setTaskState(TaskState.ReturnToCentre);
            }
            else if (leftTrigger)
            {
                // Looked too far in wrong way
                ErrorLog.logData("Looked too far in wrong way",false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[2]++;
            }
            else if (rightTrigger)
            {
                // Too far with no trigger
                ErrorLog.logData("Too far with no trigger",false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[3]++;
            }
            // else no result found yet
        }
        // Show no menu right side
        else if (!resultTrigger && !resultLeft)
        {
            if (rotationTrigger == PeripheryBehaviour.RotationResult.Left)
            {
                // Looked wrong way to trigger menu
                ErrorLog.logData("Looked wrong way to trigger menu",false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[0]++;
            }
            else if (rotationTrigger == PeripheryBehaviour.RotationResult.Right)
            {
                // Turned too fast
                ErrorLog.logData("Turned too fast",false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[1]++;
            }
            else if (leftTrigger)
            {
                // Looked too far in wrong way
                ErrorLog.logData("Looked too far in wrong way",false);
                setTaskState(TaskState.ReturnToCentreFailedTask);
                failureCount[2]++;
            }
            else if (rightTrigger)
            {
                ErrorLog.logData("Success: " + resultTrigger + " " + resultLeft + " " + rotationTrigger + " " + leftTrigger + " " + rightTrigger, false);
                // left/right num, trigger/no trigger num, sensitivity num, left/right text, Menu/NoMenu text, Sensitivity Text, Task Time, 
                // Failed Left Revealed, Failed Right Revealed, Failed Left No Reveal, Failed Right No Reveal
                string stageResultData = taskOrderData[taskID].x + "," + taskOrderData[taskID].y + "," + taskOrderData[taskID].z + ","
                    + (resultLeft ? "Left" : "Right") + "," + (resultTrigger ? "Menu" : "No Menu") + ","
                    + ((taskOrderData[taskID].z == 0) ? "Low" : ((taskOrderData[taskID].z == 1) ? "Medium" : "High")) + ","
                    + timeSinceLastState + "," + failureCount[0] + "," + failureCount[1] + "," + failureCount[2] + "," + failureCount[3];
                taskData.logData(stageResultData, true);
                failureCount = new int[] { 0, 0, 0, 0 };
                timeSinceLastState = 0;
                setTaskState(TaskState.ReturnToCentre);
            }
            // else no result found yet
        }
    }

    private void setTaskState(TaskState newTaskState)
    {
        taskState = newTaskState;
        if (taskState == TaskState.AllTasksComplete)
        {
            taskState = TaskState.AllTasksComplete;
            menuLeft.hideMenu();
            menuRight.hideMenu();
            menuMiddle.hideMenu();
            peripheryBehaviour.leftMenuDef = "";
            peripheryBehaviour.rightMenuDef = "";

            experimentState.nextState();
        }
        else if (taskState == TaskState.ReturnToCentre)
        {
            menuLeft.setButtonTitle("Return To Centre");
            menuRight.setButtonTitle("Return To Centre");
            menuMiddle.setButtonTitle("Place Cursor Here");
            peripheryBehaviour.leftMenuDef = "";
            peripheryBehaviour.rightMenuDef = "";
        }
        else if (taskState == TaskState.ReturnToCentreFailedTask)
        {
            menuLeft.setButtonTitle("Return To Centre (Try Again)");
            menuRight.setButtonTitle("Return To Centre (Try Again)");
            menuMiddle.setButtonTitle("Place Cursor Here (To Try Again)");
            peripheryBehaviour.leftMenuDef = "";
            peripheryBehaviour.rightMenuDef = "";
        }
        else if (taskState == TaskState.CompletingTask)
        {
            if (nextTask())
            {
                peripheryBehaviour.leftMenuDef = ExperimentState.SENSITIVITYTESTMENULEFT;
                peripheryBehaviour.rightMenuDef = ExperimentState.SENSITIVITYTESTMENURIGHT;

                menuLeft.setButtonTitle("Place Cursor Here");
                menuRight.setButtonTitle("Place Cursor Here");

                // Reference Note:
                // taskOrderData[taskID].x = right/left, 0 = left, 1 = right
                // taskOrderData[taskID].y = trigger state; 0 = no, 1 = yes
                // taskOrderData[taskID].z = sensitivity; 0 = low, 1 = medium, 2 = high
                string taskMessage = ((taskOrderData[taskID].x == 1) ? "Look Right " : "Look Left ")
                    + ((taskOrderData[taskID].y == 1) ? "Reveal Menu" : "Don't Reveal Menu")
                    + ((taskOrderData[taskID].z == 0) ? " (Low Sensitivity)" : ((taskOrderData[taskID].z == 1) ? " (Medium Sensitvity)" : " (High Sensitvity)"));
                menuMiddle.setButtonTitle(taskMessage);
            }
        }
    }

    private bool nextTask()
    {
        taskID++;
        menuMiddle.setMenuTitle("Menu Sensitivity Test (" + taskID + "/24 complete)");
        if (taskID >= taskOrderData.Count)
        {
            peripheryBehaviour.setSensitivity(PeripheryBehaviour.Sensitivity.Medium);
            setTaskState(TaskState.AllTasksComplete);
            return false;
        }

        // set the sensitivity setting
        peripheryBehaviour.setSensitivity((PeripheryBehaviour.Sensitivity)(taskOrderData[taskID].z));
        return true;
    }

    private void generateTaskOrder()
    {
        taskOrderData = new List<Vector3>();
        Vector3 temp;

        // Generate ordered data set to ensure two of every combination exists.
        for (int i = 0; i < 2; i++) // left/right
        {
            for(int j = 0; j < 2; j++) // trigger state (activate or don't)
            {
                for (int k = 0; k < 3; k++) // sensitivity
                {
                    temp = new Vector3(i, j, k);
                    taskOrderData.Add(temp);
                    taskOrderData.Add(temp);
                }
            }
        }
        
        // Randomly sort all the data
        int n = taskOrderData.Count;
        for (int i = 0; i < n; i++)
        {
            // NextDouble returns a random number between 0 and 1.
            // ... It is equivalent to Math.random() in Java.
            int r = i + (int)(gen.NextDouble() * (n - i));
            temp = taskOrderData[r];
            taskOrderData[r] = taskOrderData[i];
            taskOrderData[i] = temp;
        }
    }

    public static List<GameObject> Fisher_Yates_CardDeck_Shuffle(List<GameObject> aList)
    {

        System.Random _random = new System.Random();

        GameObject myGO;

        int n = aList.Count;
        for (int i = 0; i < n; i++)
        {
            // NextDouble returns a random number between 0 and 1.
            // ... It is equivalent to Math.random() in Java.
            int r = i + (int)(_random.NextDouble() * (n - i));
            myGO = aList[r];
            aList[r] = aList[i];
            aList[i] = myGO;
        }

        return aList;
    }

    public string serialiseSensitivityData(List<Vector3> taskOrder)
    {
        string result = "";
        for (int i = 0; i < taskOrder.Count; i++)
        {
            result += taskOrder[i].x + "," + taskOrder[i].y + "," + taskOrder[i].z;
            if (i < taskOrder.Count - 1)
            {
                result += ",";
            }
        }

        return result;
    }

    public List<Vector3> getSerialisedSensitivityData(string data)
    {
        string[] splitData = data.Split(',');
        if (splitData.Length == 0)
        {
            return null;
        }

        try
        {
            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < splitData.Length; i += 3)
            {
                result.Add(new Vector3(float.Parse(splitData[i]), float.Parse(splitData[i + 1]), float.Parse(splitData[i + 2])));
            }
            return result;
        }
        catch (System.Exception)
        {
            return null;
        }
    }
}
