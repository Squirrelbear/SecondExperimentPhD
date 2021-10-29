using UnityEngine;
using System.Collections;

public class CameraSnapBehaviour : MonoBehaviour {

    public CameraBehaviour camRef = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (camRef == null)
        {
            camRef = Camera.main.GetComponent<CameraBehaviour>();
        }
        else
        {
            if (camRef.camSnapTemp == this.gameObject)
            {
                transform.localScale = new Vector3(1.5f, 1.5f, 1.5f) * (1 + camRef.camSnapTimer / 1.5f);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
	}
}
