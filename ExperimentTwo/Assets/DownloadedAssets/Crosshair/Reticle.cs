using UnityEngine;
using System.Collections;

public class Reticle : MonoBehaviour {
	public Camera CameraFacing;
	private Vector3 originalScale;
    public Renderer rend;

    public Color colorStart;
    public Color colorEnd = Color.green;
    //public float duration = 1.0F;
    public float progress;
    public float lastUpdate;

	// Use this for initialization
	void Start () {
		originalScale = transform.localScale;
        rend = GetComponent<Renderer>();
        colorStart = new Color(1,0.75f,0,1);
        lastUpdate = 0;
        progress = 0;
	}
	
	// Update is called once per frame
	void Update () {
        CameraFacing = Camera.main;
        
        RaycastHit hit;
		float distance;
		if (Physics.Raycast (new Ray (CameraFacing.transform.position,
		                             CameraFacing.transform.rotation * Vector3.forward),
		                     out hit)) {
			distance = hit.distance;
		} else {
			distance = CameraFacing.farClipPlane * 0.95f;
		}
        distance = 2;
		transform.position = CameraFacing.transform.position +
			CameraFacing.transform.rotation * Vector3.forward * distance;
		transform.LookAt (CameraFacing.transform.position);
		transform.Rotate (0.0f, 180.0f, 0.0f);
		if (distance < 10.0f) {
			distance *= 1 + 5*Mathf.Exp (-distance);
		}
		transform.localScale = originalScale * distance;

        // reset the progress on the colour swap if there is not actually any interaction occuring.
        if (Time.timeSinceLevelLoad - lastUpdate > 0.3)
        {
            progress = 0;
        }

        //float lerp = Mathf.PingPong(Time.time, duration) / duration;
        rend.material.color = Color.Lerp(colorStart, colorEnd, progress);
        //print(rend.material.color);
	}

    public void updateCursor(float progress)
    {
        this.progress = progress;
        lastUpdate = Time.timeSinceLevelLoad;
    }
}
