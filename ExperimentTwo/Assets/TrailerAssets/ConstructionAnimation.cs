using UnityEngine;
using System.Collections;

public class ConstructionAnimation : MonoBehaviour {

    public float nextTimer = 10;
    int modeID = -1;

    public GameObject baseNode;
    public GameObject tower;
    public GameObject[] blocks;

	// Use this for initialization
	void Start () {
	
	} 
	
	// Update is called once per frame
	void Update () {
	    nextTimer -= Time.deltaTime;
        if(nextTimer <= 0) {
            modeID++;
            if (modeID < blocks.Length)
            {
                blocks[modeID].SetActive(true);
            }
            else if (modeID == blocks.Length)
            {
                for(int i = 0; i < blocks.Length; i++)
                {
                    blocks[i].SetActive(false);
                }
                tower.SetActive(true);
            }
            nextTimer = 0.6f;
        }
	}
}
