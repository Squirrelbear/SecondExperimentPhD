using UnityEngine;
using System.Collections;

public class CameraPanTowers : MonoBehaviour {

    public float endZAmount;

    public float endZ;
    public float panSpeed = 2;

    public GameObject baseNode;
    public GameObject tower;
    public GameObject[] blocks;

    // Use this for initialization
    void Start()
    {
        endZ = transform.position.z + endZAmount;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.z < endZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + panSpeed * Time.deltaTime);
        }
    }
}
