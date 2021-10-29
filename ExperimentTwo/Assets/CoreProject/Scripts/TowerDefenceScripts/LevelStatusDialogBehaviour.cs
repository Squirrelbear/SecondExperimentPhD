using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelStatusDialogBehaviour : MonoBehaviour {

    public enum DisplayState { WaitForWave, WaitForTime, ShowWaveStatus, WaitForInstruction };


    public float countdownClock = 0;

    public bool menuActive = false;
    public GameObject titleObject;
    public GameObject titleGoldObject;
    public GameObject titleBaseHealthObject;

    // cached references
    public Text menuTextTitle;
    public Text menuGoldTitle;
    public Text menuBaseHealthTitle;
    public Camera cam;

    public DisplayState displayState;
    public AISpawner spawner;
    public LevelManager levelManager;
    public EndPointBehaviour endPoint;

    private bool firstUpdate = true;

    // Use this for initialization
    void Start()
    {
        menuTextTitle = titleObject.GetComponent<Text>();
        menuGoldTitle = titleGoldObject.GetComponent<Text>();
        menuBaseHealthTitle = titleBaseHealthObject.GetComponent<Text>();

        cam = Camera.main;
        setDisplayState(DisplayState.WaitForInstruction, 0);
    }

    // Update is called once per frame
    public void update(float deltaTime)
    {
        if(firstUpdate)
        {
            spawner = GameObject.FindGameObjectWithTag("AISpawner").GetComponent<AISpawner>();
            levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
            endPoint = GameObject.Find("EndPoint").GetComponent<EndPointBehaviour>();
            firstUpdate = false;
        }

        if (!menuActive)
        {
            gameObject.GetComponent<Canvas>().enabled = false;
            return;
        }
        else if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.position - transform.position) * Quaternion.Euler(0, 180, 0);
        }

        if (displayState == DisplayState.WaitForTime)
        {
            countdownClock -= deltaTime;
            if (countdownClock <= 0)
            {
                countdownClock = 0;
            }
            menuTextTitle.text = "Time till Next Wave: " + (int)countdownClock + "s";
        } 
        else if(displayState == DisplayState.ShowWaveStatus)
        {
            menuTextTitle.text = spawner.getWaveProgress();
        }

        menuGoldTitle.text = ""+levelManager.currency;
        menuBaseHealthTitle.text = (int)(endPoint.unitHealth * 100 / endPoint.maxUnitHealth) + "%";
    }

    public void setDisplayState(DisplayState displayState, float var)
    {
        this.displayState = displayState;
        switch(displayState)
        {
            case DisplayState.WaitForWave:
                menuTextTitle.text = "Waiting for Next Wave";
                break;
            case DisplayState.WaitForTime:
                countdownClock = var;
                menuTextTitle.text = "Time till Next Wave: " + (int)var + "s";
                break;
            case DisplayState.WaitForInstruction:
                menuTextTitle.text = "Please Wait for Further Instruction";
                break;
            case DisplayState.ShowWaveStatus:
                menuTextTitle.text = "Waiting for Next Wave";
                break;
        }
    }

    public void showMenu()
    {
        gameObject.GetComponent<Canvas>().enabled = true;
        menuActive = true;
    }

    public void hideMenu()
    {
        menuActive = false;
    }
}
