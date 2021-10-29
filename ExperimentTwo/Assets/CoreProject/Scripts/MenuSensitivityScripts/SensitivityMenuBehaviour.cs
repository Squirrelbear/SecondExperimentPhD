using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SensitivityMenuBehaviour : MonoBehaviour {

    //int i = 0;

    public bool menuActive = false;
    public bool mouseDebugMode = true;

    public GameObject selectionTarget;
    public float timeToSelect = 1.5f;
    public float selectionTimeProgress = 0;
    public float timeSinceLastInteract = 0;
    public int menuAction = 0;

    public GameObject titleObject;
    public GameObject[] buttonObjects;

    // cached references
    public Text menuTextTitle;
    public Button[] buttons;
    public Text[] buttonTexts;
    public int[] buttonResultCode;

    //string exampleMenuDef = "Some Title,First,1,Second,2,Third,3,Fourth,4";

    public Camera cam;

    // Use this for initialization
    void Start()
    {
        buttons = new Button[buttonObjects.Length];
        buttonTexts = new Text[buttonObjects.Length];
        buttonResultCode = new int[buttonObjects.Length];
        for (int i = 0; i < buttonObjects.Length; i++)
        {
            buttonTexts[i] = buttonObjects[i].GetComponentInChildren<Text>();
            buttons[i] = buttonObjects[i].GetComponent<Button>();
            buttonResultCode[i] = -1;
        }
        menuTextTitle = titleObject.GetComponent<Text>();

        cam = Camera.main;
    }

    // Update is called once per frame
    public void update(float deltaTime)
    {
        menuAction = 0;
        if (!menuActive)
        {
            gameObject.GetComponent<Canvas>().enabled = false;
            return;
        }
        else if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.position - transform.position) * Quaternion.Euler(0, 180, 0);
        }

        // get pointer event data, then set current mouse position
        PointerEventData ped = new PointerEventData(EventSystem.current);
        mouseDebugMode = cam.GetComponent<CameraBehaviour>().clickDebugMode;
        if (mouseDebugMode)
        {
            ped.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
        else
        {
            ped.position = new Vector2(Screen.width / 2, Screen.height / 2);
        }

        // create an empty list of raycast results
        List<RaycastResult> hits = new List<RaycastResult>();

        // ray cast into UI and check for hits
        EventSystem.current.RaycastAll(ped, hits);

        bool targetFound = false;

        // check any hits to see if any of them are blocking UI elements
        foreach (RaycastResult r in hits)
        {
            for (int i = 0; i < buttonObjects.Length; i++)
            {
                if (r.gameObject.name == buttonObjects[i].name)
                {
                    targetFound = true;

                    if (selectionTarget != r.gameObject)
                    {
                        selectionTarget = r.gameObject;
                        selectionTimeProgress = 0;// Time.timeSinceLevelLoad + timeToSelect;
                    }
                    else
                    {
                        selectionTimeProgress += deltaTime;
                        if (selectionTimeProgress >= timeToSelect)//Time.timeSinceLevelLoad >= selectionTimeProgress)
                        {
                            menuAction = buttonResultCode[i];

                            selectionTarget = null;
                            selectionTimeProgress = 0;
                            //menuActive = false;
                            //menuTitle.text = ""+buttonResultCode[i];//"Selection Complete";
                            //applyMenu(exampleMenuDef);
                        }
                        else
                        {
                            //float percentRemaining = 100 - (selectionEndTime - Time.timeSinceLevelLoad) / timeToSelect * 100;
                            
                            //float percentRemaining = selectionTimeProgress / timeToSelect * 100;
                            //menuTextTitle.text = r.gameObject.name + " " + percentRemaining + "%";
                        }
                    }

                    break;
                }
            }

            if (targetFound)
            {
                break;
            }
        }
    }

    public void showMenu()
    {
        gameObject.GetComponent<Canvas>().enabled = true;
        menuActive = true;
        selectionTarget = null;
        selectionTimeProgress = 0;
    }

    public void setMenuTitle(string menuTitle)
    {
        menuTextTitle.text = menuTitle;
    }

    public void setButtonTitle(string buttonTitle)
    {
        buttonTexts[0].text = buttonTitle;
    }

    public void hideMenu()
    {
        menuActive = false;
    }
}
