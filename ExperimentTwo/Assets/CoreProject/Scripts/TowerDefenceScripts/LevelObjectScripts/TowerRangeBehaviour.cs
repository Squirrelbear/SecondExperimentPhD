using UnityEngine;
using System.Collections;

public class TowerRangeBehaviour : MonoBehaviour {

    private Material mat;
    private bool increasing = false;
    private float rangeAmount = 5;

	// Use this for initialization
	void Start () {
        mat = GetComponentInChildren<MeshRenderer>().material;
        Color newColor2 = mat.color;
        newColor2.a = 0.4f;
        mat.color = newColor2;
	}
	
	// Update is called once per frame
	void Update () {
        Color newColor2 = mat.color;
        if (increasing)
        {
            newColor2.a += Time.deltaTime/2;
        }
        else
        {
            newColor2.a -= Time.deltaTime/2;
        }

        if (newColor2.a < 0.2)
        {
            newColor2.a = 0.2f;
            increasing = true;
        }
        else if (newColor2.a > 0.4)
        {
            newColor2.a = 0.4f;
            increasing = false;
        }
        mat.color = newColor2;
	}

    public void setRange(float newValue)
    {
        if (rangeAmount == newValue)
        {
            return;
        }

        rangeAmount = newValue;
        //print("Setting range to: " + rangeAmount);
        transform.localScale = new Vector3(newValue, 0.33f, newValue);
    }
}
