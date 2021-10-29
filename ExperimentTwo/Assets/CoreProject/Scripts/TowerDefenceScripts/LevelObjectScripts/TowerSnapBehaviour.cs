using UnityEngine;
using System.Collections;

public class TowerSnapBehaviour : MonoBehaviour {

    public Vector3 snapPoint;
    public GameObject associatedTower;

    private const float actualTowerYPos = 1.59f;

	// Use this for initialization
	void Start () {
        snapPoint = new Vector3(transform.position.x, transform.position.y+actualTowerYPos, transform.position.z);
        // deprecated y position: actualTowerYPos
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public bool isAvailable()
    {
        return associatedTower == null;
    }
}
