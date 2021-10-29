using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PeripheryBehaviour : MonoBehaviour {

    public enum Sensitivity { Low, Medium, High };
    public enum RotationResult { None = 0, Up = 1, Down = 2, Left = 3, Right = 4 };

    public struct SensitivityConfig
    {
        public float thresholdAngle;
        public float thresholdZAngle;
        public float thresholdOtherAngle;
        public float thresholdTime;

        public SensitivityConfig(float thresholdAngle, float thresholdZAngle, float thresholdOtherAngle, float thresholdTime)
        {
            this.thresholdAngle = thresholdAngle;
            this.thresholdZAngle = thresholdZAngle;
            this.thresholdOtherAngle = thresholdOtherAngle;
            this.thresholdTime = thresholdTime;
        }
    }

    public SensitivityConfig lowSensitivity, mediumSensitivity, highSensitivity;

    public Sensitivity curSensitivity;
    public float thresholdAngle = 45;
    public float thresholdZAngle = 30;
    public float thresholdOtherAngle = 20;
    public float thresholdTime = 0.2f;

    public List<Vector4> history;

    public int[] lookCounter = new int[4];

    public float timerBetweenEvents = 2.0f;
    public float timerBetweenEventsNoMenu = 0.1f;
    public float ignoreTimesUntil = 0;

    // Note only public to expose to editor interface
    public GameObject leftNode;
    public GameObject rightNode;
    public GameObject menuObject;
    public MenuBehaviour menuBehaviour;

    public bool showDebug = true;

    public RotationResult curFrameResult = RotationResult.None;

    public string leftMenuDef = "LeftMenu,First,1,Second,2,Third,3,Fourth,4";
    public string rightMenuDef = "RightMenu,First,1,Second,2,Third,3,Fourth,4";

    private float curTime = 0;

    // Logging Variables
    public ExtraDataRecorder extraDataRecorder;
    public ExtraDataRecorder.ExtraDataCollection peripheryEventLog;
    public ExperimentState experimentState;

	// Use this for initialization
	void Start () {
        history = new List<Vector4>();
        leftNode = GameObject.FindGameObjectWithTag("LeftNode");
        rightNode = GameObject.FindGameObjectWithTag("RightNode");
        //menuObject = GameObject.FindGameObjectWithTag("MenuObject");
        if (menuObject != null)
        {
            menuBehaviour = menuObject.GetComponent<MenuBehaviour>();
        }
        //menuObject.GetComponent<Canvas>().enabled = false;
        //menuObject.GetComponent<TestMenuInteraction>().enabled = false;

        lowSensitivity = new SensitivityConfig(25,30,30,0.35f);
        mediumSensitivity = new SensitivityConfig(25, 30, 30, 0.3f);
        highSensitivity = new SensitivityConfig(25, 25, 25, 0.25f);
        setSensitivity(Sensitivity.Medium);

        experimentState = Camera.main.GetComponent<ExperimentState>();
        extraDataRecorder = ExtraDataRecorder.getSingleton();
        peripheryEventLog = extraDataRecorder.getDataCollection("PeripheryEvents");
        peripheryEventLog.logData("Timestamp,GameStateSuper,GameState,RotationResult,ShowMenu,Sensitivity,"
                                 +"Diff_X,Diff_Y,Diff_Z,HistoryCount,History(X;Y;Z;Time;DeltaTime) Showing Oldest To Newest", false);
	}
	
	// Update is called once per frame
	public void update (float deltaTime) {
        updateRotationTrigger(deltaTime);

        updateMenuObjects(deltaTime);
	}

    public void updateMenuObjects(float deltaTime)
    {
        if (curFrameResult == RotationResult.Right && rightMenuDef.Length != 0)
        {
            //menuObject.GetComponent<Canvas>().enabled = true;
            //menuObject.GetComponent<TestMenuInteraction>().enabled = true;
            menuObject.transform.position = rightNode.transform.position;
            menuBehaviour.showMenu(rightMenuDef);
        }
        else if (curFrameResult == RotationResult.Left && leftMenuDef.Length != 0)
        {
            //menuObject.GetComponent<Canvas>().enabled = true;
            //menuObject.GetComponent<TestMenuInteraction>().enabled = true; 
            menuObject.transform.position = leftNode.transform.position;
            menuBehaviour.showMenu(leftMenuDef);
        }

        if (menuBehaviour != null)
        {
            menuBehaviour.update(deltaTime);
        }
    }

    public void updateRotationTrigger(float deltaTime)
    {
        curTime += deltaTime;// Time.realtimeSinceStartup;
        if (curTime < ignoreTimesUntil)
        {
            curFrameResult = RotationResult.None;
            return;
        }

        // Remove all older data stored in list
        clearCache(curTime);

        addCurFrame(curTime);

        // Require at least 5 data points
        if (history.Count < 5)
        {
            curFrameResult = RotationResult.None;
            return;
        }

        // X is vertical vision, Y is horizontal vision
        // Assumption, user can't physically rotate themselves 180 degrees in 0.2 seconds
        // To correct the issue of 0 and 360 being considered the same and data triggering erratically a continuous
        // line is assumed to go from [0]-180 to [0]+180 values outside this range are normalised to correct their range value.

        Vector3 difference = getMaxDifference(history);
        curFrameResult = detectResultFromDifference(difference);

        if (curFrameResult != RotationResult.None)
        {
            //if (showDebug)
            //{
            ErrorLog.logData("Looking " + curFrameResult.ToString() + ". Difference: (" + difference.x + ", " + difference.y + ", " + difference.z + ")", showDebug);
            generateLogData(difference);
                    //+ " : " + history[0].y + " " + history[history.Count - 1].y);
            //}
            lookCounter[(int)curFrameResult - 1]++;
            if((curFrameResult == RotationResult.Left && leftMenuDef.Length == 0) || (curFrameResult == RotationResult.Right && rightMenuDef.Length == 0))
            {
                // No menu will actually be triggered, so don't delay trigger for as long a time.
                ignoreTimesUntil = curTime + timerBetweenEventsNoMenu; // Time.realtimeSinceStartup + timerBetweenEvents
            }
            else 
            {
                ignoreTimesUntil = curTime + timerBetweenEvents; // Time.realtimeSinceStartup + timerBetweenEvents
            }
            history.Clear();
        }
    }

    public void addCurFrame(float curTime)
    {
        Vector4 latest = new Vector4(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z, curTime);
        // Add the latest frame data
        history.Add(latest);
    }

    public void clearCache(float curTime)
    {
        float timesToIgnore = curTime - thresholdTime;
        // Empty out times that are longer than the threshold
        while (history.Count > 0 && history[0].w < timesToIgnore)
        {
            history.RemoveAt(0);
        }
    }

    public Vector3 getMaxDifference(List<Vector4> history)
    {
        Vector3 max = new Vector3(history[0].x, history[0].y, history[0].z);
        Vector3 min = new Vector3(history[0].x, history[0].y, history[0].z);
        for (int i = 1; i < history.Count; i++)
        {
            max.x = Math.Max(fixRotation(history[i].x, history[0].x), max.x);
            max.y = Math.Max(fixRotation(history[i].y, history[0].y), max.y);
            max.z = Math.Max(fixRotation(history[i].z, history[0].z), max.z);
            min.x = Math.Min(fixRotation(history[i].x, history[0].x), min.x);
            min.y = Math.Min(fixRotation(history[i].y, history[0].y), min.y);
            min.z = Math.Min(fixRotation(history[i].z, history[0].z), min.z);
        }

        return max - min;
    }

    public RotationResult detectResultFromDifference(Vector3 difference)
    {
        // Test the X and Y axis for being larger than the threshold angle. 
        // If one of them is, than also check that the other two angles are within smaller limits.
        // If either non-primary angle is larger than the threshold than there is no rotation result.

        // Once a rotation event on an X or Y axis has been detected the direction of rotation is assumed based on the 
        // direction relative to the first element (oldest element)

        if (difference.x > thresholdAngle && difference.y < thresholdOtherAngle && difference.z < thresholdZAngle)
        {
            if (history[0].x > fixRotation(history[history.Count - 1].x, history[0].x))
            {
                return RotationResult.Up;
            }
            else
            {
                return RotationResult.Down;
            }
        }
        else if (difference.y > thresholdAngle && difference.x < thresholdOtherAngle && difference.z < thresholdZAngle)
        {
            if (history[0].y > fixRotation(history[history.Count - 1].y, history[0].y))
            {
                return RotationResult.Left;
            }
            else
            {
                
                return RotationResult.Right;
            }
        }

        return RotationResult.None;
    }

    public void ignoreEventsForNext(float time)
    {
        if (curTime + time > ignoreTimesUntil)
        {
            ignoreTimesUntil = curTime + time;
        }
    }

    public void setSensitivity(Sensitivity newSensitivity)
    {
        curSensitivity = newSensitivity;
        switch(newSensitivity)
        {
            case Sensitivity.Low:
                setSensitivity(lowSensitivity);
                break;
            case Sensitivity.Medium:
                setSensitivity(mediumSensitivity);
                break;
            case Sensitivity.High:
                setSensitivity(highSensitivity);
                break;
        }
    }

    public void setSensitivity(SensitivityConfig config)
    {
        this.thresholdAngle = config.thresholdAngle;
        this.thresholdZAngle = config.thresholdZAngle;
        this.thresholdOtherAngle = config.thresholdOtherAngle;
        this.thresholdTime = config.thresholdTime;
        ignoreEventsForNext(thresholdTime);
    }

    /*
    public RotationResult detectResultFromDifference(Vector3 difference)
    {
        // This code won't detect the direction of rotation 
        // Perhaps store the array numbering on a per rotation basis and use this to test direction. 
        if (difference.x > thresholdAngle && difference.y < thresholdOtherAngle && difference.z < thresholdZAngle)
        {
            //print("Difference: (" + difference.x + ", " + difference.y + ", " + difference.z + ")");
            //print("Trigger on x axis rotation detected!");
            if (history[0].x > history[history.Count - 1].x)
            {
                //print("Looking up." + "Difference: (" + difference.x + ", " + difference.y + ", " + difference.z + ")");
                //lookCounter[0]++;
                return RotationResult.Up;
            }
            else
            {
                //print("Looking down." + "Difference: (" + difference.x + ", " + difference.y + ", " + difference.z + ")");
                lookCounter[1]++;
                return RotationResult.Down;
            }
            //ignoreTimesUntil = Time.realtimeSinceStartup + timerBetweenEvents;
        }
        else if (difference.y > thresholdAngle && difference.x < thresholdOtherAngle && difference.z < thresholdZAngle)
        {
            //print("Difference: (" + difference.x + ", " + difference.y + ", " + difference.z + ")");
            //print("Trigger on y axis rotation detected!");
            if (history[0].y > history[history.Count - 1].y)
            {
                //print("Looking right." + "Difference: (" + difference.x + ", " + difference.y + ", " + difference.z + ")");
                //lookCounter[2]++;
                return RotationResult.Right;
            }
            else
            {
                //print("Looking left." + "Difference: (" + difference.x + ", " + difference.y + ", " + difference.z + ")");
                //lookCounter[3]++;
                return RotationResult.Left;
            }
            //ignoreTimesUntil = Time.realtimeSinceStartup + timerBetweenEvents;
        }

        return RotationResult.None;
        //else if (difference.z > thresholdAngle && difference.x < difference.z / 2 && difference.y < difference.z / 2)
        //{
        //    print("Trigger on z axis rotation detected!");
        //}

        // 
    }*/

    public float fixRotation(float rotation, float normalPoint)
    {
        if (normalPoint + 180 < rotation)
        {
            return rotation - 360;
        }
        else if (normalPoint - 180 > rotation)
        {
            return rotation + 360;
        }
        else
        {
            return rotation;
        }
    }

    private void generateLogData(Vector3 difference)
    {
        bool showMenu = (curFrameResult == RotationResult.Left && leftMenuDef.Length != 0) || (curFrameResult == RotationResult.Right && rightMenuDef.Length != 0);
        string result = experimentState.gameSuperState + "," + experimentState.gameState + "," + curFrameResult + "," + showMenu + "," + curSensitivity + ","
                         + difference.x + "," + difference.y + "," + difference.z + "," + history.Count;
        
        float previousTime = history[0].w;
        //History(X,Y,Z,Time,DeltaTime);
        foreach(Vector4 v in history)
        {
            result += "," + v.x + "," + v.y + "," + v.z + "," + v.w + "," + (v.w - previousTime);
            previousTime = v.w;
        }

        peripheryEventLog.logData(result, true);
    }
}
