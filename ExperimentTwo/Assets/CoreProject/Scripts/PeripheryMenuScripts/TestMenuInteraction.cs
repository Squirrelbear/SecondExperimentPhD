using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestMenuInteraction : MonoBehaviour {

    int i = 0;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        print("running");

        // get pointer event data, then set current mouse position
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);//Screen.width / 2, Screen.height / 2);//

        // create an empty list of raycast results
        List<RaycastResult> hits = new List<RaycastResult>();

        // ray cast into UI and check for hits
        EventSystem.current.RaycastAll(ped,hits);

        // check any hits to see if any of them are blocking UI elements
        foreach (RaycastResult r in hits) {
            //print(r.gameObject.name);
            if (r.gameObject.name == "Button_A")
            {
                Text t = GameObject.Find("MenuTitle").GetComponent<Text>();
                t.text = "Selected: " + (++i);
            }
        }
	}
}
