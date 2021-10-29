using UnityEngine;
using System.Collections;

public class PeripheryHoverTask : MonoBehaviour {

    ExperimentState experimentState;

    ExperimentState.GameState lastGameState = ExperimentState.GameState.PeripheryHover;

    public GameObject left, right;

	// Use this for initialization
	void Start () {
        experimentState = Camera.main.GetComponent<ExperimentState>();
	}
	
	// Update is called once per frame
	void Update () {
	    if(experimentState.gameState == ExperimentState.GameState.PeripheryHover_Test && lastGameState == ExperimentState.GameState.PeripheryHover_Intro01)
        {
            left.SetActive(true);
            right.SetActive(true);
        }
        else if(experimentState.gameState == ExperimentState.GameState.PeripheryHover_Question1 && lastGameState == ExperimentState.GameState.PeripheryHover_Test)
        {
            left.SetActive(false);
            right.SetActive(false);
        }
        lastGameState = experimentState.gameState;
	}
}
