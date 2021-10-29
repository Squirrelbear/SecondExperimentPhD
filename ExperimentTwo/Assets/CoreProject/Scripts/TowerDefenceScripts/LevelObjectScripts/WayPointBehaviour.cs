using UnityEngine;
using System.Collections;

public class WayPointBehaviour : MonoBehaviour {

    public GameObject nextWaypoint;

	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Renderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
